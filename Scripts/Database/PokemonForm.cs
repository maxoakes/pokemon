using System.Collections;
using System.Collections.Generic;

public class PokemonForm
{
    public readonly int id;
    public readonly string pokemonIdentifier;
    public readonly string formIdentifier;
    public readonly int pokemonID;
    public readonly int introducedIn;
    public readonly bool isDefault;
    public readonly bool isBattleOnly;
    public readonly bool isMega;
    public readonly int formOrder;
    public readonly string formName;
    public readonly string pokemonName;

    public PokemonForm(int id, string pokemonIdentifier, string formIdentifier, int pokemonID, int introducedIn, bool isDefault, 
        bool isBattleOnly, bool isMega, int formOrder, string formName, string pokemonName)
    {
        this.id = id;
        this.pokemonID = pokemonID;
        this.introducedIn = introducedIn;
        this.isDefault = isDefault;
        this.isBattleOnly = isBattleOnly;
        this.isMega = isMega;
        this.formOrder = formOrder;
        this.formName = formName;
        this.pokemonIdentifier = pokemonIdentifier;
        this.formIdentifier = formIdentifier;
        this.pokemonName = pokemonName;
    }

    public string FullString()
    {
        return $"{this.pokemonName} with form name {this.formName}. Introduced in {this.introducedIn}. Is default: {this.isDefault}. Is Mega: {this.isMega}.";
    }

    public override string ToString()
    {
        return this.formName;
    }
}
