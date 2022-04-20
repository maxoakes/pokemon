using System.Collections;
using System.Collections.Generic;

public class Evolution
{
    public readonly int id;
    public readonly int evolvedSpeciesID;
    public readonly int evolutionTriggerID; //1=level up, 2=trade, 3=use item, 4=shed
    public readonly int triggerItemID;
    public readonly int minLevel;
    public readonly int genderID;
    public readonly int locationID;
    public readonly int heldItemID;
    public readonly string timeOfDay;
    public readonly int knownMoveID;
    public readonly int knownMoveTypeID;
    public readonly int minHappiness;
    public readonly int minBeauty;
    public readonly int minAffection;
    public readonly int relPhysicalStats;
    public readonly int partySpeciesID;
    public readonly int partyTypeID;
    public readonly int tradeSpeciesID;
    public readonly bool needsOverworldRain;
    public readonly bool turnUpsideDown;

    public Evolution(int id, int evolvedSpeciesID, int evolutionTriggerID, int triggerItemID,
        int minLevel, int genderID, int locationID, int heldItemID, string timeOfDay,
        int knownMoveID, int knownMoveTypeID, int minHappiness, int minBeauty, int minAffection,
        int relPhysicalStats, int partySpeciesID, int partyTypeID, int tradeSpeciesID,
        bool needsOverworldRain, bool turnUpsideDown)
    {
        this.id = id;
        this.evolvedSpeciesID = evolvedSpeciesID;
        this.evolutionTriggerID = evolutionTriggerID;
        this.triggerItemID = triggerItemID;
        this.minLevel = minLevel;
        this.genderID = genderID;
        this.locationID = locationID;
        this.heldItemID = heldItemID;
        this.timeOfDay = timeOfDay;
        this.knownMoveID = knownMoveID;
        this.knownMoveTypeID = knownMoveTypeID;
        this.minHappiness = minHappiness;
        this.minBeauty = minBeauty;
        this.minAffection = minAffection;
        this.relPhysicalStats = relPhysicalStats;
        this.partySpeciesID = partySpeciesID;
        this.partyTypeID = partyTypeID;
        this.tradeSpeciesID = tradeSpeciesID;
        this.needsOverworldRain = needsOverworldRain;
        this.turnUpsideDown = turnUpsideDown;
    }

    public override string ToString()
    {
        return $"Evolves into {this.evolvedSpeciesID}";
    }
}
