using System.Collections;
using System.Collections.Generic;

public class MoveMeta
{
    public readonly int moveID;
    public readonly int categoryID;
    public readonly int ailmentID;
    public readonly int minHits;
    public readonly int maxHits;
    public readonly int minTurns;
    public readonly int maxTurns;
    public readonly int drain;
    public readonly int healing;
    public readonly int critRate;
    public readonly int ailmentChance;
    public readonly int flinchChance;
    public readonly int statChance;

    public MoveMeta(int moveID, int categoryID, int ailmentID, int minHits, int maxHits, int minTurns, int maxTurns,
        int drain, int healing, int critRate, int ailmentChance, int flinchChance, int statChance)
    {
        this.moveID = moveID;
        this.categoryID = categoryID;
        this.ailmentID = ailmentChance;
        this.minHits = minHits;
        this.maxHits = maxHits;
        this.minTurns = minTurns;
        this.maxTurns = maxTurns;
        this.drain = drain;
        this.healing = healing;
        this.critRate = critRate;
        this.ailmentChance = ailmentChance;
        this.flinchChance = flinchChance;
        this.statChance = statChance;
    }

    public override string ToString()
    {
        return this.moveID.ToString();
    }
}
