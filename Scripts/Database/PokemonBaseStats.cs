using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonBaseStats
{
    private int hp;
    private int hpEffort;
    private int attack; //damage class: 2, physical
    private int attackEffort;
    private int defense; //damage class: 2, physical
    private int defenseEffort;
    private int specialAttack; //damage class: 3, special
    private int spAttackEffort;
    private int specialDefense; //damage class: 3, special
    private int spDefenseEffort;
    private int speed;
    private int speedEffort;

    public PokemonBaseStats()
    {
        this.hp = 0;
        this.attack = 0;
        this.defense = 0;
        this.specialAttack = 0;
        this.specialDefense = 0;
        this.speed = 0;

        this.hpEffort = 0;
        this.attackEffort = 0;
        this.defenseEffort = 0;
        this.spAttackEffort = 0;
        this.spDefenseEffort = 0;
        this.speedEffort = 0;
    }

    public void AddStat(int statID, int value, int effort)
    {
        switch (statID)
        {
            case 1:
                this.hp = value;
                this.hpEffort = effort;
                break;
            case 2:
                this.attack = value;
                this.attackEffort = effort;
                break;
            case 3:
                this.defense = value;
                this.defenseEffort = effort;
                break;
            case 4:
                this.specialAttack = value;
                this.spAttackEffort = effort;
                break;
            case 5:
                this.specialDefense = value;
                this.spDefenseEffort = effort;
                break;
            case 6:
                this.speed = value;
                this.speedEffort = effort;
                break;
            default:
                Debug.LogError("Unknown stat added to pokemon: " + statID);
                break;
        }
    }
    public int GetBaseStatByID(int statID)
    {
        switch (statID)
        {
            case 1:
                return this.hp;
            case 2:
                return this.attack;
            case 3:
                return this.defense;
            case 4:
                return this.specialAttack;
            case 5:
                return this.specialDefense;
            case 6:
                return this.speed;
            default:
                Debug.LogError("Unknown stat queried: " + statID);
                return 0;
        }
    }

    public (int hp, int att, int def, int spa, int spd, int spe) GetEffortValueTuple()
    {
        return (this.hpEffort, this.attackEffort, this.defenseEffort, this.spAttackEffort, this.spDefenseEffort, this.speedEffort);
    }

    public override string ToString()
    {
        return ((this.hp, this.attack, this.defense, this.specialAttack, this.specialDefense, this.speed)).ToString();
    }
}
