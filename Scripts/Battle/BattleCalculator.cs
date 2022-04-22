using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Effectiveness
{
    Regular,
    Ineffective,
    SuperEffective,
    NotEffective,
    StatusFail,
    Miss,
    NothingHappened,
    Asleep,
    Paralyzed,
    Charmed
}
public static class BattleCalculator
{
    
    //Calcuate the damage based on the attacker and the target
    public static (int, Effectiveness, bool) CalculateDamage(BattlePokemon attacker, BattlePokemon target, Move move, Weather weather)
    {
        float level = attacker.basePartyPokemon.GetLevel();
        float power = move.power;
        float attack = 0f;
        float defense = 0f;
        switch (move.damageClass)
        {
            case DamageClass.Status:
                return (0, Effectiveness.Regular, false);
            case DamageClass.Physical:
                attack = attacker.basePartyPokemon.GetStatTuple(2).actual*attacker.GetStatWithMultipliers(2);
                defense = target.basePartyPokemon.GetStatTuple(3).actual*target.GetStatWithMultipliers(3);
                break;
            case DamageClass.Special:
                attack = attacker.basePartyPokemon.GetStatTuple(4).actual*attacker.GetStatWithMultipliers(4);
                defense = target.basePartyPokemon.GetStatTuple(5).actual*target.GetStatWithMultipliers(5);
                break;
        }

        //get weather multiplier
        float targetMultiplier = 1f;
        float weatherMultiplier = 1f;
        PokemonType moveType = GameManager.Instance.registry.types[move.typeID];
        PokemonType attackerType1 = attacker.basePartyPokemon.basePokemon.GetType1();
        PokemonType attackerType2 = attacker.basePartyPokemon.basePokemon.GetType2();
        PokemonType targetType1 = target.basePartyPokemon.basePokemon.GetType1();
        PokemonType targetType2 = target.basePartyPokemon.basePokemon.GetType2();
        if (weather == Weather.Rainy)
        {
            if (moveType.identifier.Equals("water"))
            {
                Debug.Log("water move used in rain");
                weatherMultiplier = 1.5f;
            }
            if (moveType.identifier.Equals("fire"))
            {
                Debug.Log("fire move used in rain");
                weatherMultiplier = 0.5f;
            }
        }
        if (weather == Weather.HarshSun)
        {
            if (moveType.identifier.Equals("water"))
            {
                Debug.Log("water move used in harshsun");
                weatherMultiplier = 0.5f;
            }
            if (moveType.identifier.Equals("fire"))
            {
                Debug.Log("fire move used in harshsun");
                weatherMultiplier = 1.5f;
            }
        }

        //Critical hit calculation
        //Set C to 0.
        int c = 0;
        // If the attacker has Super Luck, add 1 to C.
        if (attacker.basePartyPokemon.ability.Equals("super-luck")) c += 1;

        // If Focus Energy is in effect for attacker, add 2 to C.
        if (attacker.volitileStatuses.Contains(VolitileStatus.FocusEnergy)) c += 2;

        // TODO: If attack has a "good chance for a critical hit", add 1 to C.
        // If attacker’s current species is Chansey and the attacker is holding Lucky Punch, add 2 to C.
        if (attacker.basePartyPokemon.basePokemon.identifier.Equals("chansey") && attacker.IsHoldingItem("lucky-punch")) c += 2;

        // If attacker’s current species is Farfetch’d and the attacker is holding Stick, add 2 to C.
        if (attacker.basePartyPokemon.basePokemon.identifier.Equals("farfetchd") && attacker.IsHoldingItem("stick")) c += 2;

        // If attacker is holding Scope Lens or Razor Claw, add 1 to C.
        if (attacker.IsHoldingItem("scope-lens") || attacker.IsHoldingItem("razor-claw")) c += 1;

        //get critical hit chance and multiplier
        float critChance = GetCritChanceFromStage(c);
        Debug.Log($"Crit stage calculated to be {c} with chance {critChance}.");
        bool isCrit = (UnityEngine.Random.Range(0f, 1f) < critChance);
        float crit = (isCrit) ? 1.5f : 1f;

        //get the random multiplier
        float rand = UnityEngine.Random.Range(0.85f, 1.0f); //flail, future sight, reversal are exempt
        if (move.identifier.Equals("flail") || move.identifier.Equals("future-sight") || move.identifier.Equals("reversal")) rand = 1f;

        //get same-type/attack-bonus multiplier
        float stab = 1f;
        if (attackerType1.id == move.typeID) stab = 1.5f;
        try
        {
            if (attackerType2.id == move.typeID) stab = 1.5f;
        }
        catch (Exception) {}
        if (attacker.basePartyPokemon.ability.identifier.Equals("adaptability") && Mathf.Approximately(stab, 1.5f)) stab = 2f;

        //get multiplier from target type
        Effectiveness eff = Effectiveness.Regular;
        float typeEffectiveness = GameManager.Instance.registry.types[move.typeID].GetDamageFactor(targetType1.id)/100f;
        if (targetType2 != null) typeEffectiveness *= GameManager.Instance.registry.types[move.typeID].GetDamageFactor(targetType2.id)/100f;

        if (typeEffectiveness > 1.1f) eff = Effectiveness.SuperEffective;
        else if (typeEffectiveness < 0.9f) eff = Effectiveness.NotEffective;
        if (typeEffectiveness == 0f) eff = Effectiveness.Ineffective;
        Debug.Log($"Type effectiveness is {eff.ToString()} with multiplier {typeEffectiveness}");

        //get burned attacker multiplier
        //Burn is 0.5 (from Generation III onward) if the attacker is burned, its Ability is not Guts,
        //and the used move is a physical move (other than Facade from Generation VI onward), and 1 otherwise.
        float burn = (
            attacker.basePartyPokemon.status == Status.Burnt &&
            !attacker.basePartyPokemon.ability.identifier.Equals("guts") &&
            (move.damageClass == DamageClass.Physical && !move.identifier.Equals("facade"))) ? 0.5f : 1f;
        
        //TODO: item special cases
        float other = 1f;
        int damage = Mathf.Max(1, (int)((((((2*level/5)+2)*power*(attack/defense))/50)+2)*targetMultiplier*weatherMultiplier*crit*rand*stab*typeEffectiveness*burn*other));
        Debug.Log($"Calculated damage: {damage}");
        return (damage, eff, isCrit);
    }

    public static float GetCritChanceFromStage(int stage)
    {
        if (GameManager.Instance.GENERATION_ID <= 5)
        {
            switch (stage)
            {
                case 0: return 0.0625f;
                case 1: return 0.125f;
                case 2: return 0.25f;
                case 3: return 0.333f;
                default: return 0.5f;
            }
        }
        else
        {
            switch (stage)
            {
                case 0: return 0.0625f;
                case 1: return 0.125f;
                case 2: return 0.5f;
                default: return 1.0f;
            }
        }
    }

    //based on offical logic
    public static float CalculateAccuracy(BattlePokemon attacker, BattlePokemon target, Move move, bool isLast)
    {
        Weather weather = BattleSystemV2.Instance.currentWeather;
        List<FieldEffects> fieldEffects = BattleSystemV2.Instance.currentFieldEffects;
        // TODO: If some effect is true, the attack hits.
        
        if (move.accuracy == -1) return 1f; //special case, will always hit if it is a damaging attack
        int effectiveAccuracyStage = attacker.accuracyStage;
        int effectiveEvasionStage = target.evasionStage;
        int effectiveMoveAccuracy = move.accuracy;
        
        // If the attacker has Simple, multiply its accuracy stat stage by 2.
        // If the target has Simple, multiply its evasiveness stat stage by 2.
        if (attacker.basePartyPokemon.ability.identifier.Equals("simple")) effectiveAccuracyStage *= 2;
        if (target.basePartyPokemon.ability.identifier.Equals("simple")) effectiveEvasionStage *= 2;

        // If the attacker has Unaware, the target’s evasiveness stat stage is set to 0.
        // If the target has Unaware, the attacker’s accuracy stat stage is set to 0.
        if (attacker.basePartyPokemon.ability.identifier.Equals("unaware")) effectiveEvasionStage = 0;
        if (target.basePartyPokemon.ability.identifier.Equals("unware")) effectiveAccuracyStage = 0;

        //TODO: If Foresight, Miracle Eye, or both is in effect for the target, the target’s evasiveness stat stage is set to 0 if it is greater than 0.

        //The target’s evasiveness stat stage is subtracted from the attacker’s accuracy stat stage.
        //The accuracy stat stage is adjusted so it is neither less than -6 nor greater than 6.
        float stageModifier = 1f;
        int stage = Mathf.Clamp(effectiveEvasionStage - effectiveAccuracyStage, -6, 6);
        if (GameManager.Instance.GENERATION_ID <= 5)
        {
            switch (stage)
            {
                case -6: 
                    stageModifier = 33/100;
                    break;
                case -5:
                    stageModifier = 36/100;
                    break;
                case -4:
                    stageModifier = 43/100;
                    break;
                case -3:
                    stageModifier = 50/100;
                    break;
                case -2:
                    stageModifier = 60/100;
                    break;
                case -1:
                    stageModifier = 75/100;
                    break;
                case 0:
                    stageModifier = 1f;
                    break;
                case 1:
                    stageModifier = 133/100;
                    break;
                case 2:
                    stageModifier = 166/100;
                    break;
                case 3:
                    stageModifier = 200/100;
                    break;
                case 4:
                    stageModifier = 250/100;
                    break;
                case 5:
                    stageModifier = 266/100;
                    break;
                case 6:
                    stageModifier = 300/100;
                    break;
            }
        }
        else
        {
            if (stage > 0) stageModifier = (3+stage)/3;
            else if (stage < 0) stageModifier = 3/(Mathf.Abs(stage)+3);
            else stageModifier = 1f;
        }

        //During Rain Dance, the accuracy of Thunder is set to 50.
        if (weather == Weather.Rainy && move.identifier.Equals("thunder")) return 1f;
        if (weather == Weather.HarshSun && move.identifier.Equals("thunder")) return 0.5f;

        float modifiers = 1f;
        //If the attacker has Compoundeyes, the accuracy is multiplied by 130/100.
        if (attacker.basePartyPokemon.ability.Equals("compound-eyes")) modifiers *= 1.3f;

        //During Sandstorm, the accuracy is multiplied by 80/100 if the target has Sand Veil.
        if (weather == Weather.Sandstorm && target.basePartyPokemon.ability.Equals("sand-veil")) modifiers *= 0.8f;

        //During Hail, the accuracy is multiplied by 80/100 if the target has Snow Cloak.
        if (weather == Weather.Hail && target.basePartyPokemon.ability.Equals("snow-cloak")) modifiers *= 0.8f;

        //In fog (which is considered a weather condition), the accuracy is multiplied by 6/10.
        if (weather == Weather.Fog) modifiers *= 0.6f;

        //If the attacker has Hustle and the attack is physical, the accuracy is multiplied by 80/100.
        if (attacker.basePartyPokemon.ability.Equals("hustle") && move.damageClass == DamageClass.Physical) modifiers *= 0.8f;

        //If the target has Tangled Feet and is confused, the accuracy is halved.
        if (target.basePartyPokemon.ability.Equals("tangled-feet") && target.volitileStatuses.Contains(VolitileStatus.Confusion)) modifiers *= 0.5f;

        //If the target is holding BrightPowder or Lax Incense, the accuracy is multiplied by 90/100.
        if (target.IsHoldingItem("bright-powder") || target.IsHoldingItem("lax-incense")) modifiers *= 0.9f;

        // If the attacker is holding Wide Lens, the accuracy is multiplied by 110/100.
        if (attacker.IsHoldingItem("wide-lens")) modifiers *= 1.1f;

        // If the attacker is holding Zoom Lens and strikes last this turn, the accuracy is multiplied by 120/100.
        if (attacker.IsHoldingItem("zoom-lens") && isLast) modifiers *= 1.2f;

        // TODO: If the Micle Berry activated for the attacker, the accuracy is multiplied by 120/100, and that effect is removed.
        // If Gravity is in effect, the accuracy is multiplied by 10/6.
        if (fieldEffects.Contains(FieldEffects.Gravity)) modifiers *= 1.6f;

        float accuracy = (effectiveMoveAccuracy/100f)*(stageModifier)*modifiers;
        Debug.Log($"Accuracy calculated to {accuracy} with combined stage of {stage}");
        return accuracy;
    }

    public static float CalculateEffectChance(Move move)
    {
        if (move.effectChance == -1) return 1f;
        else return ((float)move.effectChance/100f);
    }

    public static int GetNumHits(Move move)
    {
        int minHits = move.meta.minHits;
        int maxHits = move.meta.maxHits;
        if (minHits == maxHits) return (minHits == -1) ? 1 : minHits;
        else if (minHits == 2 && maxHits == 5)
        {
            List<(int, float)> numHitWeights = new List<(int, float)>
                {(2, 3f/8f), (3, 3f/8f), (4, 1f/8f), (5, 1f/8f)};
            float randomWeight = UnityEngine.Random.value;
            float currentWeight = 0f;
            foreach ((int, float) pair in numHitWeights)
            {
                currentWeight += pair.Item2;
                if (randomWeight <= currentWeight) return pair.Item1;
            }
        }
        return 1;
    }

    public static int CalculateDrainAmount(Move move, int damage)
    {
        //if negative=recoil. if positive=
        int percentHealthToAdd = move.meta.drain;
        if (percentHealthToAdd == 0) return 0;
        float healthToAttacker = ((float)percentHealthToAdd/100f)*(float)damage;
        return (int)Mathf.Ceil(healthToAttacker);
    }

    public static int CalcuateHealAmount(BattlePokemon attacker, Move move)
    {
        int percentHeal = move.meta.healing;
        float healAmount = ((float)percentHeal/100f)*(float)attacker.basePartyPokemon.GetStatTuple(1).actual;
        return (int)Mathf.Ceil(healAmount);
    }

    public static void ApplyEVs(PartyPokemon winner, PartyPokemon fainted)
    {

        if (winner.GetCurrentHP() <= 0) return;
        winner.AddEVTuple(fainted.basePokemon.GetBaseStats().GetEffortValueTuple());
    }
    public static int CalculateExpGain(PartyPokemon winner, PartyPokemon fainted)
    {
        int numAssisted = 1;

        if (winner.GetCurrentHP() <= 0) return 0;

        float a = (fainted.GetOriginalTrainer() is null) ? 1f : 1.5f; //is wild or is trainer
        int b = fainted.basePokemon.baseExperience; //opponent base experience
        float e = 1f;
        if (winner.GetHeldItem() != null)
        {
            if (winner.GetHeldItem().identifier.Equals("lucky-egg")) e = 1.5f; //if the winner has lucky egg
        }
        float f = 1; //affection
        int L = fainted.GetLevel(); //oppenent level
        int Lp = winner.GetLevel(); //winner's level
        float p = 1; //exp point power
        int s = numAssisted; //assists and experience share users
        float t = 1; //traded pokemon
        float v = 1; //past level of evolve
        float DEBUG_MULTIPLIER = 4f;

        //gain experience
        if (GameManager.Instance.GEN == Generation.Gen3 || GameManager.Instance.GEN == Generation.Gen4 || GameManager.Instance.GEN == Generation.Gen6)
        {
            float gained = DEBUG_MULTIPLIER*(a*t*b*e*L*p*f*v)/(7*s);
            Debug.Log("Experience gained: " + gained);
            return (int)Mathf.Ceil(gained);
        }
        else
        {
            float gained = DEBUG_MULTIPLIER*((a*b*L)/(5*s))*(Mathf.Pow(((2*L)+10)/(L+Lp+10),2.5f)+1)*t*e*p;
            Debug.Log("Experience gained: " + gained);
            return (int)Mathf.Ceil(gained);
        }
    }

    public static int AITrainerGetIndexNextPokemon()
    {
        List<int> validIndex = new List<int>();
        for (int i = 0; i < GameManager.Instance.foeTrainer.party.Count ; i++)
        {
            if (GameManager.Instance.foeTrainer.party[i].UsableInBattle()) validIndex.Add(i);
        }
        Debug.Log($"Foe trainer has {validIndex.Count} valid pokemon left");
        if (validIndex.Count > 0) return validIndex[0];
        else return -1;
    }
}
