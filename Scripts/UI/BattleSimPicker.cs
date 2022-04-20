using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleSimPicker : MonoBehaviour
{
    public Registry registry;
    public Dropdown genSelector;
    public Button genConfirm;
    public GameObject pokemonSelectorBox;
    public List<GameObject> pokemonItems;
    public List<PartyPokemon> party;

    void Start()
    {
        GameManager.Instance.gameMode = Mode.BattleSimPicker;
        pokemonSelectorBox.SetActive(false);
        party = new List<PartyPokemon>();
        GameManager.Instance.player = new Trainer("Blue", "Cool Trainer", Gender.Male, Team.Ally);
    }
    public void OnGenConfirm()
    {
        Generation chosenGeneration = Generation.Gen5;
        int dropdownSelection = genSelector.value;
        switch (dropdownSelection)
        {
            case 0:
                chosenGeneration = Generation.Gen3;
                break;
            case 1:
                chosenGeneration = Generation.Gen4;
                break;
            case 2:
                chosenGeneration = Generation.Gen5;
                chosenGeneration = Generation.Gen6; //TODO: remove when sprites for gen 6 exist
                break;
            case 3:
                chosenGeneration = Generation.Gen6;
                break;
        }
        Debug.Log($"Chosen: {dropdownSelection} with enum {chosenGeneration}");
        GameManager.Instance.SetGeneration(chosenGeneration);
        pokemonSelectorBox.SetActive(true);
        try
        {
            party.RemoveRange(0,6);
        }
        catch (Exception)
        {
            Debug.Log("No party already created");
        }
        UpdateDisplayedList();

    }

    public void OnPokemonConfirm()
    {
        if (party.Count == 6)
        {
            Debug.Log("Party is full");
            return;
        }
        string pokemonInput = pokemonSelectorBox.transform.Find("Name").GetComponentInChildren<Text>().text;
        int id = 0;
        try
        {
            id = Int32.Parse(pokemonInput);
        }
        catch (Exception)
        {
            Debug.Log("not a number for pokemon");
        }
        string levelInput = pokemonSelectorBox.transform.Find("Level").GetComponentInChildren<Text>().text;
        int level = 1; 
        try
        {
            level = Int32.Parse(levelInput);
        }
        catch (Exception)
        {
            Debug.Log("level not valid");
            return;
        }
        
        try
        {
            if (id == 0)
            {
                Debug.Log($"Found {GameManager.Instance.registry.pokemonNames[pokemonInput]}");
                PartyPokemon p = new PartyPokemon(GameManager.Instance.registry.pokemonNames[pokemonInput], null, level);
                p.SetOriginalTrainer(GameManager.Instance.player);
                party.Add(p);
            }            
            else
            {   
                Debug.Log($"Found {GameManager.Instance.registry.species[id].name}");
                PartyPokemon p = new PartyPokemon(id, null, level);
                p.SetOriginalTrainer(GameManager.Instance.player);
                party.Add(p);
            }
            UpdateDisplayedList();
        }
        catch (Exception)
        {
            Debug.Log("Bad input");
        }
    }

    public void OnDeleteButton(int slot)
    {
        if (slot > party.Count) return;
        party.RemoveAt(slot);
        Debug.Log($"Removed pokemon at slot {slot}");
        UpdateDisplayedList();
    }
    public void UpdateDisplayedList()
    {
        for (int i = 0; i < 6; i++)
        {
            try
            {
                //pokemonItems[i].SetActive(true);
                pokemonItems[i].transform.Find("Description").GetComponent<Text>().text = party[i].FullString();
                Debug.Log("Party " + i + ": " + party[i].FullString());
            }
            catch (Exception)
            {
                pokemonItems[i].transform.Find("Description").GetComponent<Text>().text = "";
            }
        }
    }

    public void OnWildEncounterButton()
    {
        //ready the player trainer
        GameManager.Instance.player.party = party;

        //ready the wild pokemon encounter
        int level = Mathf.Max(party[0].GetLevel()-2,1);
        List<int> availableIDs = new List<int>(GameManager.Instance.registry.pokemon.Keys);
        int randomID = UnityEngine.Random.Range(0, availableIDs.Count);
        PartyPokemon encounter = new PartyPokemon(GameManager.Instance.registry.pokemon[availableIDs[randomID]].id, null, level);
        Trainer placeholderTrainer = new Trainer(true);
        placeholderTrainer.party.Add(encounter);
        
        GameManager.Instance.SetUpBattle(BattleType.Wild, placeholderTrainer);
        SceneManager.LoadScene("Battle");
    }

    public void OnTrainerEncounter()
    {
        //ready the player trainer
        GameManager.Instance.player.party = party;

        //ready the trainer and their pokemon
        Trainer trainer = new Trainer();
        int numPokemon = UnityEngine.Random.Range(2,6);
        int level = Mathf.Max(party[0].GetLevel()-2,1);
        List<int> availableIDs = new List<int>(GameManager.Instance.registry.pokemon.Keys);
        List<PartyPokemon> trainerPokemon = new List<PartyPokemon>();
        for (int i = 0; i < numPokemon; i++)
        {
            int randomID = UnityEngine.Random.Range(0, availableIDs.Count);
            trainerPokemon.Add(new PartyPokemon(GameManager.Instance.registry.pokemon[availableIDs[randomID]].id, null, level));
            trainerPokemon[i].SetOriginalTrainer(trainer);
            Debug.Log($"trainer has {trainerPokemon[i].FullString()}");
        }
        trainer.party = trainerPokemon;
        GameManager.Instance.SetUpBattle(BattleType.Trainer, trainer);
        SceneManager.LoadScene("Battle");
    }
}
