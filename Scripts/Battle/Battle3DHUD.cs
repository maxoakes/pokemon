using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Battle3DHUD : MonoBehaviour
{
    private BattlePokemon pokemon;
    public GameObject nameObject;
    private TextMeshPro nameText;
    public GameObject levelObject;
    private TextMeshPro levelText;
    public GameObject genderObject;
    private TextMeshPro genderText;
    public GameObject hpObject;
    private TextMeshPro hpText;
    public GameObject statusObject;
    private TextMeshPro statusText;

    public Transform hpBar;
    public Transform expBar;
    public Transform expBarContainer;

    public void SetHUD(BattlePokemon pokemon)
    {
        this.pokemon = pokemon;
        this.nameText = nameObject.GetComponent<TextMeshPro>();
        this.levelText = levelObject.GetComponent<TextMeshPro>();
        this.genderText = genderObject.GetComponent<TextMeshPro>();
        this.hpText = hpObject.GetComponent<TextMeshPro>();
        this.statusText = statusObject.GetComponent<TextMeshPro>();

        if (this.pokemon.team == Team.Foe)
        {
            //this.gameObject.transform.localScale *= .8f;
            this.expBar.gameObject.SetActive(false);
            this.expBarContainer.gameObject.SetActive(false);
        }
        else
        {
            //this.gameObject.transform.localScale *= .3f;
            this.SetExp();
        }

        this.SetName();
        this.SetLevel();

        (string s, Color c) gender = PartyMenu.GetGenderStringColor(this.pokemon.basePartyPokemon);
        this.genderText.text = gender.s;
        this.genderText.color = gender.c;
        this.SetStatus();
        this.SetHP();
    }
    public void SetHP()
    {
        float percent = ((float)this.pokemon.basePartyPokemon.GetCurrentHP()/(float)this.pokemon.basePartyPokemon.GetStatTuple(1).actual)*100f;
        hpBar.localScale = new Vector3(percent,100f,100f);
        hpText.text = $"{this.pokemon.basePartyPokemon.GetCurrentHP()}/{this.pokemon.basePartyPokemon.GetStatTuple(1).actual}";
    }
    public void SetExp()
    {
        float maxExp = this.pokemon.basePartyPokemon.GetExperienceOfNextLevel();
        float minExp = this.pokemon.basePartyPokemon.GetExperienceOfCurrentLevel();
        float currExp = this.pokemon.basePartyPokemon.GetCurrentExperience();
        float percent = (currExp-minExp)/(maxExp-minExp)*100f;
        expBar.localScale = new Vector3(percent,100f,100f);
    }
    public void SetLevel()
    {
        levelText.text = $"L.{this.pokemon.basePartyPokemon.GetLevel()}";
    }
    public void SetName()
    {
        nameText.text = this.pokemon.displayName;
    }
    public void SetStatus()
    {
        (string s, Color c) status = PartyMenu.GetStatusString(this.pokemon.basePartyPokemon);
        this.statusText.text = status.s;
        this.statusText.color = status.c;
    }
    public void UpdateLevel()
    {
        this.SetExp();
        this.SetLevel();
        this.SetHP();
    }

    public BattlePokemon GetAccompanyingPokemon()
    {
        return this.pokemon;
    }
}
