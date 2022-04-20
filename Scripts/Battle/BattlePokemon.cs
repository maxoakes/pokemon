using System.Collections.Generic;
using UnityEngine;

public class BattlePokemon : MonoBehaviour
{
    public Team team;
    public int partyIndex;
    private Battle3DHUD hud;
    public int fieldIndex;
    public string displayName;
    public PartyPokemon basePartyPokemon;
    public List<VolitileStatus> volitileStatuses;
    private int attackStage;
    private int defenseStage;
    private int spAttackStage;
    private int spDefenseStage;
    private int speedStage;
    public int accuracyStage;
    public int evasionStage;
    public PokemonType effectiveType1;
    public PokemonType effectiveType2;
    public Item effectiveItem;

    //niche needs that are only used like once
    public int attemptsToRun;
    public int remainingSleepTurns;
    public bool newToField;

    //initialization
    void Awake()
    {
        this.accuracyStage = 0;
        this.evasionStage = 0;
        Debug.Log("Awoke a Battle Pokemon");
    }

    public void SetPokemon(PartyPokemon pokemon, Team team, int partyIndex, int fieldIndex=0)
    {
        this.team = team;
        this.fieldIndex = fieldIndex;
        this.partyIndex = partyIndex;
        this.basePartyPokemon = pokemon;
        this.displayName = this.basePartyPokemon.GetName();
        this.effectiveType1 = this.basePartyPokemon.basePokemon.GetType1();
        this.effectiveType2 = this.basePartyPokemon.basePokemon.GetType2();
        this.effectiveItem = this.basePartyPokemon.GetHeldItem();

        if (pokemon.status == Status.Sleep) remainingSleepTurns = Random.Range(2,5);
        else remainingSleepTurns = -1;
        this.attemptsToRun = 0;
        this.newToField = true;
        Debug.Log($"Filled in Battle Pokemon {this.basePartyPokemon.basePokemon.species.name} at level {this.basePartyPokemon.GetLevel()}");
    }
    public void SetAccompanyingHUD(Battle3DHUD hud)
    {
        this.hud = hud;
    }

    //getters
    public Battle3DHUD GetAccompanyingHUD()
    {
        return this.hud;
    }
    public static float GetStageMultiplier(int stage)
    {
        if (stage > 0) return ((float)(stage+2)/2f);
        else if (stage < 0) return (2f/(float)(Mathf.Abs(stage)+2));
        else return 1f;
    }
    public float GetStatWithMultipliers(int statID)
    {
        switch (statID)
        {
            case 1:
                return 1f;
            case 2:
                return 1f*GetStageMultiplier(attackStage);
            case 3:
                return 1f*GetStageMultiplier(defenseStage);
            case 4:
                return 1f*GetStageMultiplier(spAttackStage);
            case 5:
                return 1f*GetStageMultiplier(spDefenseStage);
            case 6:
                return this.GetSpeedMultiplier()*GetStageMultiplier(speedStage);
            default:
                return 1f;
        }
    }
    public float GetSpeedMultiplier()
    {
        float s = 1f;
        if (this.basePartyPokemon.status == Status.Paralyzed)
        {
            s *= .75f;
        }
        return s;
    }

    public bool IsHoldingItem(string identifier)
    {
        if (this.effectiveItem == null) return false;
        return this.effectiveItem.identifier.Equals(identifier);
    }

    //setters
    public int UpdateAccuracyStage(int change)
    {
        int newValue = Mathf.Clamp(this.accuracyStage + change, -6, 6);
        this.accuracyStage = newValue;
        return this.accuracyStage;
    }

    public int UpdateEvasionStage(int change)
    {
        int newValue = Mathf.Clamp(this.evasionStage + change, -6, 6);
        this.evasionStage = newValue;
        return this.evasionStage;
    }

    public override string ToString()
    {
        return $"{this.basePartyPokemon}:{this.name}";
    }
}

