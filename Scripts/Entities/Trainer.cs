using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AI
{
    Random,
    Smart,
    Leader
}
public class Trainer
{
    public readonly string name;
    public readonly string occupation;
    public readonly Team team;
    public readonly AI battleAI;
    public readonly Gender gender;
    public readonly int id;
    public readonly string dialogWinBattle;
    public readonly string dialogLoseBattle;
    public readonly string dialogStartBattle;
    private int money;

    public List<PartyPokemon> party;
    private List<Item> inventory;
    
    // a random trainer
    public Trainer(bool isWild=false)
    {
        this.team = Team.Foe;
        if (isWild)
        {
            this.gender = Gender.Genderless;
            this.name = "Mother Nature";
            this.occupation = "Wild Pokemon Placeholder";
            this.id = 0;
            this.party = new List<PartyPokemon>();
            this.battleAI = AI.Random;
        }
        else
        {
            this.gender = (Random.Range(0f,1f) > .5f) ? Gender.Male : Gender.Female;
            this.name = (this.gender == Gender.Male) ? GameManager.Instance.registry.GetRandomMaleName() : GameManager.Instance.registry.GetRandomFemaleName();
            this.occupation = (this.gender == Gender.Male) ? GameManager.Instance.registry.GetRandomMaleOccupation() : GameManager.Instance.registry.GetRandomFemaleOccupation();
            this.id = Random.Range(0,99999);
            this.party = new List<PartyPokemon>();
            this.inventory = new List<Item>();
            this.dialogLoseBattle = "I lost! No!";
            this.dialogWinBattle = "Woo! I won!";
            this.dialogStartBattle = "Get ready to lose!";
            this.money = Random.Range(0,10000);
            this.battleAI = AI.Random;
        }
    }
    public Trainer(string name, string occupation, Gender gender, Team team=Team.Foe, AI battleAI=AI.Random)
    {
        this.team = team;
        this.name = name;
        this.occupation = occupation;
        this.gender = gender;
        this.id = Random.Range(0,99999);
        this.party = new List<PartyPokemon>();
        this.inventory = new List<Item>();
        this.dialogLoseBattle = "Damn! I lost!";
        this.dialogWinBattle = "YES! I won!";
        this.dialogStartBattle = "I'm ready!";
        this.money = 5500;
        this.battleAI = battleAI;
    }
    public void SetParty(List<PartyPokemon> party)
    {
        this.party = party;
    }
    public ref List<PartyPokemon> GetParty()
    {
        return ref this.party;
    }
    public int MoneyAfterBattleLoss()
    {
        return money/2;
    }
    public int UpdateMoney(int netGain)
    {
        this.money += netGain;
        return money;
    }
    public int GetNumberUsablePokemon()
    {
        int usablePokemon = 0;
        foreach (PartyPokemon p in this.party)
        {
            if (p.UsableInBattle()) usablePokemon++;
        }
        return usablePokemon;
    }
    public string FullString()
    {
        return $"{this.gender} {this.occupation} {this.name} with ID:{this.id}";
    }
    public override string ToString()
    {
        return $"{this.occupation} {this.name}";
    }
}
