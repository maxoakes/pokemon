using System.Collections;
using System.Collections.Generic;

public enum DamageClass
{
    Status,
    Physical,
    Special
}
public class Move
{
    public readonly int id;
    public readonly string identifier;
    public readonly int generationID;
    public readonly int typeID;
    public readonly int power;
    public readonly int pp;
    public readonly int accuracy;
    public readonly int priority;
    public readonly int targetID;
    public readonly DamageClass damageClass;
    public readonly MoveEffect effect;
    public readonly MoveMeta meta;
    public readonly int effectChance;
    public readonly string name;
    public readonly string description;
    private List<int> flags;

    public Move(int id, string identifier, int generationID, int typeID, int power,
        int pp, int accuracy, int priority, int targetID, int damageClassID, MoveEffect effect, int effectChance,
        string name, MoveMeta meta, string description)
    {
        this.id = id;
        this.identifier = identifier;
        this.generationID = generationID;
        this.typeID = typeID;
        this.power = power;
        this.pp = pp;
        this.accuracy = accuracy;
        this.priority = priority;
        this.targetID = targetID;
        this.effect = effect;
        this.effectChance = effectChance;
        this.name = name;
        this.meta = meta;
        this.description = description;
        switch (damageClassID)
        {
            case 1:
                this.damageClass = DamageClass.Status;
                break;
            case 2:
                this.damageClass = DamageClass.Physical;
                break;
            case 3:
                this.damageClass = DamageClass.Special;
                break;
        }
        flags = new List<int>();
    }

    public void addFlag(int flag)
    {
        this.flags.Add(flag);
    }

    public List<int> GetFlags()
    {
        return flags;
    }

    public string FullString()
    {
        return
            $@"{this.name} is from gen {this.generationID} with power {this.power}, PP {this.pp}, accuracy {this.accuracy}, effect {this.effect.ToString()} and drain {this.meta.drain}.";
    }

    public override string ToString()
    {
        return this.name;
    }
}
