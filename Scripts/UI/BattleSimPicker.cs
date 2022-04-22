using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleSimPicker : MonoBehaviour
{
    //screen elements
    public TMP_Dropdown genSelector;
    public Button genConfirm;
    public TMP_InputField pokemonName;
    public TMP_InputField pokemonLevel;
    public List<GameObject> pokemonItems;

    public List<PartyPokemon> party;

    void Start()
    {
        GameManager.Instance.gameMode = Mode.BattleSimPicker;
        party = new List<PartyPokemon>();
        GameManager.Instance.player = new Trainer("Blue", "Cool Trainer", Gender.Male, Team.Ally);
        GameManager.Instance.SetGeneration(Generation.Gen5);
    }
    public void OnGenConfirm() { } //TODO: when all of the assets for all generations are obtained, then make this available

    public void OnPokemonConfirm()
    {
        if (party.Count == 6)
        {
            Debug.LogWarning("Party is full");
            return;
        }
        string pokemonInput = pokemonName.text;
        int id = 0;
        try
        {
            id = Int32.Parse(pokemonInput);
        }
        catch (Exception)
        {
            Debug.LogError("not a number for pokemon");
        }
        string levelInput = pokemonLevel.text;
        int level = 1; 
        try
        {
            level = Int32.Parse(levelInput);
        }
        catch (Exception)
        {
            Debug.LogError("level not valid");
            return;
        }
        
        try
        {
            PartyPokemon pokemon;
            if (id == 0)
            {
                Debug.Log($"Found {GameManager.Instance.registry.pokemonNames[pokemonInput]}");
                pokemon = new PartyPokemon(GameManager.Instance.registry.pokemonNames[pokemonInput], null, level);
            }            
            else
            {   
                Debug.Log($"Found {GameManager.Instance.registry.species[id].name}");
                pokemon = new PartyPokemon(id, null, level);
            }
            pokemon.SetOriginalTrainer(GameManager.Instance.player);
            party.Add(pokemon);
            UpdateDisplayedList();
        }
        catch (Exception e)
        {
            Debug.LogError(e.StackTrace);
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
                pokemonItems[i].transform.Find("Description").GetComponent<TextMeshProUGUI>().text = party[i].FullString();
                Debug.Log($"Party {i}: {party[i].FullString()}");
            }
            catch (Exception)
            {
                pokemonItems[i].transform.Find("Description").GetComponent<TextMeshProUGUI>().text = "";
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
