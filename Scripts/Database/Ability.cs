using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability
{
    public readonly int id;
    public readonly string identifier;
    public readonly int generationID;
    public readonly bool isMainSeries;
    public readonly string name;
    public readonly string description;
    public readonly string effectShort;
    public readonly string effectLong;

    public Ability(int id, string identifier, int generationID, bool isMainSeries, string name, string effectShort, string effectLong, string description)
    {
        this.id = id;
        this.identifier = identifier;
        this.generationID = generationID;
        this.isMainSeries = isMainSeries;
        this.name = name;
        this.effectShort = effectShort;
        this.effectLong = effectLong;
        this.description = description;
    }
    public string FullString()
    {
        return
            $"{this.name} with effect {this.effectShort}. Introduced in {this.generationID}. Description {this.description}";
    }

    public override string ToString()
    {
        return this.name;
    }
}
