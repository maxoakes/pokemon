using System.Collections;
using System.Collections.Generic;

public class Characteristic
{
    public readonly int id;
    public readonly int statID;
    public readonly int geneMod;
    public readonly string description;

    public Characteristic(int id, int statID, int geneMod, string description)
    {
        this.id = id;
        this.statID = statID;
        this.geneMod = geneMod;
        this.description = description;
    }

    public override string ToString()
    {
        return this.description;
    }
}
