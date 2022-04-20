using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SwapType
{
    None,
    OverworldSwapSlots,
    BattleActionSwap,
    OnFaintedFoe,
    OnFaintedAlly
}
public class PartyMenu : MonoBehaviour
{
    public GameObject partyMenuUI;
    private GameObject menuUI;
    public static PartyMenu Instance;
    public SwapType currentSwapType;

    void Awake()
    {
        Debug.Log("PartyMenu awoken");
        DontDestroyOnLoad(this.gameObject);
        if (PartyMenu.Instance == null) Instance = this;
        else UnityEngine.Object.Destroy(gameObject);
        menuUI = partyMenuUI.transform.GetChild(0).gameObject;
        CloseMenu();
    }

    public void ShowMenu()
    {
        Trainer player = GameManager.Instance.player;
        if (GameManager.Instance.gameMode != Mode.Battle || GameManager.Instance.gameMode == Mode.Overworld) return;
        menuUI.SetActive(true);
        List<Image> partyMemberPanel = new List<Image>();
        for (int i = 0; i < 6; i++)
        {
            partyMemberPanel.Add(partyMenuUI.transform.GetChild(0).Find("partyslot."+i).gameObject.GetComponent<Image>());
            try
            {
                partyMemberPanel[i].transform.Find("sprite").GetComponentInChildren<Image>().sprite = 
                    SpriteLookup.Instance.GetIcon(player.party[i]);
                Debug.Log("found sprite of " + i);
                partyMemberPanel[i].transform.Find("name").GetComponent<TextMeshProUGUI>().text = player.party[i].GetName();
                Debug.Log("found name of " + i);
                (string s, Color c) gender = GetGenderStringColor(player.party[i]);
                partyMemberPanel[i].transform.Find("gender").GetComponent<TextMeshProUGUI>().text = gender.s;
                partyMemberPanel[i].transform.Find("gender").GetComponent<TextMeshProUGUI>().color = gender.c;
                Debug.Log("found gender of " + i);
                partyMemberPanel[i].transform.Find("level").GetComponent<TextMeshProUGUI>().text = "Lvl: " + player.party[i].GetLevel();
                Debug.Log("found level of " + i);
                partyMemberPanel[i].transform.Find("status").GetComponent<TextMeshProUGUI>().text = GetStatusString(player.party[i]).Item1;
                partyMemberPanel[i].transform.Find("status").GetComponent<TextMeshProUGUI>().color = GetStatusString(player.party[i]).Item2;
                Debug.Log("found status of " + i);
                partyMemberPanel[i].transform.Find("hpText").GetComponent<TextMeshProUGUI>().text = "HP: " + player.party[i].GetCurrentHP() + "/" + player.party[i].GetStatTuple(1).actual;
                Debug.Log("found hpText of " + i);
                Slider hp = partyMemberPanel[i].transform.Find("hpBar").GetComponent<Slider>();
                hp.minValue = 0;
                hp.maxValue = player.party[i].GetStatTuple(1).actual;
                hp.value = player.party[i].GetCurrentHP();
                Debug.Log("found hpBar of " + i);
                partyMemberPanel[i].gameObject.SetActive(true);
                if (GameManager.Instance.gameMode == Mode.Battle)
                {
                    partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>().interactable = player.party[i].UsableInBattle();
                    if (BattleSystemV2.Instance.GetActivePartyIndex(Team.Ally) == i) partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>().interactable = false;
                    int p = i;
                    if (currentSwapType == SwapType.BattleActionSwap)
                    {
                        partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>()
                            .onClick.AddListener(() => OnQueueForSwitchButton(p));
                    }
                    else if (currentSwapType == SwapType.OnFaintedAlly || currentSwapType == SwapType.OnFaintedFoe)
                    {
                        partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>()
                            .onClick.AddListener(() => OnImmediateSwitchButton(p));
                    }
                }
                else
                {
                    //TODO: fix this for overworld
                    partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>().onClick.AddListener(() => SwapPokemonInSlots(i, i));
                }
            }
            catch (Exception)
            {
                Debug.Log("could not find all of " + i);
                partyMemberPanel[i].gameObject.SetActive(false);
            }
        }
        Button cancel = partyMenuUI.transform.GetChild(0).transform.Find("cancel").GetComponentInParent<Button>();
        cancel.interactable = true;
        if (currentSwapType != SwapType.OnFaintedAlly)
        {
            cancel.onClick.AddListener(OnCancelButton);
        }
        else
        {
            cancel.interactable = false;
        }
        
    }

    public void OnQueueForSwitchButton(int slot)
    {
        Debug.Log("Performing switch from party menu " + slot);
        BattleSystemV2.Instance.SetPartySwitchAction(slot);
        CloseMenu();
    }

    public void OnImmediateSwitchButton(int slot)
    {
        Debug.Log($"Performing {currentSwapType.ToString()} switch from party menu {slot}");
        StartCoroutine(BattleSystemV2.Instance.SwapCurrentPokemon(slot));
        CloseMenu();
    }

    public void OnCancelButton()
    {
        Debug.Log($"Cancel button pressed");
        StartCoroutine(BattleSystemV2.Instance.CancelSwitch());
        CloseMenu();
    }

    public void SwapPokemonInSlots(int slot1, int slot2)
    {
        return;
    }

    public void CloseMenu()
    {
        currentSwapType = SwapType.None;
        List<Image> partyMemberPanel = new List<Image>();
        for (int i = 0; i < 6; i++)
        {
            partyMemberPanel.Add(partyMenuUI.transform.GetChild(0).Find("partyslot."+i).gameObject.GetComponent<Image>());
            try
            {
                partyMemberPanel[i].transform.transform.Find("buttonSwitch").GetComponentInParent<Button>()
                    .onClick.RemoveAllListeners();
            }
            catch (Exception) {}
        }
        menuUI.SetActive(false);
    }

    //static functions for general UI
    public static (string, Color) GetGenderStringColor(PartyPokemon pokemon)
    {
        switch (pokemon.gender)
        {
            case Gender.Male:
                return ("♂", new Color32(0x08, 0x12, 0xFF, 0xFF));
            case Gender.Female:
                return ("♀", new Color32(0xFF, 0x14, 0x93, 0xFF));
        }
        return ("", Color.black);
    }

    public static (string, Color) GetStatusString(PartyPokemon pokemon)
    {
        switch (pokemon.status)
        {
            case (Status.BadlyPoisoned):
                return ("TOX", Color.magenta);
            case (Status.Poisoned):
                return ("PSN", Color.magenta);
            case (Status.Burnt):
                return ("BRN", Color.red);
            case (Status.Frozen):
                return ("FRZ", Color.cyan);
            case (Status.Paralyzed):
                return ("PRZ", Color.yellow);
            case (Status.Sleep):
                return ("SLP", Color.gray);
        }
        return ("", Color.black);
    }
}
