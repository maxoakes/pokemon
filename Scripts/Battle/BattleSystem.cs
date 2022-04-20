// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using UnityEngine.SceneManagement;

// public class BattleSystem : MonoBehaviour
// {
//     public GameObject partySelectionScreen;
//     private static GameObject activeMenuOverlay;
//     private static bool menuBackButtonPressed;
//     public GameObject allyPrefab;
//     public GameObject foePrefab;
//     public Transform allyPlacement;
//     public Transform foePlacement;    
//     private static BattlePokemon allyPokemon;
//     private static BattlePokemon foePokemon;
//     public BattleHUD allyHUD;
//     public BattleHUD foeHUD;
//     public TextMeshProUGUI dialogueText;
//     public List<Button> moveButtons;
//     public Button partyButton; //party
//     public Button itemsButton; //item
//     public Button fleeButton; //run

//     //battle settings
//     private static float WAIT_TIME = 1.5f;
//     private static int turn;
//     private static BattleState state;
//     private static Weather currentWeather;
//     private static BattleAction allySelectedAction;
//     private static BattleAction foeSelectedAction;
//     private static AI foeAI;

//     //TODO: for double battles, use list for ally/foe pokemon and lists for selected actions

//     void Start()
//     {
//         foeHUD.gameObject.SetActive(false);
//         allyHUD.gameObject.SetActive(false);
//         dialogueText.gameObject.SetActive(false);
//         foreach (Button button in moveButtons)
//         {
//             button.gameObject.SetActive(false);
//         }
//         partyButton.gameObject.SetActive(false);
//         itemsButton.gameObject.SetActive(false);
//         fleeButton.gameObject.SetActive(false);
//         Debug.Log("Starting Battle Script");
//         state = BattleState.Setup;
//         StartCoroutine(SetupPhase());
//     }

//     IEnumerator SetupPhase()
//     {
//         //set up opening dialogue, foe AI and foe pokemon and HUD
//         dialogueText.gameObject.SetActive(true);
//         if (GameManager.Instance.currentBattleType == BattleType.Trainer)
//         {
//             //TODO: slide in ally trainer from side
//             //TODO: if trainer, slide in trainer from side
//             //TODO: wait for both trainers to slide into scene
//             dialogueText.text = $"{GameManager.Instance.currentFoeTrainer} wants to battle!";
//             //TODO: slide out foe trainer
//             yield return new WaitForSeconds(WAIT_TIME);
//             yield return StartCoroutine(SummonPokemon(Team.Foe, 0));
//         }
//         else
//         {
//             dialogueText.text = $"A wild {foePokemon.displayName} appeared!";
//             yield return StartCoroutine(SummonPokemon(Team.Foe, 0));
//             //TODO: slide in ally trainer
//         }
//         //TODO: slide out ally trainer
//         //set up ally pokemon and hud
//         yield return StartCoroutine(SummonPokemon(Team.Ally, 0));
        
//         turn = 0;
//         currentWeather = GameManager.Instance.weather;

//         yield return new WaitForSeconds(WAIT_TIME);
//         foreach (Button button in moveButtons)
//         {
//             button.gameObject.SetActive(true);
//         }
//         partyButton.gameObject.SetActive(true);
//         itemsButton.gameObject.SetActive(true);
//         fleeButton.gameObject.SetActive(true);
//         StartCoroutine(SelectionPhase());
//     }

//     IEnumerator SummonPokemon(Team team, int partyIndex)
//     {
//         if (team == Team.Foe)
//         {
//             GameObject foeGameObject = Instantiate(foePrefab, foePlacement);
//             foePokemon = foeGameObject.GetComponent<BattlePokemon>();
//             if (GameManager.Instance.currentBattleType == BattleType.Wild)
//             {
//                 foePokemon.SetPokemon(GameManager.Instance.currentWildEncounter[partyIndex], Team.Foe, partyIndex);
//                 foeAI = AI.Random;
//             }
//             else
//             {
//                 //TODO: pokeball deploy animation
//                 foePokemon.SetPokemon(GameManager.Instance.currentFoeTrainer.party[partyIndex], Team.Foe, partyIndex);
//                 dialogueText.text = $"{GameManager.Instance.currentFoeTrainer} sent out {foePokemon.displayName}!";
//                 foeAI = AI.Random;
//             }
//             foeHUD.gameObject.SetActive(true);
//             foeHUD.SetHUD(foePokemon);
            
//         }
//         else
//         {
//             //TODO: pokeball deploy animation
//             yield return new WaitForSeconds(WAIT_TIME);
//             GameObject allyGameObject = Instantiate(allyPrefab, allyPlacement);
//             allyPokemon = allyGameObject.GetComponent<BattlePokemon>();
//             allyPokemon.SetPokemon(GameManager.Instance.activeParty[0], Team.Ally, partyIndex);
//             dialogueText.text = $"Go, {allyPokemon.displayName}!";

//             allyHUD.gameObject.SetActive(true);
//             allyHUD.SetHUD(allyPokemon);
            
//         }
//         yield return new WaitForSeconds(WAIT_TIME);
//     }

//     IEnumerator RecallPokemon(Team team)
//     {
//         StartCoroutine(ClearPokemon(team));
//         if (team == Team.Foe)
//         {
//             if (foePokemon.currentHP < 1)
//             {
//                 //take back pokemon
//                 //if more pokemon in party, send out another
//                 //if no more pokemon, do EndBattle
//             }
//             else
//             {
//                 dialogueText.text = $"Come back {foePokemon.basePartyPokemon.GetName()}";
//             }
//         }
//         else
//         {
//             if (allyPokemon.currentHP < 1)
//             {
//                 dialogueText.text = $"Good try {allyPokemon.basePartyPokemon.GetName()}";
//                 //take back pokemon
//                 //if more pokemon in party, send out another
//                 //if no more pokemon, do EndBattle
//             }
//             else
//             {
//                 dialogueText.text = $"Come back {allyPokemon.basePartyPokemon.GetName()}";
//             }
//         }
//         yield return new WaitForSeconds(WAIT_TIME);
//     }
    
//     IEnumerator ClearPokemon(Team team)
//     {
//         //TODO: pokeball retrieval effect
//         if (team == Team.Foe)
//         {
//             foeHUD.gameObject.SetActive(false);
//             Destroy(foePokemon);
//         }
//         else
//         {
//             allyHUD.gameObject.SetActive(false);
//             Destroy(allyPokemon);
//         }
//         yield return new WaitForSeconds(WAIT_TIME);
//     }

//     IEnumerator InstantiatePartyMenuAndWaitForPress()
//     {
//         activeMenuOverlay = Instantiate(partySelectionScreen);
//         List<Image> partyMemberPanel = new List<Image>();
//         for (int i = 0; i < 6; i++)
//         {
//             partyMemberPanel.Add(activeMenuOverlay.transform.Find("slot"+i).gameObject.GetComponent<Image>());
//             try
//             {
//                 partyMemberPanel[i].transform.Find("sprite").GetComponent<Image>().color = new Color(.2f,.3f,.4f,.8f);
//                 Debug.Log("found sprite of " + i);
//                 partyMemberPanel[i].transform.Find("name").GetComponent<Text>().text = GameManager.Instance.activeParty[i].GetName();
//                 Debug.Log("found name of " + i);
//                 partyMemberPanel[i].transform.Find("level").GetComponent<Text>().text = "Lvl: " + GameManager.Instance.activeParty[i].GetLevel();
//                 Debug.Log("found level of " + i);
//                 partyMemberPanel[i].transform.Find("status").GetComponent<Text>().text = GameManager.Instance.activeParty[i].status.ToString();
//                 Debug.Log("found status of " + i);
//                 partyMemberPanel[i].transform.Find("hpText").GetComponent<Text>().text = "HP: " + GameManager.Instance.activeParty[i].GetCurrentHP() + "/" + GameManager.Instance.activeParty[i].CalculateActualStatByID(1);
//                 Debug.Log("found hpText of " + i);
//                 Slider hp = partyMemberPanel[i].transform.Find("hpBar").GetComponent<Slider>();
//                 hp.minValue = 0;
//                 hp.maxValue = GameManager.Instance.activeParty[i].CalculateActualStatByID(1);
//                 hp.value = GameManager.Instance.activeParty[i].GetCurrentHP();
//                 Debug.Log("found status of " + i);
//                 partyMemberPanel[i].gameObject.SetActive(true);
//                 if (allyPokemon.partyIndex == i) partyMemberPanel[i].transform.Find("switch").GetComponent<Button>().interactable = false;
//             }
//             catch (Exception)
//             {
//                 Debug.Log("could not find all of " + i);
//                 partyMemberPanel[i].gameObject.SetActive(false);
//             }
//         }
//         Button cancel = activeMenuOverlay.transform.Find("back").GetComponentInParent<Button>();
//         cancel.onClick.AddListener(CloseActiveMenu);

//         menuBackButtonPressed = false;
//         if (state == BattleState.Selection)
//         {
//             cancel.interactable = true;
//             //yield return new WaitUntil(() => (allySelectedAction != null || menuBackButtonPressed));
            
//         }
//         else if (state == BattleState.Action)
//         {
//             cancel.interactable = false;
//             //yield return new WaitUntil(() => (allySelectedAction != null || menuBackButtonPressed));
//         }
//         //CloseActiveMenu();
//         yield break;
//     }

//     private void PlayerUISelectionPhase()
//     {
//         //setting selection phase UI
//         dialogueText.text = "Select an action.";
//         for (int i = 0; i < 4; i++)
//         {
//             try
//             {
//                 string moveString = $"{allyPokemon.basePartyPokemon.GetMoves()[i].Item3.name}\n{allyPokemon.basePartyPokemon.GetMoves()[i].Item1}/{allyPokemon.basePartyPokemon.GetMoves()[i].Item2}";
//                 moveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = moveString;
//                 moveButtons[i].interactable = true;
//             }
//             catch (Exception)
//             {
//                 Debug.Log("Ally pokemon does not have 4 moves. One move button is disabled.");
//                 moveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
//                 moveButtons[i].interactable = false;
//             }
//         }
//         partyButton.interactable = true;
//         itemsButton.interactable = true;
//         fleeButton.interactable = true;
//         //TODO: if all pp=0, make big button for struggle move
//     }
//     IEnumerator SelectionPhase()
//     {
//         turn++;
//         allySelectedAction = null;
//         foeSelectedAction = null;
//         state = BattleState.Selection;
//         Debug.Log("Starting selection phase");
//         PlayerUISelectionPhase();

//         Debug.Log("Performing foe AI selection turn.");
//         PerformAISelectionPhase();

//         //yield return new WaitForSeconds(WAIT_TIME);
//         Debug.Log("Awaiting player input.");
//         //await player input
//         //logically go to OnMoveButton and OnPartyButton, then return here when an action is pressed
//         yield return new WaitUntil(() => (allySelectedAction != null && foeSelectedAction != null));
//         //disable all buttons
//         for (int i = 0; i < 4; i++) moveButtons[i].interactable = false;
//         partyButton.interactable = false;
//         itemsButton.interactable = false;
//         fleeButton.interactable = false;
//         StartCoroutine(ActionPhase()); //both parties have locked in a move, so the selection phase has ended.
//     }

//     public void OnMoveButton(int slot)
//     {
//         if (state != BattleState.Selection) return;
//         allySelectedAction = new BattleAction(
//             action: ActionType.Fight,
//             attackingTeam: Team.Ally,
//             targetTeam: Team.Foe,
//             move: allyPokemon.basePartyPokemon.GetMoves()[slot].Item3,
//             moveSlot: slot);
//     }

//     public void OnPartyButton()
//     {
//         StartCoroutine(InstantiatePartyMenuAndWaitForPress());
//     }

//     public void OnPartySwitchButton(int slot)
//     {
//         Debug.Log("switch out with " + GameManager.Instance.activeParty[slot].GetName());
//         allySelectedAction = new BattleAction(
//             action: ActionType.Party,
//             attackingTeam: Team.Ally,
//             targetTeam: Team.Ally);
//         CloseActiveMenu();
//     }

//     IEnumerator ActionPhase()
//     {
//         //TODO: for multi-battles
//             //for each selected action
//                 //for int i from 10 to -10
//                     //if action has priority i
//                         //add to list l
//                             //for each a in l
//                                 //sort by pokemon speed
//                             //enqueue in order of descending speeds for each priority
//         Debug.Log("Starting action phase of turn " + turn);
//         Debug.Log($"AllyPriority:{allySelectedAction.priority}. FoePriority:{foeSelectedAction.priority}.");
//         state = BattleState.Action;
//         Queue<BattleAction> actionQueue = new Queue<BattleAction>();
//         if (allySelectedAction.priority > foeSelectedAction.priority) //ally has higher priority
//         {
//             actionQueue.Enqueue(allySelectedAction);
//             actionQueue.Enqueue(foeSelectedAction);
//         }            
//         else if (allySelectedAction.priority < foeSelectedAction.priority) //foe has higher priority
//         {
//             actionQueue.Enqueue(foeSelectedAction);
//             actionQueue.Enqueue(allySelectedAction);
//         }
            
//         else //same priority, so the speedier pokemon will go first
//         {
//             float allyEffectiveSpeed = allyPokemon.basePartyPokemon.CalculateActualStatByID(6)*allyPokemon.speedMultiplier;
//             float foeEffectiveSpeed = foePokemon.basePartyPokemon.CalculateActualStatByID(6)*foePokemon.speedMultiplier;
//             Debug.Log($"AllySpeed:{allyEffectiveSpeed}. FoeSpeed:{foeEffectiveSpeed}");

//             if (allyEffectiveSpeed > foeEffectiveSpeed || allyEffectiveSpeed == foeEffectiveSpeed)
//             {
//                 actionQueue.Enqueue(allySelectedAction);
//                 actionQueue.Enqueue(foeSelectedAction);
//             }
//             else
//             {           
//                 actionQueue.Enqueue(foeSelectedAction);
//                 actionQueue.Enqueue(allySelectedAction);
//             }
//         }

//         foreach (BattleAction action in actionQueue)
//         {
//             BattlePokemon attacker;
//             BattleHUD attackerHUD;
//             if (action.attackingTeam == Team.Ally)
//             {
//                 attacker = allyPokemon;
//                 attackerHUD = allyHUD;
//             }
//             else
//             {
//                 attacker = foePokemon;
//                 attackerHUD = foeHUD;
//             }

//             BattlePokemon target;
//             BattleHUD targetHUD;
//             if (action.targetTeam == Team.Ally)
//             {
//                 target = allyPokemon;
//                 targetHUD = allyHUD;
//             }
//             else
//             {
//                 target = foePokemon;
//                 targetHUD = foeHUD;
//             }
//             if (action.action == ActionType.Fight)
//             {
//                 yield return StartCoroutine(PerformAttack(attacker, attackerHUD, target, targetHUD, action.move));
//                 if (!action.move.identifier.Equals("struggle"))
//                 {
//                 if (target.basePartyPokemon.ability.identifier.Equals("pressure")) attacker.basePartyPokemon.ReduceMovePP(action.moveSlot, 2);
//                     else attacker.basePartyPokemon.ReduceMovePP(action.moveSlot, 1);
//                 }
//             }
//             else if (action.action == ActionType.Party)
//             {
//                 yield return StartCoroutine(RecallPokemon(Team.Ally));
//                 yield return StartCoroutine(SummonPokemon(Team.Ally, action.swapWithPartyIndex));
//             }

//             //update base party pokemon
//             GameManager.Instance.activeParty[allyPokemon.partyIndex] = allyPokemon.basePartyPokemon;
//             GameManager.Instance.currentFoeTrainer.party[foePokemon.partyIndex] = foePokemon.basePartyPokemon;

//             if (IsFaintedPokemonOnField())
//             {
//                 yield return StartCoroutine(SwapOutFainted());
//                 yield break;
//             }
//             yield return new WaitForSeconds(WAIT_TIME);
//         }
//         Debug.Log("Ending action phase");
//         yield return StartCoroutine(PostActionPhase(new List<Team>{Team.Ally, Team.Foe}));
//     }

//     IEnumerator PostActionPhase(List<Team> affectedTeams)
//     {
//         Debug.Log("Start Post-action phase");
//         //TODO:
//         //post-action phase
//             //apply any effects that are post-turn
//             //if pokemon faints, action phase ends
//                 //if player, select next pokemon
//                     //if no more pokemon, end battle
//                         //if wild, done
//                         //if trainer, give money
//                 //if foe, participating pokemon get experience
//                     //if wild, done,
//                     //if trainer, rotate to next pokemon
//                         //if trainer has no more, get money and done
//         Debug.Log("End Post-action phase and turn " + turn);
//         yield return StartCoroutine(SelectionPhase());
//     }
//     IEnumerator PerformAttack(BattlePokemon attacker, BattleHUD attackerHUD, BattlePokemon target, BattleHUD targetHUD, Move move)
//     {
//         Debug.Log($"Performing attack from {attacker.basePartyPokemon.GetName()} to {target.basePartyPokemon.GetName()} using {move.name}");
//         if (attacker.currentHP < 1)
//         {
//             Debug.Log("Attacking pokemon is dead");
//             yield break;
//         }
        
//         if (target.currentHP < 1)
//         {
//             dialogueText.text = $"Target has already fainted!";
//             yield break;
//         }
//         else
//         {
//             dialogueText.text = $"{attacker.basePartyPokemon.GetName()} used {move.name}!";
//         }

//         //TODO: wait and do move vfx
//         yield return new WaitForSeconds(WAIT_TIME);

//         //check if move is physical or special
//         if (move.damageClass == DamageClass.Physical || move.damageClass == DamageClass.Special)
//         {
//             Debug.Log("is a damaging move");
//             //check if move will hit and cause damage
//             float hitChance = CalculateAccuracy(attacker, target, move);
//             if (UnityEngine.Random.Range(0f, 1f) < hitChance)
//             {
//                 Debug.Log("move hit");
//                 (int damage, Effectiveness effective) = CalculateDamage(attacker, target, move);
//                 if (effective == Effectiveness.SuperEffective)
//                 {
//                     dialogueText.text = "It was super-effective!";
//                     Debug.Log("is super effective");
//                 }
//                 else if (effective == Effectiveness.NotEffective)
//                 {
//                     dialogueText.text = "It was not very effective.";
//                     Debug.Log("is not effective");
//                 }
//                 //TODO: slowly drain HUD HP bar for vfx
//                 int remainingHP = 0; //update BattlePokemon HP
//                 target.basePartyPokemon.UpdateHP(remainingHP); //update PartyPokemon HP
//                 targetHUD.SetHP(target.currentHP);
                
//                 yield return new WaitForSeconds(WAIT_TIME);
//             }
//             else
//             {
//                 Debug.Log("move miss");
//                 dialogueText.text = $"{move.name} missed!";
//                 yield return new WaitForSeconds(WAIT_TIME);
//             }
//         }
//         else
//         {
//             bool successful = true;
//             if (successful)
//             {
//                 //TODO: move vfx
//                 yield return new WaitForSeconds(WAIT_TIME);
//                 //move is a status move
//                 Debug.Log("status move");
//                 //do effect
//             }
//             else
//             {
//                 dialogueText.text = $"But it failed.";
//             }
//         }
//     }
//     private bool IsFaintedPokemonOnField()
//     {
//         return (foePokemon.IsFainted() || allyPokemon.IsFainted());
//     }
//     IEnumerator SwapOutFainted()
//     {
//         List<Team> aliveTeam = new List<Team>{Team.Ally, Team.Foe};
//         //for each foe pokemon, swap then out
//         if (foePokemon.IsFainted())
//         {
//             aliveTeam.Remove(Team.Foe);
//             if (GameManager.Instance.currentBattleType == BattleType.Trainer)
//             {
//                 dialogueText.text = $"Foe {foePokemon.basePartyPokemon.GetName()} fainted!";
//                 yield return new WaitForSeconds(WAIT_TIME);

//                 //check if there are any remaining pokemon that can fight
//                 List<int> validIndex = new List<int>();
//                 for (int i = 0; i < GameManager.Instance.currentFoeTrainer.party.Count ; i++)
//                 {
//                     if (GameManager.Instance.currentFoeTrainer.party[i].UsableInBattle()) validIndex.Add(i);
//                 }
//                 Debug.Log($"Foe trainer has {validIndex.Count} valid pokemon left");
//                 if (validIndex.Count > 0)
//                 {
//                     //there is at least one valid pokemon left
//                     yield return StartCoroutine(FoeAISwitchPokemon(validIndex[0]));
//                 }
//                 else
//                 {
//                     //there are no more valid pokemon left
//                     state = BattleState.Win;
//                     yield return StartCoroutine(EndBattle());
//                 }
//             }
//             else
//             {
//                 state = BattleState.Win;
//                 dialogueText.text = $"Wild {foePokemon.basePartyPokemon.GetName()} fainted!";
//                 //TODO: for double battles, do further checking for other pokemon
//                 yield return new WaitForSeconds(WAIT_TIME);
//                 yield return StartCoroutine(EndBattle());
//             }
//         }
//         //if ally is fainted
//         if (allyPokemon.currentHP < 1)
//         {
//             aliveTeam.Remove(Team.Ally);
//             dialogueText.text = $"{allyPokemon.basePartyPokemon.GetName()} fainted!";
//             yield return new WaitForSeconds(WAIT_TIME);
//             bool canSwitchOut = false;
//             foreach (PartyPokemon p in GameManager.Instance.activeParty)
//             {
//                 if (p.UsableInBattle())
//                 {
//                     canSwitchOut = true;
//                     break;
//                 }
//             }
//             if (canSwitchOut)
//             {
//                 StartCoroutine(InstantiatePartyMenuAndWaitForPress());
//             }
//             else
//             {
//                 state = BattleState.Loss;
//                 dialogueText.text = $"All of your Pokemon have fainted!";
//                 yield return StartCoroutine(EndBattle());
//             }
//         }
//         yield return new WaitForSeconds(WAIT_TIME);
//         yield return StartCoroutine(PostActionPhase(aliveTeam));
//     }

//     IEnumerator FoeAISwitchPokemon(int index)
//     {
//         yield return StartCoroutine(RecallPokemon(Team.Foe));
//         yield return StartCoroutine(SummonPokemon(Team.Foe, index));
//     }

//     //Calcuate the damage based on the attacker and the target
//     private (int, Effectiveness) CalculateDamage(BattlePokemon attacker, BattlePokemon target, Move move)
//     {
//         float level = attacker.level;
//         float power = move.power;
//         float attack = 0f;
//         float defense = 0f;
//         switch (move.damageClass)
//         {
//             case DamageClass.Status:
//                 return (0, Effectiveness.None);
//             case DamageClass.Physical:
//                 attack = attacker.basePartyPokemon.CalculateActualStatByID(2)*attacker.attackMultiplier;
//                 defense = target.basePartyPokemon.CalculateActualStatByID(3)*target.defenseMultiplier;
//                 break;
//             case DamageClass.Special:
//                 attack = attacker.basePartyPokemon.CalculateActualStatByID(4)*attacker.spAttackMultiplier;
//                 defense = target.basePartyPokemon.CalculateActualStatByID(5)*target.spDefenseMultiplier;
//                 break;
//         }

//         //get weather multiplier
//         float targetMultiplier = 1f;
//         float weatherMultiplier = 1f;
//         PokemonType moveType = GameManager.Instance.registry.types[move.typeID];
//         PokemonType attackerType1 = attacker.basePartyPokemon.basePokemon.GetType1();
//         PokemonType attackerType2 = attacker.basePartyPokemon.basePokemon.GetType2();
//         PokemonType targetType1 = target.basePartyPokemon.basePokemon.GetType1();
//         PokemonType targetType2 = target.basePartyPokemon.basePokemon.GetType2();
//         if (currentWeather == Weather.Rainy)
//         {
//             if (moveType.identifier.Equals("water"))
//             {
//                 Debug.Log("water move used in rain");
//                 weatherMultiplier = 1.5f;
//             }
//             if (moveType.identifier.Equals("fire"))
//             {
//                 Debug.Log("fire move used in rain");
//                 weatherMultiplier = 0.5f;
//             }
//         }
//         if (currentWeather == Weather.HarshSun)
//         {
//             if (moveType.identifier.Equals("water"))
//             {
//                 Debug.Log("water move used in harshsun");
//                 weatherMultiplier = 0.5f;
//             }
//             if (moveType.identifier.Equals("fire"))
//             {
//                 Debug.Log("fire move used in harshsun");
//                 weatherMultiplier = 1.5f;
//             }
//         }

//         //get critical hit chance and multiplier
//         float crit = (UnityEngine.Random.Range(0f, 1f) < attacker.GetCritChance()) ? 1.5f : 1f;

//         //get the random multiplier
//         float rand = (UnityEngine.Random.Range(85, 101)/100f); //flail, future sight, reversal are exempt

//         //get same-type/attack-bonus multiplier
//         float stab = 1f;
//         if (attackerType1.id == move.typeID) stab = 1.5f;
//         try
//         {
//             if (attackerType2.id == move.typeID) stab = 1.5f;
//         }
//         catch (Exception)
//         {
//             Debug.Log($"{attacker.basePartyPokemon.GetName()} does not have a secondary type");
//         }
//         if (attacker.basePartyPokemon.ability.identifier.Equals("adaptability") && stab > 1.1f) stab = 2f;

//         //get multiplier from target type
//         float typeEffectiveness = GameManager.Instance.registry.types[move.typeID].GetDamageFactor(targetType1.id)/100f;
//         Effectiveness eff = Effectiveness.None;

//         if (targetType2 != null)
//         {
//             typeEffectiveness *= GameManager.Instance.registry.types[move.typeID].GetDamageFactor(targetType2.id)/100f;
//         }
//         if (typeEffectiveness > 1.1f) eff = Effectiveness.SuperEffective;
//         else if (typeEffectiveness < 0.9f) eff = Effectiveness.NotEffective;

//         //get burned attacker multiplier
//         //Burn is 0.5 (from Generation III onward) if the attacker is burned, its Ability is not Guts,
//         //and the used move is a physical move (other than Facade from Generation VI onward), and 1 otherwise.
//         float burn = (
//             attacker.status == Status.Burnt &&
//             !attacker.basePartyPokemon.ability.identifier.Equals("guts") &&
//             (move.damageClass == DamageClass.Physical && !move.identifier.Equals("facade"))) ? 0.5f : 1f;
        
//         //TODO: item special cases
//         float other = 1f;
//         int damage = (int)((((((2*level/5)+2)*power*(attack/defense))/50)+2)*targetMultiplier*weatherMultiplier*crit*rand*stab*typeEffectiveness*burn*other);
//         Debug.Log($"Calculated damage: {damage}");
//         return (damage, eff);
//     }

//     //based on offical logic
//     private float CalculateAccuracy(BattlePokemon attacker, BattlePokemon target, Move move)
//     {
//         if (move.accuracy == -1) return 1f; //special case, will always hit if it is a damaging attack

//         float stageModifier = 1f;
//         int stage = Mathf.Clamp(target.evasionStage - attacker.accuracyStage, -6, 6);
//         if (GameManager.Instance.GENERATION_ID <= 5)
//         {
//             switch (stage)
//             {
//                 case -6: 
//                     stageModifier = 33/100;
//                     break;
//                 case -5:
//                     stageModifier = 36/100;
//                     break;
//                 case -4:
//                     stageModifier = 43/100;
//                     break;
//                 case -3:
//                     stageModifier = 50/100;
//                     break;
//                 case -2:
//                     stageModifier = 60/100;
//                     break;
//                 case -1:
//                     stageModifier = 75/100;
//                     break;
//                 case 0:
//                     stageModifier = 1f;
//                     break;
//                 case 1:
//                     stageModifier = 133/100;
//                     break;
//                 case 2:
//                     stageModifier = 166/100;
//                     break;
//                 case 3:
//                     stageModifier = 200/100;
//                     break;
//                 case 4:
//                     stageModifier = 250/100;
//                     break;
//                 case 5:
//                     stageModifier = 266/100;
//                     break;
//                 case 6:
//                     stageModifier = 300/100;
//                     break;
//             }
//         }
//         else
//         {
//             if (stage > 0) stageModifier = (3+stage)/3;
//             else if (stage < 0) stageModifier = 3/(Mathf.Abs(stage)+3);
//             else stageModifier = 1f;
//         }

//         float modifiers = 1f; //TODO: item and ability modifiers
//         float accuracy = (move.accuracy/100f)*(stageModifier)*modifiers;
//         Debug.Log($"Accuracy calculated to {accuracy} with combined stage of {stage}");
//         return accuracy;
//     }
//     IEnumerator EndBattle()
//     {
//         if (state == BattleState.Loss)
//         {
//             yield return new WaitForSeconds(WAIT_TIME);
//             GameManager.Instance.CleanUpBattle();
//             SceneManager.LoadScene("MainMenu");
//         }
//         if (state == BattleState.Win)
//         {
//             if (GameManager.Instance.currentBattleType == BattleType.Trainer)
//             {
//                 dialogueText.text = $"{GameManager.Instance.currentFoeTrainer} has been defeated!";
//                 //TODO: foe trainer gives money
//                 yield return new WaitForSeconds(WAIT_TIME);
//                 GameManager.Instance.CleanUpBattle();
//             }
//             else
//             {
//                 //if a wild pokemon is defeated, do nothing more
//                 GameManager.Instance.CleanUpBattle();
//             }
//         }
//         SceneManager.LoadScene("MainMenu");
//     }

//     private void PerformAISelectionPhase()
//     {
//         foeSelectedAction = new BattleAction(
//             action: ActionType.Fight,
//             attackingTeam: Team.Foe,
//             targetTeam: Team.Ally,
//             move: GameManager.Instance.registry.struggle,
//             moveSlot: 0);
//         if (foeAI == AI.Random)
//         {
//             Debug.Log("AI deciding on a random move...");
//             List<(int, int, Move)> moves = foePokemon.basePartyPokemon.GetMoves();
//             for (int i = 0; i < moves.Count; i++)
//             {
//                 if (moves[i].Item1 > 0)
//                 {
//                     foeSelectedAction = new BattleAction(
//                         action: ActionType.Fight,
//                         attackingTeam: Team.Foe,
//                         targetTeam: Team.Ally,
//                         move: moves[i].Item3,
//                         moveSlot: i);
//                     Debug.Log($"Chosen foe move is {foeSelectedAction.move.name}");
//                     break;
//                 }
//             }

//         }
//         Debug.Log("AI has picked a move.");
//     }
//     public void CloseActiveMenu()
//     {
//         Destroy(activeMenuOverlay);
//         activeMenuOverlay = null;
//     }

//     public void OnMenuBackButton()
//     {
//         menuBackButtonPressed = true;
//         CloseActiveMenu();
//     }
// }
