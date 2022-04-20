using System.Collections;
using System.Collections.Generic;

public class PokemonSpecies
{
    public readonly int id;
    public readonly string identifier;
    public readonly int generationID;
    public readonly int evolvesFrom;
    public readonly int evolutionChainID;
    public readonly int genderRate;
    public readonly int captureRate;
    public readonly int baseHappiness;
    public readonly bool isBaby;
    public readonly int hatchCounter;
    public readonly bool hasGenderDifferences;
    public readonly int growthRateID;
    public readonly bool formsSwitchable;
    public readonly string description;
    private Dictionary<int, int> pokedexValues; //(dexNum, pokemonNum)
    public readonly string name;
    public readonly string genus;
    public readonly PokemonColor color;
    public readonly PokemonShape shape;
    public readonly PokemonHabitat habitat;
    public PokemonPalPark palpark;
    
    public PokemonSpecies(int id, string identifier, int generationID, int evolvesFrom, int evolutionChainID,
        PokemonColor color, PokemonShape shape, PokemonHabitat habitat, int genderRate, int captureRate, int baseHappiness, bool isBaby,
        int hatchCounter, bool hasGenderDifferences, int growthRateID,  bool formsSwitchable, string name, string genus, string description)
    {
        this.id = id;
        this.identifier = identifier;
        this.generationID = generationID;
        this.evolvesFrom = evolvesFrom;
        this.evolutionChainID = evolutionChainID;
        this.color = color;
        this.shape = shape;
        this.habitat = habitat;
        this.genderRate = genderRate;
        this.captureRate = captureRate;
        this.baseHappiness = baseHappiness;
        this.isBaby = isBaby;
        this.hatchCounter = hatchCounter;
        this.hasGenderDifferences = hasGenderDifferences;
        this.growthRateID = growthRateID;
        this.formsSwitchable = formsSwitchable;
        this.name = name;
        this.genus = genus;
        this.description = description;
        this.pokedexValues = new Dictionary<int, int>();
    }
    //post-ctor setters
    public void AddPokedexValue(int dexNum, int pokemonNum)
    {
        this.pokedexValues.Add(dexNum, pokemonNum);
    }
    public void SetPalPark(PokemonPalPark pp)
    {
        this.palpark = pp;
    }

    //getters
    public PokemonPalPark GetPokemonPalPark()
    {
        return this.palpark;
    }

    public int GetPokedexValue(int pokedexID)
    {
        return this.pokedexValues[pokedexID];
    }

    public string FullString()
    {
        return
            $@"{this.name} ({this.genus}) with Kanto Pokedex No.{this.GetPokedexValue(GameManager.Instance.POKEDEX_ID)} is of
            color {this.color}, shape of {this.shape} in habitat {this.habitat}, and has gender difference {this.hasGenderDifferences.ToString()}.
            Has pal park status {this.palpark}. Description is as follows: {this.description}";
    }

    public override string ToString()
    {
        return this.name + ", " + this.genus;
    }
}
