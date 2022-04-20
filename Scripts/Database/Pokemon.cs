using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    //from database
    public readonly int id;
    public readonly string identifier;
    public readonly PokemonSpecies species;
    public readonly int height;
    public readonly int weight;
    public readonly int baseExperience;
    public readonly int order;
    public readonly bool isDefault;

    //addons from database
    #nullable enable
    private PokemonBaseStats baseStats;
    private PokemonType type1;
    private PokemonType? type2;
    private List<Ability> possibleAbilities;
    private Ability? hiddenAbility;
    private List<EggGroup> eggGroups;
    private List<PokemonHeldItem> possibleHeldItems;
    private List<PokemonForm> possibleForms;
    private List<PokemonMoveMapping> moveList;
    #nullable disable

    public Pokemon(int id, string identifier, PokemonSpecies species, int height,
        int weight, int baseExperience, int order, bool isDefault)
    {
        this.id = id;
        this.identifier = identifier;
        this.species = species;
        this.height = height;
        this.weight = weight;
        this.baseExperience = baseExperience;
        this.order = order;
        this.isDefault = isDefault;
        this.type1 = null;
        this.type2 = null;

        this.possibleAbilities = new List<Ability>();
        this.eggGroups = new List<EggGroup>();
        this.possibleHeldItems = new List<PokemonHeldItem>();
        this.possibleForms = new List<PokemonForm>();

        moveList = new List<PokemonMoveMapping>();
    }

    //set
    public void AddType(PokemonType t, int slot)
    {
        switch (slot)
        {
            case 1:
                this.type1 = t;
                break;
            case 2:
                this.type2 = t;
                break;
            default:
                Debug.LogError($"Unknown slot number ({slot}) when assigning to pokemon {this.identifier}.");
                break;
        }
    }
    public void AddAbility(Ability ability, bool isHidden)
    {
        if (isHidden)
        {
            this.hiddenAbility = ability;
        }
        else
        {
            this.possibleAbilities.Add(ability);
        }
    }
    public void AddEggGroup(EggGroup group)
    {
        this.eggGroups.Add(group);
    }
    public void AddItem(PokemonHeldItem i)
    {
        this.possibleHeldItems.Add(i);
    }
    public void AddForm(PokemonForm form)
    {
        this.possibleForms.Add(form);
    }
    public void AddBaseStats(PokemonBaseStats stats)
    {
        this.baseStats = stats;
    }
    public void AddMove(PokemonMoveMapping pmm)
    {
        this.moveList.Add(pmm);
    }
    public List<PokemonForm> getForms()
    {
        return this.possibleForms;
    }
    public PokemonBaseStats GetBaseStats()
    {
        return this.baseStats;
    }
    public List<Ability> GetPossibleAbilities()
    {
        return this.possibleAbilities;
    }
    public Ability GetHiddenAbility()
    {
        return this.hiddenAbility;
    }
    public List<PokemonHeldItem> GetPossibleHeldItems()
    {
        return this.possibleHeldItems;
    }
    public List<PokemonMoveMapping> GetMoves()
    {
        return this.moveList;
    }
    public PokemonType GetType1()
    {
        return this.type1;
    }

    public PokemonType GetType2()
    {
        return this.type2;
    }
    public string FullString()
    {
        return $"{this.identifier} is species {this.species.name} is of color {this.species.color}. Height and weight are {this.height} and {this.weight}. Base stats are {this.baseStats} and is of type(s) {this.type1} {this.type2}. One possible ability is {this.possibleAbilities[0]}. It has {this.possibleForms.Count} forms.";
    }
    public override string ToString()
    {
        return this.identifier;
    }
}
