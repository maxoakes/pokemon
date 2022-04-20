using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonHeldItem
{
    public readonly int versionID;
    public readonly int itemID;
    public readonly int rarity;

    public PokemonHeldItem(int versionID, int itemID, int rarity)
    {
        this.versionID = versionID;
        this.itemID = itemID;
        this.rarity = rarity;
    }
}
