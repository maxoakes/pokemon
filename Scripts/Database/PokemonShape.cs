using System.Collections;
using System.Collections.Generic;

public class PokemonShape
{
    private readonly int id;
    private readonly string identifier;
    private readonly string name;
    private readonly string awesomeName;
    private readonly string description;

    public PokemonShape(int id, string identifier, string name, string awesomeName, string description)
    {
        this.id = id;
        this.identifier = identifier;
        this.name = name;
        this.awesomeName = awesomeName;
        this.description = description;
    }

    public string FullString()
    {
        string s = $"{this.id}:{this.identifier} ({this.name}) ({this.awesomeName}) {this.description}";
        return s;
    }

    public override string ToString()
    {
        return this.awesomeName;
    }
}
