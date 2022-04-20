using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System;
using System.Text.RegularExpressions;
using Mono.Data.Sqlite;
using UnityEngine;

public enum Weather
{
    Clear,
    Rainy,
    Snowy,
    HarshSun,
    Hail,
    Sandstorm,
    Fog,
    Windy
}
public enum Gender
{
    Genderless,
    Male,
    Female
}

public enum Status
{
    None,
    Sleep,
    Burnt,
    Frozen,
    Paralyzed,
    Poisoned,
    BadlyPoisoned

}

public enum VolitileStatus
{
    Bound,
    NoEscape,
    Confusion,
    Drowsy,
    Embargo,
    Encore,
    Flinch,
    HealBlock,
    Identified,
    Infatuation,
    LeechSeed,
    Nightmare,
    PerishSong,
    Taunt,
    Telekinesis,
    Torment,
    AquaRing,
    Bracing,
    Charging,
    CenterOfAttention,
    Rooting,
    MagicCoat,
    Levitation,
    Mimic,
    Minimize,
    Protection,
    Recharging,
    SemiInvulnerable,
    Substitute,
    Thrashing,
    Transformed,
    FocusEnergy
}

public class Registry
{
    private int gen;
    private int ver;
    private int vergrp;
    private int pokedex;
    public List<string> trainerNamesMale;
    public List<string> trainerNamesFemale;
    public List<string> trainerOccupationsUnisex;
    public List<string> trainerOccupationsMale;
    public List<string> trainerOccupationsFemale;
    public Dictionary<int, PokemonSpecies> species;
    public Dictionary<int, PokemonColor> colors;
    public Dictionary<int, PokemonShape> shapes;
    public Dictionary<int, PokemonHabitat> habitats;
    public Dictionary<int, PokemonForm> forms;
    public Dictionary<int, Ability> abilities;
    public Dictionary<int, EggGroup> eggGroups;
    public Dictionary<int, PokemonType> types;
    public Dictionary<int, PokemonBaseStats> baseStats;
    public Dictionary<int, Pokemon> pokemon;
    public Dictionary<string, int> pokemonNames;
    public Dictionary<int, Evolution> evolutions;
    public Dictionary<int, MoveMeta> moveMetas;
    public Dictionary<int, MoveEffect> moveEffects;
    public Dictionary<int, Move> moves;
    public Dictionary<int, Item> items;
    public Dictionary<int, ItemCategory> itemCategories;
    public Dictionary<int, Characteristic> characteristics;
    public Dictionary<int, Nature> natures;
    public Dictionary<int, Berry> berries;
    public PokemonGrowthRate growthRate;
    public Move struggle;

    private string DB_LOCATION = "URI=file:" + Application.dataPath + "/veekun-pokedex.sqlite";
    private IDbConnection dbconn;
    private IDbCommand dbcmd;
 
    public Registry(int generationID, int versionGroupID, int versionID, int pokedexID)
    {
        this.gen = generationID;
        this.vergrp = versionGroupID;
        this.ver = versionID;
        this.pokedex = pokedexID;

        Debug.Log("Building Registry from Database...");
        dbconn = new SqliteConnection(DB_LOCATION);
        dbcmd = dbconn.CreateCommand();        

        //create all major pokemon primatives
        CreateSpecies();
        CreateItems();
        CreateMoves();

        //create smaller primatives and pokemon
        CreateEggGroups();
        CreateAbilities();
        CreateForms();
        CreateTypes();
        CreateBaseStats();
        CreatePokemon();
        CreateGrowthRates();
        
        //create side objects
        CreateEvolutions();
        CreateCharacteristics();
        CreateNatures();
        CreateBerries();
        CreateTrainerVarients();
    }

    private IDataReader StartQuery(string query)
    {
        dbconn.Open();
        dbcmd.CommandText = query;
        return dbcmd.ExecuteReader();
    }

    private void EndQuery(IDataReader reader)
    {
        reader.Close();
        dbconn.Close();
    }

    private int GetIntFromDB(IDataReader r, int col)
    {
        if (r.IsDBNull(col))
        {
            return -1;
        }
        else
        {
            return r.GetInt32(col);
        }
    }

    private string GetStringFromDB(IDataReader r, int col)
    {
        if (r.IsDBNull(col))
        {
            return "";
        }
        else
        {
            return Regex.Replace(r.GetString(col), @"\s+", " "); //this operation adds at least .5 seconds
        }
    }

    /*
    * Initialize and fill the basic objects that a pokemon species is made of
    */
    private void CreateSpecies()
    {
        //Color
        IDataReader r = StartQuery(
            @"select c.id, c.identifier, cn.name
            from pokemon_colors as c join pokemon_color_names as cn on c.id=cn.pokemon_color_id
            where cn.local_language_id=9");

        this.colors = new Dictionary<int, PokemonColor>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonColor entry = new PokemonColor(
                id: key,
                identifier: GetStringFromDB(r, 1),
                name: GetStringFromDB(r, 2));
            this.colors.Add(key, entry);
        }
        EndQuery(r);

        //Shape
        r = StartQuery(
            @"select s.id, s.identifier, sp.name, sp.awesome_name, sp.description
            from pokemon_shapes as s join pokemon_shape_prose as sp on s.id=sp.pokemon_shape_id
            where sp.local_language_id=9");

        this.shapes = new Dictionary<int, PokemonShape>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonShape entry = new PokemonShape(
                id: key,
                identifier: GetStringFromDB(r, 1),
                name: GetStringFromDB(r, 2),
                awesomeName: GetStringFromDB(r, 3),
                description: GetStringFromDB(r, 4));
            this.shapes.Add(key, entry);
        }
        EndQuery(r);

        //Habitat
        r = StartQuery(
            @"select h.id, h.identifier, hn.name
            from pokemon_habitats as h join pokemon_habitat_names as hn on h.id=hn.pokemon_habitat_id
            where hn.local_language_id=9");

        this.habitats = new Dictionary<int, PokemonHabitat>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonHabitat entry = new PokemonHabitat(
                id: GetIntFromDB(r, 0),
                identifier: GetStringFromDB(r, 1),
                name: GetStringFromDB(r, 2));
            this.habitats.Add(key, entry);
        }
        EndQuery(r);
        this.habitats.Add(-1, new PokemonHabitat(-1, "none", "None"));

        //create species
        r = StartQuery(
            $@"select s.id, s.identifier, s.generation_id, s.evolves_from_species_id,
            s.evolution_chain_id, s.color_id, s.shape_id, s.habitat_id, s.gender_rate,
            s.capture_rate, s.base_happiness, s.is_baby, s.hatch_counter, s.has_gender_differences,
            s.growth_rate_id, s.forms_switchable, n.name, n.genus, t.flavor_text
            from (pokemon_species as s join pokemon_species_names as n on s.id=n.pokemon_species_id)
			join pokemon_species_flavor_text as t on t.species_id=s.id
            where n.local_language_id = 9 and generation_id <= {gen}
            and language_id=9 and version_id={ver}");

        species = new Dictionary<int, PokemonSpecies>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonSpecies entry = new PokemonSpecies(
                id: key,
                identifier: GetStringFromDB(r, 1),
                generationID: GetIntFromDB(r, 2),
                evolvesFrom: GetIntFromDB(r, 3),
                evolutionChainID: GetIntFromDB(r, 4),
                color: GetColorByID(GetIntFromDB(r, 5)),
                shape: GetShapeByID(GetIntFromDB(r, 6)),
                habitat: GetHabitatByID(GetIntFromDB(r, 7)),
                genderRate: GetIntFromDB(r, 8),
                captureRate: GetIntFromDB(r, 9),
                baseHappiness: GetIntFromDB(r, 10),
                isBaby: r.GetBoolean(11),
                hatchCounter: GetIntFromDB(r, 12),
                hasGenderDifferences: r.GetBoolean(13),
                growthRateID: GetIntFromDB(r, 14),
                formsSwitchable: r.GetBoolean(15),
                name: GetStringFromDB(r, 16), 
                genus: GetStringFromDB(r, 17),
                description: GetStringFromDB(r, 18));
            this.species.Add(key, entry);
        }
        EndQuery(r);

        //create PalPark for each species
        r = StartQuery(
            @"select pp.species_id, pp.area_id, pp.base_score, pp.rate, ppa.identifier, ppan.name
            from (pal_park as pp join pal_park_areas as ppa on pp.area_id=ppa.id)
            join pal_park_area_names as ppan on pp.area_id=ppan.pal_park_area_id
            where ppan.local_language_id=9");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.species.ContainsKey(key)) continue;
            PokemonPalPark entry = new PokemonPalPark(
                speciesID: key,
                areaID: GetIntFromDB(r, 1),
                baseScore: GetIntFromDB(r, 2),
                rate: GetIntFromDB(r, 3),
                identifier: GetStringFromDB(r, 4),
                name: GetStringFromDB(r, 5));
            species[key].SetPalPark(entry);
        }
        EndQuery(r);

        //fill pokedex values
        r = StartQuery(
            @"select * from pokemon_dex_numbers");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.species.ContainsKey(key)) continue;
            species[key].AddPokedexValue(GetIntFromDB(r, 1), GetIntFromDB(r, 2));
        }
        EndQuery(r);
    }

    public PokemonColor GetColorByID(int id)
    {
        return this.colors[id];
    }

    public PokemonShape GetShapeByID(int id)
    {
        return this.shapes[id];
    }

    public PokemonHabitat GetHabitatByID(int id)
    {
        return this.habitats[id];
    }
    public PokemonSpecies GetSpeciesByID(int id)
    {
        return this.species[id];
    }

    /*
    * Initialize and fill items and item categories
    */
    private void CreateItems()
    {
        //get the item categories
        IDataReader r = StartQuery(
            @"select id, identifier, pocket_id, name
            from item_categories as c join item_category_prose as p on c.id=p.item_category_id where local_language_id=9");

        itemCategories = new Dictionary<int, ItemCategory>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            ItemCategory entry = new ItemCategory(
                id: key,
                identifier: GetStringFromDB(r, 1),
                pocketID: GetIntFromDB(r, 2),
                name: GetStringFromDB(r, 3));
            this.itemCategories.Add(key, entry);
        }
        EndQuery(r);

        //get all the items
        r = StartQuery(
            $@"select id, identifier, category_id, cost, fling_power, fling_effect_id, name, flavor_text
            from (items as i join item_names as n on i.id=n.item_id)
			join item_flavor_text as t on t.item_id=i.id where local_language_id=9
			and version_group_id = {vergrp} and language_id=9");

        items = new Dictionary<int, Item>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Item entry = new Item(
                id: key,
                identifier: GetStringFromDB(r, 1),
                category: GetItemCategoryByID(GetIntFromDB(r, 2)),
                cost: GetIntFromDB(r, 3),
                flingPower: GetIntFromDB(r, 4),
                flingEffectID: GetIntFromDB(r, 5),
                name: GetStringFromDB(r, 6),
                description: GetStringFromDB(r, 7));
            this.items.Add(key, entry);
        }
        EndQuery(r);

        //fill item flags
        r = StartQuery(
            @"select * from item_flag_map");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.items.ContainsKey(key)) continue;
            this.items[key].AddFlag(GetIntFromDB(r, 1));
        }
        EndQuery(r);
    }
    public Item GetItemByID(int id)
    {
        return this.items[id];
    }
    public ItemCategory GetItemCategoryByID(int id)
    {
        return this.itemCategories[id];
    }

    /*
    * Initialize and fill moves. Only moves <618
    */
    private void CreateMoves()
    {
        //create move metas
        IDataReader r = StartQuery(
            @"select * from move_meta");

        moveMetas = new Dictionary<int, MoveMeta>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            MoveMeta entry = new MoveMeta(
                moveID: GetIntFromDB(r, 0),
                categoryID: GetIntFromDB(r, 1),
                ailmentID: GetIntFromDB(r, 2),
                minHits: GetIntFromDB(r, 3),
                maxHits: GetIntFromDB(r, 4),
                minTurns: GetIntFromDB(r, 5),
                maxTurns: GetIntFromDB(r, 6),
                drain: GetIntFromDB(r, 7),
                healing: GetIntFromDB(r, 8),
                critRate: GetIntFromDB(r, 9),
                ailmentChance: GetIntFromDB(r, 10),
                flinchChance: GetIntFromDB(r, 11),
                statChance: GetIntFromDB(r, 12));
            moveMetas.Add(key, entry);
        }
        EndQuery(r);

        //create effects for moves
        r = StartQuery(
            @"select move_effect_id, short_effect, effect from move_effect_prose where local_language_id=9");

        moveEffects = new Dictionary<int, MoveEffect>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            MoveEffect entry = new MoveEffect(
                id: key,
                effectShort: GetStringFromDB(r, 1),
                effectLong: GetStringFromDB(r, 2));
            moveEffects.Add(key, entry);
        }
        EndQuery(r);

        //create moves
        r = StartQuery(
            $@"select id, identifier, generation_id, type_id, power, pp, accuracy, priority,
            target_id, damage_class_id, effect_id, effect_chance, name, flavor_text
            from (moves as m join move_names as n on m.id=n.move_id)
			join move_flavor_text as t on t.move_id=m.id
            where local_language_id = 9 and id < 618 and generation_id <= {gen}
			and version_group_id = {vergrp} and language_id = 9");

        moves = new Dictionary<int, Move>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Move entry = new Move(
                id: key,
                identifier: GetStringFromDB(r, 1),
                generationID: GetIntFromDB(r, 2),
                typeID: GetIntFromDB(r, 3),
                power: GetIntFromDB(r, 4),
                pp: GetIntFromDB(r, 5),
                accuracy: GetIntFromDB(r, 6),
                priority: GetIntFromDB(r, 7),
                targetID: GetIntFromDB(r, 8),
                damageClassID: GetIntFromDB(r, 9),
                effect: GetMoveEffectByID(GetIntFromDB(r, 10)),
                effectChance: GetIntFromDB(r, 11),
                name: GetStringFromDB(r, 12),
                meta: GetMoveMetaByID(key),
                description: GetStringFromDB(r, 13));
            moves.Add(key, entry);
            if (GetStringFromDB(r, 1).Equals("struggle"))
            {
                Debug.Log($"Struggle move found! Is {key}:{GetStringFromDB(r, 13)}");
                this.struggle = entry;
            }
        }
        EndQuery(r);

        //fill move flags
        r = StartQuery(
            @"select * from move_flag_map where move_id < 618");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.moves.ContainsKey(key)) continue;
            moves[key].addFlag(GetIntFromDB(r, 1));
        }
        EndQuery(r);
    }
    
    public MoveMeta GetMoveMetaByID(int id)
    {
        return this.moveMetas[id];
    }

    public MoveEffect GetMoveEffectByID(int id)
    {
        return this.moveEffects[id];
    }
    public Move GetMoveByID(int id)
    {
        return this.moves[id];
    }

    /*
    * Initialize and fill other Pokemon primatives
    */
    private void CreateEggGroups()
    {
        IDataReader r = StartQuery(
            @"select e.id, e.identifier, p.name
            from egg_groups as e join egg_group_prose as p on e.id=p.egg_group_id
            where local_language_id=9");

        eggGroups = new Dictionary<int, EggGroup>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            EggGroup entry = new EggGroup(
                id: key,
                identifier: GetStringFromDB(r, 1),
                name: GetStringFromDB(r, 2));
            eggGroups.Add(key, entry);
        }
        EndQuery(r);
    }
    public EggGroup GetEggGroupByID(int id)
    {
        return this.eggGroups[id];
    }

    private void CreateAbilities()
    {
        IDataReader r = StartQuery(
            $@"select a.id, a.identifier, a.generation_id, a.is_main_series, n.name, p.short_effect, p.effect, flavor_text
            from ((abilities as a join ability_names as n on a.id=n.ability_id)
			join ability_prose as p on a.id=p.ability_id)
			join ability_flavor_text as t on t.ability_id=a.id
            where n.local_language_id=9 and p.local_language_id=9
			and language_id=9 and version_group_id = 16"); //hard-coded version group id for mega forms

        abilities = new Dictionary<int, Ability>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Ability entry = new Ability(
                id: key,
                identifier: GetStringFromDB(r, 1),
                generationID: GetIntFromDB(r, 2),
                isMainSeries: r.GetBoolean(3),
                name: GetStringFromDB(r, 4),
                effectShort: GetStringFromDB(r, 5),
                effectLong: GetStringFromDB(r, 6),
                description: GetStringFromDB(r, 7));
            abilities.Add(key, entry);
        }
        EndQuery(r);
    }
    public Ability GetAbilityByID(int id)
    {
        return this.abilities[id];
    }

    private void CreateForms()
    {
        IDataReader r = StartQuery(
            $@"select f.id, f.identifier, f.form_identifier, f.pokemon_id,
            f.introduced_in_version_group_id, f.is_default, f.is_battle_only, f.is_mega,
            f.form_order, fn.form_name, fn.pokemon_name
            from pokemon_forms as f join pokemon_form_names as fn on f.id=fn.pokemon_form_id
            where fn.local_language_id = 9 and introduced_in_version_group_id <= {vergrp}");

        forms = new Dictionary<int, PokemonForm>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonForm entry = new PokemonForm(
                id: key,
                pokemonIdentifier: GetStringFromDB(r, 1),
                formIdentifier: GetStringFromDB(r, 2),
                pokemonID: GetIntFromDB(r, 3),
                introducedIn: GetIntFromDB(r, 4),
                isDefault: r.GetBoolean(5), 
                isBattleOnly: r.GetBoolean(6),
                isMega: r.GetBoolean(7),
                formOrder: GetIntFromDB(r, 8),
                formName: GetStringFromDB(r, 9),
                pokemonName: GetStringFromDB(r, 10));
            forms.Add(key, entry);
        }
        EndQuery(r);
    }
    public PokemonForm GetFormByID(int id)
    {
        return this.forms[id];
    }

    private void CreateTypes()
    {
        IDataReader r = StartQuery(
            @"select t.id, t.identifier, t.generation_id, t.damage_class_id, n.name
            from types as t join type_names as n on t.id=n.type_id where local_language_id=9");

        types = new Dictionary<int, PokemonType>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            PokemonType entry = new PokemonType(
                id: key,
                identifier: GetStringFromDB(r, 1),
                introduced: GetIntFromDB(r, 2),
                damageClassID: GetIntFromDB(r, 3),
                name: GetStringFromDB(r, 4));
            types.Add(key, entry);
        }
        EndQuery(r);

        //fill descriptions of each ability
        r = StartQuery(
            @"select * from type_efficacy");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            types[key].addTargetDamageFactor(GetIntFromDB(r, 1), GetIntFromDB(r, 2));
        }
        EndQuery(r);
    }
    public PokemonType GetTypeByID(int id)
    {
        return this.types[id];
    }

    //fill the stats of a pokemon by ID
    private void CreateBaseStats()
    {
        IDataReader r = StartQuery(
            @"select * from pokemon_stats order by pokemon_id");

        this.baseStats = new Dictionary<int, PokemonBaseStats>();
        while (r.Read())
        {
            int pokemonID = GetIntFromDB(r, 0);
            if (!this.baseStats.ContainsKey(pokemonID))
            {
                this.baseStats.Add(pokemonID, new PokemonBaseStats());
            }
            this.baseStats[pokemonID].AddStat(GetIntFromDB(r, 1), GetIntFromDB(r, 2), GetIntFromDB(r, 3));
        }
        EndQuery(r);
    }
    public PokemonBaseStats GetBaseStatByID(int id)
    {
        return this.baseStats[id];
    }
    private void CreateGrowthRates()
    {
        IDataReader r = StartQuery(
            @"select * from experience");

        growthRate = new PokemonGrowthRate();
        while (r.Read())
        {
            growthRate.addGrowthRateElement(GetIntFromDB(r, 0), GetIntFromDB(r, 1), GetIntFromDB(r, 2));
        }
        EndQuery(r);
    }

    /*
    * create pokemon and fill them with all of the established objects
    */
    private void CreatePokemon()
    {
        IDataReader r = StartQuery(
            @"select * from pokemon");

        pokemon = new Dictionary<int, Pokemon>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            int speciesID = GetIntFromDB(r, 2);
            string identifier = GetStringFromDB(r, 1);
            if (GameManager.Instance.VERSION_GROUP_ID <= 15 && identifier.Contains("-mega")) continue;
            if (identifier.Contains("-primal")) continue;
            if (key >= 10080 && key <= 10085) continue; //pikachu varients
            if (!this.species.ContainsKey(speciesID)) continue;
            Pokemon entry = new Pokemon(
                id: key,
                identifier: identifier,
                species: this.GetSpeciesByID(speciesID),
                height: GetIntFromDB(r, 3),
                weight: GetIntFromDB(r, 4),
                baseExperience: GetIntFromDB(r, 5),
                order: GetIntFromDB(r, 6),
                isDefault: r.GetBoolean(7));
            pokemon.Add(key, entry);
        }
        EndQuery(r);

        //fill possible abilites
        r = StartQuery(
            @"select pokemon_id, ability_id, is_hidden from pokemon_abilities");

        while (r.Read())
        {
            int pokemonID = GetIntFromDB(r, 0);
            int abilityID = GetIntFromDB(r, 1);
            if (!this.abilities.ContainsKey(abilityID)) continue;
            if (!this.pokemon.ContainsKey(pokemonID)) continue;
            pokemon[pokemonID].AddAbility(this.GetAbilityByID(abilityID), r.GetBoolean(2));
        }
        EndQuery(r);

        //fill egg groups
        r = StartQuery(
            @"select * from pokemon_egg_groups");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.pokemon.ContainsKey(key)) continue;
            pokemon[key].AddEggGroup(this.GetEggGroupByID(GetIntFromDB(r, 1)));
        }
        EndQuery(r);

        //fill possible held items
        r = StartQuery(
            @"select * from pokemon_items");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.pokemon.ContainsKey(key)) continue;
            pokemon[key].AddItem(
                    new PokemonHeldItem(GetIntFromDB(r, 1), GetIntFromDB(r, 2), GetIntFromDB(r, 3))
                );
        }
        EndQuery(r);

        //fill possible forms
        foreach (int key in this.forms.Keys)
        {
            pokemon[forms[key].pokemonID].AddForm(this.GetFormByID(key));
        }

        //fill types
        r = StartQuery(
            @"select * from pokemon_types");

        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            if (!this.pokemon.ContainsKey(key)) continue;
            pokemon[key].AddType(this.GetTypeByID(GetIntFromDB(r, 1)), GetIntFromDB(r, 2));
        }
        EndQuery(r);

        //fill stats
        foreach (int key in this.baseStats.Keys)
        {
            if (!this.pokemon.ContainsKey(key)) continue;
            pokemon[key].AddBaseStats(this.baseStats[key]);
        }
        EndQuery(r);

        //add moves to pokemon
        r = StartQuery(
            $@"select * from pokemon_moves where version_group_id = {vergrp}");

        while (r.Read())
        {
            int pokemonID = GetIntFromDB(r, 0);
            PokemonMoveMapping p = new PokemonMoveMapping(
                pokemonID: pokemonID,
                versionGroup: GetIntFromDB(r, 1),
                moveID: GetIntFromDB(r, 2),
                methodID: GetIntFromDB(r, 3),
                level: GetIntFromDB(r, 4),
                order: GetIntFromDB(r, 5));
            this.pokemon[pokemonID].AddMove(p);
        }
        EndQuery(r);

        //make a dictionary of pokemon based on name
        this.pokemonNames = new Dictionary<string, int>();
        foreach(KeyValuePair<int, Pokemon> entry in this.pokemon)
        {
            this.pokemonNames.Add(entry.Value.identifier, entry.Key);
        }
    }
    public Pokemon GetPokemonByID(int id)
    {
        return this.pokemon[id];
    }
    public Pokemon GetPokemonByName(string name)
    {
        if (this.pokemonNames.ContainsKey(name))
        {
            return this.pokemon[this.pokemonNames[name]];
        }
        else
        {
            Debug.LogError($"Pokemon '{name}' does not exist. Perhaps a misspelling?");
            return null;
        }
    }

    private void CreateEvolutions()
    {
        IDataReader r = StartQuery(
            @"select * from pokemon_evolution");

        evolutions = new Dictionary<int, Evolution>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Evolution entry = new Evolution(
                id: key,
                evolvedSpeciesID: GetIntFromDB(r, 1),
                evolutionTriggerID: GetIntFromDB(r, 2),
                triggerItemID: GetIntFromDB(r, 3),
                minLevel: GetIntFromDB(r, 4),
                genderID: GetIntFromDB(r, 5),
                locationID: GetIntFromDB(r, 6),
                heldItemID: GetIntFromDB(r, 7),
                timeOfDay: GetStringFromDB(r, 8),
                knownMoveID: GetIntFromDB(r, 9),
                knownMoveTypeID: GetIntFromDB(r, 10),
                minHappiness: GetIntFromDB(r, 11),
                minBeauty: GetIntFromDB(r, 12),
                minAffection: GetIntFromDB(r, 13),
                relPhysicalStats: GetIntFromDB(r, 14),
                partySpeciesID: GetIntFromDB(r, 15),
                partyTypeID: GetIntFromDB(r, 16),
                tradeSpeciesID: GetIntFromDB(r, 17),
                needsOverworldRain: r.GetBoolean(18),
                turnUpsideDown: r.GetBoolean(19));
            evolutions.Add(key, entry);
        }
        EndQuery(r);
    }
    private void CreateCharacteristics()
    {
        IDataReader r = StartQuery(
            @"select id, stat_id, gene_mod_5, message
            from characteristics as c join characteristic_text as t on c.id=t.characteristic_id where local_language_id=9");

        characteristics = new Dictionary<int, Characteristic>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Characteristic entry = new Characteristic(
                id: key,
                statID: GetIntFromDB(r, 1),
                geneMod: GetIntFromDB(r, 2),
                description: GetStringFromDB(r, 3));
            characteristics.Add(key, entry);
        }
        EndQuery(r);
    }
    public Characteristic GetCharacteristicByID(int id)
    {
        return characteristics[id];
    }

    public List<int> GetListOfCharacteristicIDsForStatID(int stat)
    {
        List<int> characteristicIDs = new List<int>();
        foreach (Characteristic c in this.characteristics.Values)
        {
            if (c.statID == stat)
            {
                characteristicIDs.Add(c.statID);
            }
        }
        return characteristicIDs;
    }
    private void CreateNatures()
    {
        IDataReader r = StartQuery(
            @"select id, identifier, decreased_stat_id, increased_stat_id, hates_flavor_id, likes_flavor_id, name
            from natures as n join nature_names as nn on n.id=nn.nature_id where local_language_id=9");

        natures = new Dictionary<int, Nature>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Nature entry = new Nature(
                id: key,
                identifier: GetStringFromDB(r, 1),
                decreasedStatID: GetIntFromDB(r, 2),
                increasedStatID: GetIntFromDB(r, 3),
                hatesFlavorID: GetIntFromDB(r, 4),
                likesFlavorID: GetIntFromDB(r, 5),
                name: GetStringFromDB(r, 6));
            natures.Add(key, entry);
        }
        EndQuery(r);
    }
    public List<int> GetNatureKeys()
    {
        return new List<int>(this.natures.Keys);
    }
    private void CreateBerries()
    {
        IDataReader r = StartQuery(
            @"select * from berries");

        berries = new Dictionary<int, Berry>();
        while (r.Read())
        {
            int key = GetIntFromDB(r, 0);
            Berry entry = new Berry(
                id: key,
                itemID: GetIntFromDB(r, 1),
                firmnessID: GetIntFromDB(r, 2),
                naturalGiftPower: GetIntFromDB(r, 3),
                naturalGiftTypeID: GetIntFromDB(r, 4),
                size: GetIntFromDB(r, 5),
                maxHarvest: GetIntFromDB(r, 6),
                growthTime: GetIntFromDB(r, 7),
                soilDryness: GetIntFromDB(r, 8),
                smoothness: GetIntFromDB(r, 9));
            berries.Add(key, entry);
        }
        EndQuery(r);
    }

    public void CreateTrainerVarients()
    {
        this.trainerNamesMale = new List<string>
        {"James","Robert","John","Michael","William","David","Richard","Joseph","Thomas","Charles","Christopher","Daniel","Matthew","Anthony","Mark","Donald","Steven","Paul","Andrew","Joshua","Kenneth","Kevin","Brian","George","Edward","Ronald","Timothy","Jason","Jeffrey","Ryan","Jacob","Gary","Nicholas","Eric","Jonathan","Stephen","Larry","Justin","Scott","Brandon","Benjamin","Samuel","Gregory","Frank","Alexander","Raymond","Patrick","Jack","Dennis","Jerry","Tyler","Aaron","Jose","Adam","Henry","Nathan","Douglas","Zachary","Peter","Kyle","Walter","Ethan","Jeremy","Harold","Keith","Christian","Roger","Noah","Gerald","Carl","Terry","Sean","Austin","Arthur","Lawrence","Jesse","Dylan","Bryan","Joe","Jordan","Billy","Bruce","Albert","Willie","Gabriel","Logan","Alan","Juan","Wayne","Roy","Ralph","Randy","Eugene","Vincent","Russell","Elijah","Louis","Bobby","Philip","Johnny"};

        this.trainerNamesFemale = new List<string>
        {"Mary","Patricia","Jennifer","Linda","Elizabeth","Barbara","Susan","Jessica","Sarah","Karen","Nancy","Lisa","Betty","Margaret","Sandra","Ashley","Kimberly","Emily","Donna","Michelle","Dorothy","Carol","Amanda","Melissa","Deborah","Stephanie","Rebecca","Sharon","Laura","Cynthia","Kathleen","Amy","Shirley","Angela","Helen","Anna","Brenda","Pamela","Nicole","Emma","Samantha","Katherine","Christine","Debra","Rachel","Catherine","Carolyn","Janet","Ruth","Maria","Heather","Diane","Virginia","Julie","Joyce","Victoria","Olivia","Kelly","Christina","Lauren","Joan","Evelyn","Judith","Megan","Cheryl","Andrea","Hannah","Martha","Jacqueline","Frances","Gloria","Ann","Teresa","Kathryn","Sara","Janice","Jean","Alice","Madison","Doris","Abigail","Julia","Judy","Grace","Denise","Amber","Marilyn","Beverly","Danielle","Theresa","Sophia","Marie","Diana","Brittany","Natalie","Isabella","Charlotte","Rose","Alexis","Kayla"};

        this.trainerOccupationsUnisex = new List<string>
        {"Account Collector","Accounting Specialist","Administrative Assistant","Aeronautical Engineer","Aerospace Engineer","Anesthesiologist","Animal Breeder","Animal Trainer","Aquaculturist","Art Appraiser","Athletic Coach","Baker","Bank Teller","Biologist","Bus Driver","Cardiologist","Cartographer","Cartoonist","Cartoonist","Chemical Engineer","Chief Financial Officer","Civil Drafter","Civil Engineer","Clergy Member","Clinical Dietitian","Clinical Psychologist","Clinical Sociologist","Community Health Nurse","Compliance Officer","Computer Operators","Computer Programmer","Congressional Aide","Construction Laborer","Cook","Coroner","Criminal Lawyer","Crossing Guard","Database Administrator","Delivery Driver","Dentist","Dermatologist","Director","Disk Jockey","Diver","Door To Door Salesmen","Electrical Engineer","Elementary School Teacher","Environmental Engineer","Farm Hand","Farmer","Fashion Designer","Fashion Model","File Clerk","Financial Planner","Fine Artist","Fire Inspector","Fish & Game Warden","Fisherman","Fitness Trainer","Flight Attendant","Floral Designer","Forest Engineer","Forklift Operators","Glass Blower","Graphic Designer","Gynecologist","Hair Stylist","High School Teacher","Hospital Nurse","Hotel Manager","Industrial Engineer","Insurance Agent","Intelligence Agent","IT Administrator","Janitor","Kindergarten Teacher","Lawyer","Legal Assistant","Library Technician","Loan Officer","LPN","Mail Clerk","Makeup Artist","Massage Therapist","Math Professor","Mechanic","Mechanical Engineer","Medical Assistant","Medical Equipment Preparer","Metal Fabricator","Military Officer","Mill Worker","Mining Engineer","Museum Curator","Music Therapist","Nuclear Engineer","Nurse's Aide","Nursery Worker","Obstetrician","Office Clerk","Ophthalmologist","Orthodontist","Painter","Paramedic","Pathologist","Pediatrician","Petroleum Technician","Pharmacy Technician","Physical Therapist","Pilot","Plant Breeder","Plastic Surgeon","Plumber","Poet","Police Officer","Political Scientist","Postal Service Clerks","Power Plant Operators","Preschool Teacher","Printmaker","Private Detective","Probation Officer","Professor","Professor","Psychiatrist","Quarry Worker","Radar Technician","Radio Announcer","Radio Mechanic","Radio Talk Show Host","Radiologist","Rail Yard Engineer","Railroad Engineer","Real Estate Sales Agent","Recreational Therapist","Referee","Respiratory Therapist","Revenue Agent","Safety Inspector","Sales Manager","School Nurse","Screen Writer","Seamen","Security Guard","Ship Mate","Social Worker","Soil Scientist","Steel Worker","Structural Engineer","Surgeon","Tax Collector","Taxi Driver","Teachers Aide","Technician","Textile Designer","Tour Guide","Travel Agent","Treasurer","Truck Driver","TV Announcer","TV Talk Show Host","Veterinarian","Weather Observer","Welder","Writer","Zoologist"};

        this.trainerOccupationsMale = new List<string> {"Host","Waiter"};
        this.trainerOccupationsFemale = new List<string> {"Hostess","Waitress"};
    }
    
    public string GetRandomMaleOccupation()
    {
        List<string> returnList = new List<string>(this.trainerOccupationsUnisex);
        returnList.AddRange(trainerOccupationsMale);
        return returnList[UnityEngine.Random.Range(0, returnList.Count)];
    }

    public string GetRandomFemaleOccupation()
    {
        List<string> returnList = new List<string>(this.trainerOccupationsUnisex);
        returnList.AddRange(trainerOccupationsFemale);
        return returnList[UnityEngine.Random.Range(0, returnList.Count)];
    }

    public string GetRandomMaleName()
    {
        return this.trainerNamesMale[UnityEngine.Random.Range(0, this.trainerNamesMale.Count)];
    }

    public string GetRandomFemaleName()
    {
        return this.trainerNamesFemale[UnityEngine.Random.Range(0, this.trainerNamesFemale.Count)];
    }
}