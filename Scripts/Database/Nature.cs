using System.Collections;
using System.Collections.Generic;

public class Nature
{
    public readonly int id;
    public readonly string identifier;
    public readonly int decreasedStatID;
    public readonly int increasedStatID;
    public readonly int hatesFlavorID;
    public readonly int likesFlavorID;
    public readonly string name;

    public Nature(int id, string identifier, int decreasedStatID, int increasedStatID, int hatesFlavorID, int likesFlavorID, string name)
    {
        this.id = id;
        this.identifier = identifier;
        this.decreasedStatID = decreasedStatID;
        this.increasedStatID = increasedStatID;
        this.hatesFlavorID = hatesFlavorID;
        this.likesFlavorID = likesFlavorID;
        this.name = name;
    }

    public override string ToString()
    {
        return this.name;
    }
}
