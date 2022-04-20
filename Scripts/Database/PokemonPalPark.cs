using System.Collections;
using System.Collections.Generic;

public class PokemonPalPark
{
    private readonly int speciesID;
    private readonly int areaID; //1=Forest, 2=Field, 3=Mountain, 4=Pond, 5=Sea
    private readonly int baseScore;
    private readonly int rate;
    private readonly string identifier;
    private readonly string name;

    public PokemonPalPark(int speciesID, int areaID, int baseScore, int rate, string identifier, string name)
    {
        this.speciesID = speciesID;
        this.areaID = areaID;
        this.baseScore = baseScore;
        this.rate = rate;
        this.identifier = identifier;
        this.name = name;
    }

    public string FullString()
    {
        string s = $"{this.areaID}:{this.identifier} ({this.name}) with score {this.baseScore} and rate {this.rate} for speciesID {this.speciesID}";
        return s;
    }

    public override string ToString()
    {
        return this.name + " with rate " + this.rate;
    }
}
