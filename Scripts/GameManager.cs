using System;
using System.Collections.Generic;
using UnityEngine;

public enum Generation
{
    Gen3, // Firered/Leafgreen
    Gen4, //HeartGold/SoulSilver
    Gen5, // Black2/White2
    Gen6 // OmegaRuby/AlphaSapphire
}

public enum Mode
{
    MainMenu,
    Overworld,
    Battle,
    BattleSimPicker
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Mode gameMode;
    //registry and settings
    public Registry registry;
    public Generation GEN;
    public int POKEDEX_ID;
    public int VERSION_GROUP_ID;
    public int VERSION_ID;
    public int GENERATION_ID;

    //player
    public Trainer player;

    //game status
    public Weather weather = Weather.Clear;

    //active battle
    public Trainer foeTrainer;
    public BattleType currentBattleType;
    public int battleSize;

    void Awake()
    {
        Debug.Log("GameManager awoken.");
        DontDestroyOnLoad(this.gameObject);
        if (GameManager.Instance == null) Instance = this;
        else UnityEngine.Object.Destroy(gameObject);
        gameMode = Mode.MainMenu;
    }
    public void SetGeneration(Generation gen)
    {
        registry = null;
        Debug.Log("Registry reset to null.");
        switch (gen)
        {
            case Generation.Gen3:
                POKEDEX_ID = 1;
                VERSION_ID = 11;
                VERSION_GROUP_ID = 7;
                GENERATION_ID = 3;
                break;
            case Generation.Gen4:
                POKEDEX_ID = 1;
                VERSION_ID = 16;
                VERSION_GROUP_ID = 10;
                GENERATION_ID = 4;
                break;
            case Generation.Gen5:
                POKEDEX_ID = 1;
                VERSION_ID = 18;
                VERSION_GROUP_ID = 14;
                GENERATION_ID = 5;
                break;
            case Generation.Gen6:
                POKEDEX_ID = 1;
                VERSION_ID = 24;
                VERSION_GROUP_ID = 15;
                GENERATION_ID = 6;
                break;
            default:
                POKEDEX_ID = 1;
                VERSION_ID = 16;
                VERSION_GROUP_ID = 10;
                GENERATION_ID = 4;
                break;
        }
        Debug.Log($"Generation set to {GENERATION_ID} and version {VERSION_GROUP_ID}.");
        registry = new Registry(GENERATION_ID, VERSION_GROUP_ID, VERSION_ID, POKEDEX_ID);
    }

    public void TestRegistry()
    {
        foreach (Pokemon p in this.registry.pokemon.Values)
        {
            PartyPokemon pp = new PartyPokemon(p.id, null, 35);
            Debug.Log(pp.FullString());
        }
    }

    public void AddPokemonToParty(PartyPokemon pokemon)
    {
        if (this.player.party.Count == 6)
        {
            Debug.Log("Party is already at maximum size");
            return;
        }
        this.player.party.Add(pokemon);
        Debug.Log($"{pokemon.GetName()} added to party.");
    }
    public void SwitchPartyPokemon(int first, int second)
    {
        if (first == second)
        {
            Debug.Log("Cannot switch a slot with itself");
            return;
        }
        PartyPokemon temp = this.player.party[second];
        this.player.party[second] = this.player.party[first];
        this.player.party[first] = temp;
    }

    public PartyPokemon RemovePokemonFromParty(int slot)
    {
        PartyPokemon removedPokemon = this.player.party[slot];
        this.player.party.RemoveAt(slot);
        return removedPokemon;
    }

    public int GetSlotOfFirstAlive()
    {
        for (int i = 0; i < 6; i++)
        {
            try
            {
                if (this.player.party[i].GetCurrentHP() > 0 && !this.player.party[i].IsEgg())
                {
                    Debug.Log("Active slot " + i);
                    return i;
                }
            }
            catch (Exception)
            {
                Debug.Log("Invalid party slot");
            }
        }
        Debug.Log("Active slot -1");
        return -1;
    }

    public void SetUpBattle(BattleType battleType, Trainer trainer, int battleSize=1)
    {
        this.currentBattleType = battleType;
        this.foeTrainer = trainer;
        this.battleSize = battleSize;
    }

    public void CleanUpBattle()
    {
        Typewriter.Instance.HideDialog();
        this.foeTrainer = null;
        this.battleSize = 1;
    }
}
