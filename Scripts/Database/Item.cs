using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public readonly int id;
    public readonly string identifier;
    public readonly ItemCategory category;
    public readonly int cost;
    public readonly int flingPower;
    public readonly int flingEffectID; //1=badly poisons, 2=burns, 3=berry effect on target, 4=herb's effect on target, 5=paralyzes target, 6=poisons target, 7=target flinches
    public readonly string name;
    public readonly string description;
    private List<int> flags; //1=countable, 2=consumable, 3=overworld usable, 4=battle usable, 5=holdable, 6=holdable passive, 7=holdable active, 8=underground


    public Item(int id, string identifier, ItemCategory category, int cost, int flingPower, int flingEffectID, string name, string description)
    {
        this.id = id;
        this.identifier = identifier;
        this.category = category;
        this.cost = cost;
        this.flingPower = flingPower;
        this.flingEffectID = flingEffectID;
        this.name = name;
        this.flags = new List<int>();
        this.description = description;
    }

    public void AddFlag(int flag)
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
            $@"{this.name} is a '{this.category.ToString()}' with cost of {this.cost} and flags {string.Join(",",this.GetFlags().ToArray())}
            Description: {this.description}";
    }

    public override string ToString()
    {
        return this.name;
    }
}