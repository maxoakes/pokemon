using System.Collections;
using System.Collections.Generic;

public class EggGroup
{
    public readonly int id;
    public readonly string identifier;
    public readonly string name;

    public EggGroup(int id, string identifier, string name)
    {
        this.id = id;
        this.identifier = identifier;
        this.name = name;
    }

    public string FullString()
    {
        return $"{this.name} with id {this.id}";
    }
    
    public override string ToString()
    {
        return this.name;
    }
}
