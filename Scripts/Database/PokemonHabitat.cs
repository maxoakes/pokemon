using System.Collections;
using System.Collections.Generic;

public class PokemonHabitat
{
    private readonly int id;
    private readonly string identifier;
    private readonly string name;

    public PokemonHabitat(int id, string identifier, string name)
    {
        this.id = id;
        this.identifier = identifier;
        this.name = name;
    }

    public string FullString()
    {
        string s = $"{this.id}:{this.identifier} ({this.name})";
        return s;
    }
    
    public override string ToString()
    {
        return this.name;
    }
}
