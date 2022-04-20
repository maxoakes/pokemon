using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI level;
    public TextMeshProUGUI gender;
    public TextMeshProUGUI hp;
    public TextMeshProUGUI status;
    public Slider hpSlider;
    public Slider expSlider;

    public void SetHUD(BattlePokemon pokemon)
    {
        nameText.text = pokemon.displayName;
        level.text = $"Lvl:{pokemon.basePartyPokemon.GetLevel()}";
        (string s, Color c) gender = PartyMenu.GetGenderStringColor(pokemon.basePartyPokemon);
        // this.genderText.text = gender.s;
        // this.genderText.color = gender.c;
        // this.statusText.text = PartyMenu.GetStatusString(pokemon.basePartyPokemon);
        hpSlider.maxValue = pokemon.basePartyPokemon.GetStatTuple(1).actual;
        hpSlider.value = pokemon.basePartyPokemon.GetCurrentHP();
        hpSlider.minValue = 0;
        hp.text = $"{pokemon.basePartyPokemon.GetCurrentHP()}/{pokemon.basePartyPokemon.GetStatTuple(1).actual}";
        if (expSlider is null) return;
        expSlider.minValue = pokemon.basePartyPokemon.GetExperienceOfCurrentLevel();
        expSlider.maxValue = pokemon.basePartyPokemon.GetExperienceOfNextLevel();
        expSlider.value = pokemon.basePartyPokemon.GetCurrentExperience();
    }

    public void ClearHUD()
    {

    }
    public void SetHP(int newValue)
    {
        hpSlider.value = newValue;
        this.hp.text = $"{(int)hpSlider.value}/{(int)hpSlider.maxValue}";
    }

    public void SetExp(int newValue)
    {
        expSlider.value = newValue;
    }

    public void UpdateLevel(BattlePokemon pokemon)
    {
        expSlider.minValue = pokemon.basePartyPokemon.GetExperienceOfCurrentLevel();
        expSlider.maxValue = pokemon.basePartyPokemon.GetExperienceOfNextLevel();
        expSlider.value = pokemon.basePartyPokemon.GetCurrentExperience();
        level.text = $"Lvl:{pokemon.basePartyPokemon.GetLevel()}";
        hpSlider.maxValue = pokemon.basePartyPokemon.GetStatTuple(1).actual;
        hpSlider.value = pokemon.basePartyPokemon.GetCurrentHP();
        hpSlider.minValue = 0;
        hp.text = $"{pokemon.basePartyPokemon.GetCurrentHP()}/{pokemon.basePartyPokemon.GetStatTuple(1).actual}";
    }
}
