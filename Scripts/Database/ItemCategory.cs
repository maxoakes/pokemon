using System.Collections;
using System.Collections.Generic;

public class ItemCategory
{
    public readonly int id;
    public readonly string identifier;
    public readonly int pocketID;
    public readonly string name;

    public ItemCategory(int id, string identifier, int pocketID, string name)
    {
        this.id = id;
        this.identifier = identifier;
        this.pocketID = pocketID;
        this.name = name;
    }

    public override string ToString()
    {
        return this.name;
    }
}
