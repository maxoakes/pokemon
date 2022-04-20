using static System.DateTime;
using System.Collections.Generic;
using UnityEngine;

public enum TrainingStatus
{
    Wild,
    New,
    Trained,
    Maximum
}
public class PartyPokemon
{
    private int stepsToHatchEgg; //-1=not an egg, >-1=an egg
    private Trainer originalTrainer;
    private string nickname;
    public readonly Pokemon basePokemon;
    private PokemonForm currentForm;
    public readonly string captureBall;
    public readonly string captureLocation;
    public readonly string captureMethod;
    private long captureDate;
    public readonly int captureLevel;
    public readonly Gender gender;
    public readonly bool isShiny;
    private int level;
    private int experience;
    private int currentHp;
    private Dictionary<int, int> actualStats;
    private Dictionary<int, int> baseStats;
    private Dictionary<int, int> IVs;
    public Dictionary<int, int> EVs;

    public Status status;
    public readonly Nature nature;
    public readonly Characteristic characteristic;
    private int friendship;
    public readonly Ability ability;
    private List<(int currPP, int maxPP, Move move)> moves; //(currpp, totalpp, move)
    private Item heldItem;
    private List<string> ribbons;
    private bool hasPokerus;
    
    //wild pokemon constructor
    public PartyPokemon(int pokemonID, int? formID, int level)
    {
        this.stepsToHatchEgg = -1; //not an egg
        this.originalTrainer = null; //wild, no trainer
        this.nickname = ""; //no nickname
        this.basePokemon = GameManager.Instance.registry.GetPokemonByID(pokemonID);

        //assign default or specified form
        List<PokemonForm> possibleForms = this.basePokemon.getForms();
        this.currentForm = null;
        if (possibleForms.Count > 0)
        {
            foreach (PokemonForm form in possibleForms)
            {
                if (form.isDefault)
                {
                    this.currentForm = form;
                    break;
                }
            }
        }
        //if the form is specified
        if (formID != null)
        {
            bool formFound = false;
            foreach (PokemonForm form in possibleForms)
            {
                if (form.id == formID)
                {
                    this.currentForm = form;
                    formFound = true;
                    break;
                }
            }
            if (!formFound) Debug.LogError($"Form {formID} not found for {pokemonID}:{this.basePokemon.species}");
            else Debug.Log($"Form {formID}:{this.currentForm.pokemonName} found for {pokemonID}:{this.basePokemon.species}");
        }

        //get capture details
        this.captureBall = "";
        this.captureLocation = "PLACEHOLDER LOCATION";
        this.captureMethod = (stepsToHatchEgg == -1) ? "Caught" : "Hatched";
        this.captureDate = System.DateTime.Now.Ticks;
        this.captureLevel = level;
        int genderRate = this.basePokemon.species.genderRate; //int from 1 to 8
        if (genderRate == -1) //genderless species
        {
            this.gender = Gender.Genderless;
        }
        else
        {
            this.gender = (Random.Range(0f, 8f) < genderRate) ? Gender.Female : Gender.Male;
        }
        this.isShiny = (Random.Range(0f, 8192f) < 1f) ? true : false;
        this.level = level;
        this.experience = GameManager.Instance.registry.growthRate.GetExperienceAtLevel(this.basePokemon.species.growthRateID, level);
        this.nature = GameManager.Instance.registry.natures[(int)Mathf.Round(Random.Range (1, GameManager.Instance.registry.GetNatureKeys().Count))]; //randomly pick nature
        
        //calcuate and fill stats
        this.actualStats = new Dictionary<int, int>();
        this.baseStats = new Dictionary<int, int>();
        this.IVs = new Dictionary<int, int>();
        this.EVs = new Dictionary<int, int>();
        for (int i = 1; i <= 6; i++)
        {
            this.baseStats.Add(i, basePokemon.GetBaseStats().GetBaseStatByID(i));
            this.IVs.Add(i, (int)Mathf.Floor(Random.Range(0f,32f)));
            this.EVs.Add(i, 0);
            this.actualStats[i] = this.GetCurrentStatByID(i);
        }
        this.currentHp = this.actualStats[1];

        //get the characteristic based on IVs
        List<int> possibleCharacteristicIDs = GameManager.Instance.registry.GetListOfCharacteristicIDsForStatID(this.GetHighestIV());
        int chosenCharacteristic = possibleCharacteristicIDs[Random.Range(0, possibleCharacteristicIDs.Count)];
        this.characteristic = GameManager.Instance.registry.GetCharacteristicByID(chosenCharacteristic);

        this.status = Status.None;
        this.friendship = this.basePokemon.species.baseHappiness;
        this.ability = this.basePokemon.GetPossibleAbilities()[Random.Range(0, this.basePokemon.GetPossibleAbilities().Count)];
        this.hasPokerus = false;
        this.ribbons = new List<string>();

        //possibly add a held item to the pokemon
        List<PokemonHeldItem> heldItemStruct = this.basePokemon.GetPossibleHeldItems();
        List<(Item item, int rarity)> possibleItems = new List<(Item, int)>();
        int remainingRarity = 100;
        foreach (PokemonHeldItem i in heldItemStruct)
        {
            if (i.versionID == GameManager.Instance.VERSION_ID)
            {
                possibleItems.Add((item: GameManager.Instance.registry.GetItemByID(i.itemID), rarity: i.rarity));
                remainingRarity = remainingRarity - i.rarity;
            }
        }
        possibleItems.Add((null, remainingRarity));

        int randomWeight = Random.Range(0, 100);
        int currentWeight = 0;
        foreach ((Item i, int r) pair in possibleItems)
        {
            currentWeight += pair.r;
            if (randomWeight <= currentWeight)
                    this.heldItem = pair.i;
                    break;
        }

        //get moves
        List<PokemonMoveMapping> moveStruct = this.basePokemon.GetMoves();
        List<(int l, Move m)> possibleMoves = new List<(int, Move)>();
        foreach (PokemonMoveMapping m in moveStruct)
        {
            if (m.method == MoveLearnMethod.LevelUp && m.versionGroup == GameManager.Instance.VERSION_GROUP_ID && this.level >= m.level)
            {
                possibleMoves.Add((m.level, GameManager.Instance.registry.GetMoveByID(m.moveID)));
            }
        }
        possibleMoves.Sort((x, y) => x.l.CompareTo(y.l));
        this.moves = new List<(int, int, Move)>();
        for (int i = Mathf.Max(0, possibleMoves.Count-4); i < possibleMoves.Count; i++)
        {
            (int currPP, int maxPP, Move move) toAdd = (currPP: possibleMoves[i].m.pp, maxPP: possibleMoves[i].m.pp, move: possibleMoves[i].m);
            if (this.moves.Contains(toAdd)) continue;
            this.moves.Add(toAdd);
        }
    }
    /*
    ..######..########.########.########.########.########...######.
    .##....##.##..........##.......##....##.......##.....##.##....##
    .##.......##..........##.......##....##.......##.....##.##......
    ..######..######......##.......##....######...########...######.
    .......##.##..........##.......##....##.......##...##.........##
    .##....##.##..........##.......##....##.......##....##..##....##
    ..######..########....##.......##....########.##.....##..######.
    */
    public void SetOriginalTrainer(Trainer trainer)
    {
        this.originalTrainer = trainer;
    }
    public void AddEVTuple((int hp, int att, int def, int spa, int spd, int spe) ev)
    {
        this.EVs[1] += Mathf.Min(ev.hp,  255);
        this.EVs[2] += Mathf.Min(ev.att, 255);
        this.EVs[3] += Mathf.Min(ev.def, 255);
        this.EVs[4] += Mathf.Min(ev.spa, 255);
        this.EVs[5] += Mathf.Min(ev.spd, 255);
        this.EVs[6] += Mathf.Min(ev.spe, 255);
    }
    public void LevelUp()
    {
        float percentHP = (float)this.currentHp/(float)this.actualStats[1];
        Debug.Log($"percent HP {percentHP}");
        this.level++;
        for (int i = 1; i <= 6; i++)
        {
            float increased = (this.basePokemon.GetBaseStats().GetBaseStatByID(i)/50f) + ((this.IVs[i] + this.EVs[i])/100f);
            this.actualStats[i] = this.GetCurrentStatByID(i);
            Debug.Log($"Stat {i} increased by {increased}. Is now {this.actualStats[i]}");
        }
        this.currentHp = (int)Mathf.Ceil(percentHP*(float)this.actualStats[1]);

    }

    public List<Move> GetMovesOnLevelUp()
    {
        List<Move> moves = new List<Move>();
        foreach (PokemonMoveMapping mm in this.basePokemon.GetMoves())
        {
            if (mm.versionGroup == GameManager.Instance.VERSION_GROUP_ID && mm.method == MoveLearnMethod.LevelUp && mm.level == this.level)
            {
                moves.Add(GameManager.Instance.registry.GetMoveByID(mm.moveID));
            }
        }
        return moves;
    }

    public bool AddMove(Move move)
    {
        if (this.moves.Count < 4)
        {
            moves.Add((move.pp, move.pp, move));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ReplaceMove(Move move, int? slot)
    {
        if (slot != null) this.moves[(int)slot] = (move.pp, move.pp, move);
    }
    public void ReduceMovePP(int slot, int amount)
    {
        Move m = this.moves[slot].move;
        int pp = this.moves[slot].currPP;
        int max = this.moves[slot].maxPP;
        this.moves[slot] = (Mathf.Max(pp-amount, 0), max, m);
    }
    public void SetHP(int hp)
    {
        this.currentHp = Mathf.Max(hp, 0);
    }
    public void SetExperience(int exp)
    {
        this.experience = exp;
    }
    public void SetHeldItem(Item item)
    {
        //TODO: check to see if the item can be a held item
        this.heldItem = item;
    }

    /*
    ..######...########.########.########.########.########...######.
    .##....##..##..........##.......##....##.......##.....##.##....##
    .##........##..........##.......##....##.......##.....##.##......
    .##...####.######......##.......##....######...########...######.
    .##....##..##..........##.......##....##.......##...##.........##
    .##....##..##..........##.......##....##.......##....##..##....##
    ..######...########....##.......##....########.##.....##..######.
    */
    public int GetHighestIV()
    {
        List<int> i = new List<int>
            {this.IVs[1], this.IVs[2], this.IVs[3], this.IVs[4], this.IVs[5], this.IVs[6]};
        int highest = Mathf.Max(i.ToArray());
        return 1 + i.IndexOf(highest);
    }
    private int GetCurrentStatByID(int statID)
    {
        if (statID < 1 || statID > 6) return 0;
        if (statID == 1) //HP
        {
            if (this.basePokemon.identifier.Equals("shedinja")) return 1; //a very special case

            return (int)Mathf.Floor(
                (2*this.basePokemon.GetBaseStats().GetBaseStatByID(statID)+this.IVs[1]+this.EVs[1]) * this.level / 100 + this.level + 10);
        }

        //2=Att, 3=Def, 4=SpAtt, 5=SpDef, 6=Speed
        float natureMultiplier = 1;
        if (this.nature.decreasedStatID == statID)
        {
            natureMultiplier = 0.9f;
        }
        else if (this.nature.increasedStatID == statID)
        {
            natureMultiplier = 1.1f;
        }

        return (int)Mathf.Floor(
            Mathf.Floor(
                (2 * this.basePokemon.GetBaseStats().GetBaseStatByID(statID) + this.IVs[statID] + this.EVs[statID]) * this.level / 100 + 5) * natureMultiplier);
    }
    public (int actual, int ba, int IV, int EV) GetStatTuple(int statID)
    {
        return (this.actualStats[statID], this.baseStats[statID], this.IVs[statID], this.EVs[statID]);
    }
    public int GetExperienceOfNextLevel()
    {
        int experience = GameManager.Instance.registry.growthRate.GetExperienceAtLevel(this.basePokemon.species.growthRateID, this.level + 1);
        return experience;
    }
    public int GetExperienceOfCurrentLevel()
    {
        int experience = GameManager.Instance.registry.growthRate.GetExperienceAtLevel(this.basePokemon.species.growthRateID, this.level);
        return experience;
    }
    public int GetExperienceNeededToLevelUp()
    {
        if (this.level == 100) return int.MaxValue;
        return this.GetExperienceOfNextLevel()-this.experience;
    }
    public int GetCurrentExperience()
    {
        return this.experience;
    }
    public string GetName()
    {
        return (this.nickname.Equals("")) ? this.basePokemon.species.name : this.nickname;
    }

    public int GetCurrentHP()
    {
        return this.currentHp;
    }
    public List<(int pp, int ppm, Move m)> GetMoves()
    {
        return this.moves;
    }
    public Trainer GetOriginalTrainer()
    {
        return this.originalTrainer;
    }
    public Item GetHeldItem()
    {
        return this.heldItem;
    }
    public bool IsEgg()
    {
        return (this.stepsToHatchEgg == -1) ? true : false;
    }
    public bool UsableInBattle()
    {
        //return (!this.IsEgg() && this.currentHp > 0);
        return (this.currentHp > 0);
    }
    public PokemonForm GetForm()
    {
        return this.currentForm;
    }
    public string GetFormIdentifier()
    {
        if (this.currentForm is null)
        {
            return "";
        }
        else
        {
            return this.currentForm.formIdentifier;
        }
    }
    public int GetLevel()
    {
        return this.level;
    }
    /*
    .########..########.########..##.....##..######
    .##.....##.##.......##.....##.##.....##.##....##
    .##.....##.##.......##.....##.##.....##.##......
    .##.....##.######...########..##.....##.##...####
    .##.....##.##.......##.....##.##.....##.##....##
    .##.....##.##.......##.....##.##.....##.##....##
    .########..########.########...#######...######
    */
    public string MoveListString()
    {
        string m = "";
        foreach((int,int,Move) p in this.moves)
        {
            m += $"{p.Item3.name} ({p.Item1}/{p.Item2}), ";
        }
        return m;
    }
    public string GetCurrentExperienceString()
    {
        return $"Starting exp at level {this.level} is {GetExperienceOfCurrentLevel()}. Currently at {GetCurrentExperience()}. Next level starts at {GetExperienceOfNextLevel()}. Need {GetExperienceNeededToLevelUp()})";
    }
    public string StatsListString()
    {
        return $"HP:{(this.GetCurrentStatByID(1),this.IVs[1],this.EVs[1])}, Att:{(this.GetCurrentStatByID(2),this.IVs[2],this.EVs[2])}, Def:{(this.GetCurrentStatByID(3),this.IVs[3],this.EVs[3])}, SpAtt:{(this.GetCurrentStatByID(4),this.IVs[4],this.EVs[4])}, SpDef:{(this.GetCurrentStatByID(5),this.IVs[5],this.EVs[5])}, Sp:{(this.GetCurrentStatByID(6),this.IVs[6],this.EVs[6])}";
    }
    public string FullString()
    {
        return $"Wild Pokemon {this.basePokemon.species.name} (p:{this.basePokemon}) in form ({this.currentForm}) is level {this.level} and is {this.gender}. Stats are {this.StatsListString()}. Moves are {this.MoveListString()}. Ability is {this.ability}. Hidden ability is {this.basePokemon.GetHiddenAbility()}. IsShiny:{this.isShiny}. Holding item {this.heldItem}. Nature is {this.nature}. Characteristic is {this.characteristic}";
    }
    public override string ToString()
    {
        return $"{this.basePokemon.species.name}:{this.nickname}";
    }

}
