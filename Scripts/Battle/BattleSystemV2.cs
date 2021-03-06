using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;
using UnityEngine.SceneManagement;
using TMPro;

public enum BattleType
{
    Trainer,
    Wild
}
public enum BattleState
{
    Setup,
    Selection,
    Action,
    PostAction,
    End
}

public enum EndType
{
    Win,
    Loss,
    Flee
}
public enum Team
{
    Ally,
    Foe
}

public enum FieldEffects
{
    Gravity
}
public class BattleSystemV2 : MonoBehaviour
{
    public static BattleSystemV2 Instance;
    public static float WAIT_TIME = 0.5f;

    //Screen Elements
    public ParticleSystem pokeballParticle;
    public ParticleSystem statBuffParticle;
    public ParticleSystem statReduceParticle;
    public ParticleSystem genericAttack;
    public Transform allyPlacement;
    public Transform foePlacement;
    public Transform allyHUD3DPlacement;
    public Transform foeHUD3DPlacement;
    public GameObject spritePlane;
    public GameObject HUD3D;
    private static BattlePokemon allyPokemon;
    private static BattlePokemon foePokemon;
    private static Battle3DHUD allyHUD3D;
    private static Battle3DHUD foeHUD3D;

    //buttons and text
    public List<Button> moveButtons;
    public Button partyButton; //party
    public Button itemsButton; //item
    public Button fleeButton; //run

    //shortcuts
    private BattleType battleType;
    private Trainer player;
    private Trainer foe;

    //battle settings
    public BattleState state;
    private static int turn;
    public Weather currentWeather;
    public List<FieldEffects> currentFieldEffects;
    private static BattleAction allySelectedAction;
    private static BattleAction foeSelectedAction;
    private List<PartyPokemon> foePokemonFaintedThisTurn;
    public bool UISwitchPerformed;

    void Start()
    {
        Debug.Log("BattleSystemV2 has started.");
        DontDestroyOnLoad(this.gameObject);
        if (BattleSystemV2.Instance != null) Destroy(BattleSystemV2.Instance.gameObject);
        Instance = this;

        //start with blank scene, only background
        GameManager.Instance.gameMode = Mode.Battle;
        SetButtonVisable(false);
        SetButtonsClickable(false);
        currentFieldEffects = new List<FieldEffects>();
        foePokemonFaintedThisTurn = new List<PartyPokemon>();
        StartCoroutine(SetupPhase());        
    }

    /*
    .########..##.....##....###.....######..########..######.
    .##.....##.##.....##...##.##...##....##.##.......##....##
    .##.....##.##.....##..##...##..##.......##.......##......
    .########..#########.##.....##..######..######....######.
    .##........##.....##.#########.......##.##.............##
    .##........##.....##.##.....##.##....##.##.......##....##
    .##........##.....##.##.....##..######..########..######.
    */

    IEnumerator SetupPhase()
    {
        foe = GameManager.Instance.foeTrainer;
        player = GameManager.Instance.player;
        battleType = GameManager.Instance.currentBattleType;
        state = BattleState.Setup;
        currentWeather = GameManager.Instance.weather;
        turn = 0;

        if (battleType == BattleType.Trainer)
        {
            StartCoroutine(SummonTrainer(foe));
            yield return StartCoroutine(Typewriter.Instance.WriteText($"{foe} wants to battle!"));
        }
        yield return StartCoroutine(SummonTrainer(player));
        if (battleType == BattleType.Trainer) yield return StartCoroutine(RecallTrainer(foe));
        yield return StartCoroutine(SummonPokemon(foe, 0));
        yield return StartCoroutine(RecallTrainer(player));
        yield return StartCoroutine(SummonPokemon(player, 0));

        while (state != BattleState.End)
        {
            turn++;
            foePokemonFaintedThisTurn.Clear();
            Debug.Log($"*************************** Starting turn {turn}. ***************************");
            yield return StartCoroutine(PreTurnPhase());
            SelectionPhase();
            yield return StartCoroutine(AwaitSelections());
            yield return StartCoroutine(ActionPhase());
            yield return StartCoroutine(PostActionPhase());
            yield return StartCoroutine(SwitchOutPhase());
            yield return null;
        }
    }

    IEnumerator PreTurnPhase()
    {
        List<BattlePokemon> allPokemon = GetAllFieldPokemon();
        foreach (BattlePokemon pokemon in allPokemon)
        {
            if (pokemon.newToField)
            {
                // TODO: Effect of Toxic Spikes
                // TODO: Effect of Spikes
                // TODO: Effect of Stealth Rock
                // TODO: Effect of Lunar Dance/Healing Wish
                // TODO: Effects from abilities like Download and Intimidate are applied. If this is the end of the turn, other Pok??mon that have Slow Start and didn???t just enter the battle have their Slow Start counter reduced by 1, if not already done this turn.
                // TODO: Form changes, including Forecast, are applied.
                // TODO: Effects from certain held items are applied.
                // TODO: If more than one Pok??mon is chosen when Pok??mon are switched in from fainted ones, the above effects don???t happen until all those Pok??mon enter the battle (in turn order for the new Pok??mon, taking into account effects like Quick Claw and Custap Berry that influence priority, except that Custap Berry is not consumed after use). At any other time when more than one Pok??mon is chosen, the effects happen immediately after the Pok??mon enters the battle.
                // TODO: At the beginning of a battle, effects from abilities, form changes, and held items resolve for each Pok??mon in turn order, taking into account effects like Quick Claw and Custap Berry that influence priority, except that Custap Berry is not consumed after use.
                pokemon.newToField = false;
            }
        }
        yield return null;
    }
    private void SelectionPhase()
    {
        Debug.Log("Starting selection phase");
        state = BattleState.Selection;
        allySelectedAction = null;
        foeSelectedAction = null;

        PlayerUISelectionPhase();
        AISelectionPhase();        
    }
    private void PlayerUISelectionPhase()
    {
        SetButtonVisable(true);
        SetButtonsClickable(true);
        Typewriter.Instance.SetText("Select an action.");
        for (int i = 0; i < 4; i++)
        {
            try
            {
                (int pp, int ppm, Move m) move = allyPokemon.basePartyPokemon.GetMoves()[i];
                string moveString = $"{move.m.name}\n{move.pp}/{move.ppm}";
                moveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = moveString;
                moveButtons[i].GetComponent<Image>().color = GameManager.Instance.registry.GetTypeByID(move.m.typeID).color;;
                // ColorBlock cb = moveButtons[i].GetComponent<Button>().colors;
                // cb.normalColor = GameManager.Instance.registry.GetTypeByID(move.m.typeID).color;
                if (allyPokemon.basePartyPokemon.GetMoves()[i].Item1 <= 0) moveButtons[i].interactable = false;
            }
            catch (Exception)
            {
                moveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                ColorBlock cb = moveButtons[i].GetComponent<Button>().colors;
                cb.normalColor = Color.white;
                moveButtons[i].interactable = false;
            }
        }
        partyButton.interactable = true;
        itemsButton.interactable = true;
        fleeButton.interactable = (battleType == BattleType.Wild);
        //TODO: if all pp=0, make big button for struggle move
    }
    private void AISelectionPhase()
    {
        if (foe.battleAI == AI.Random)
        {
            List<(int, int, Move)> moves = foePokemon.basePartyPokemon.GetMoves();
            List<Move> selectableMoves = new List<Move>();
            for (int i = 0; i < moves.Count; i++)
            {
                if (moves[i].Item1 > 0)
                {
                    selectableMoves.Add(moves[i].Item3);
                }
            }
            if (selectableMoves.Count > 0)
            {
                int selectedSlot = UnityEngine.Random.Range(0, selectableMoves.Count-1);
                foeSelectedAction = new BattleAction(
                        action: ActionType.Fight,
                        attackingPokemon: foePokemon,
                        targetTeam: Team.Ally,
                        targetIndex: 0,
                        move: moves[selectedSlot].Item3,
                        moveSlot: selectedSlot);
            }
            else
            {
                foeSelectedAction = new BattleAction(
                    action: ActionType.Fight,
                    attackingPokemon: foePokemon,
                    targetTeam: Team.Ally,
                    targetIndex: 0,
                    move: GameManager.Instance.registry.struggle,
                    moveSlot: 0);
            }
        }
        Debug.Log($"Chosen foe move is {foeSelectedAction.move.name}");
    }
    public void OnMoveButton(int slot)
    {
        if (state != BattleState.Selection) return;
        allySelectedAction = new BattleAction(
            action: ActionType.Fight,
            attackingPokemon: allyPokemon,
            targetTeam: Team.Foe,
            targetIndex: 0,
            move: allyPokemon.basePartyPokemon.GetMoves()[slot].m,
            moveSlot: slot);
        Debug.Log("Move Pressed " + allySelectedAction);
        SetButtonsClickable(false);
        SetButtonVisable(false);
    }

    public void OnPartyButton()
    {
        PartyMenu.Instance.currentSwapType = SwapType.BattleActionSwap;
        PartyMenu.Instance.ShowMenu();
    }

    public void OnBagButton()
    {
        //PartyMenu.Instance.ShowMenu();
    }

    public void OnRunButton()
    {
        if (state != BattleState.Selection) return;
        allySelectedAction = new BattleAction(
            action: ActionType.Run,
            attackingPokemon: allyPokemon,
            targetTeam: Team.Foe,
            targetIndex: 0);
        Debug.Log("Flee pressed");
        SetButtonsClickable(false);
        SetButtonVisable(false);
    }

    public void SetPartySwitchAction(int slot)
    {
        if (state != BattleState.Selection) return;
        PartyMenu.Instance.currentSwapType = SwapType.BattleActionSwap;
        allySelectedAction = new BattleAction(
            action: ActionType.Party,
            attackingPokemon: allyPokemon,
            targetTeam: Team.Ally,
            targetIndex: 0,
            partyIndex: slot);
        Debug.Log("Party Switch selected " + allySelectedAction);
        SetButtonsClickable(false);
    }
    public IEnumerator SwapCurrentPokemon(int slot)
    {
        //TODO: Before Pok??mon switch out at the switching-out phase of the turn, a message is shown for each Pok??mon that chose Focus Punch for use in turn order.
        yield return StartCoroutine(RecallPokemon(allyPokemon));
        yield return StartCoroutine(SummonPokemon(player, slot));
        UISwitchPerformed = true;
    }
    //get the submitted actions of all pokemon on the field and put them into a queue order by action priority, then pokemon speed
    private Queue<BattleAction> GetActionQueue()
    {
        //Create action queue
        Queue<BattleAction> actionQueue = new Queue<BattleAction>();
        if (allySelectedAction.priority > foeSelectedAction.priority) //ally has higher priority
        {
            actionQueue.Enqueue(allySelectedAction);
            actionQueue.Enqueue(foeSelectedAction);
        }            
        else if (allySelectedAction.priority < foeSelectedAction.priority) //foe has higher priority
        {
            actionQueue.Enqueue(foeSelectedAction);
            actionQueue.Enqueue(allySelectedAction);
        }
        else //same priority, so the speedier pokemon will go first
        {
            float allyEffectiveSpeed = allyPokemon.basePartyPokemon.GetStatTuple(6).actual*allyPokemon.GetStatWithMultipliers(6);
            float foeEffectiveSpeed = foePokemon.basePartyPokemon.GetStatTuple(6).actual*foePokemon.GetStatWithMultipliers(6);
            Debug.Log($"AllySpeed:{allyEffectiveSpeed}. FoeSpeed:{foeEffectiveSpeed}");

            if (allyEffectiveSpeed > foeEffectiveSpeed || allyEffectiveSpeed == foeEffectiveSpeed)
            {
                actionQueue.Enqueue(allySelectedAction);
                actionQueue.Enqueue(foeSelectedAction);
            }
            else
            {           
                actionQueue.Enqueue(foeSelectedAction);
                actionQueue.Enqueue(allySelectedAction);
            }
        }
        return actionQueue;
    }
    IEnumerator ActionPhase()
    {
        Debug.Log("Starting action phase of turn " + turn);
        state = BattleState.Action;
        Queue<BattleAction> actionQueue = GetActionQueue();

        //Iterate over action queue
        int actionsPerformedThisTurn = 0;
        foreach (BattleAction action in actionQueue)
        {
            GetPokemonAtSlot(action.targetTeam).lastMoveUsedAgainstThis = null;
            if (!action.attackingPokemon.basePartyPokemon.UsableInBattle() || !GetPokemonAtSlot(action.targetTeam).basePartyPokemon.UsableInBattle()) continue;
            Debug.Log($"Performing action from {action.attackingPokemon.basePartyPokemon.GetName()} to {GetPokemonAtSlot(action.targetTeam).basePartyPokemon.GetName()}");
            //if (!action.attackingPokemon.basePartyPokemon.UsableInBattle()) yield break;

            if (action.action == ActionType.Run) yield return StartCoroutine(PerformAttemptToFlee(action));
            if (action.action == ActionType.Bag) yield return StartCoroutine(PerformActionBag(action));
            else if (action.action == ActionType.Party) yield return StartCoroutine(PerformActionSwitch(action));
            else if (action.action == ActionType.Fight) yield return StartCoroutine(PerformActionMove(action, actionsPerformedThisTurn, 1-actionsPerformedThisTurn));
            actionsPerformedThisTurn++;

            
        }
        Debug.Log("Ending action phase");
    }

    IEnumerator PerformActionSwitch(BattleAction action)
    {
        yield return StartCoroutine(RecallPokemon(action.attackingPokemon));
        yield return StartCoroutine(SummonPokemon(action.attackingPokemon.ownerTrainer, action.swapWithPartyIndex));
    }
    IEnumerator PerformActionBag(BattleAction action)
    {
        yield return null;
    }
    IEnumerator PerformAttemptToFlee(BattleAction action)
    {
        if (battleType == BattleType.Wild)
        {
            bool canRun = false;
            int attackerSpeed = action.attackingPokemon.basePartyPokemon.GetStatTuple(6).actual;
            int targetSpeed = GetPokemonAtSlot(action.targetTeam).basePartyPokemon.GetStatTuple(6).actual;
            
            if (attackerSpeed >= targetSpeed)
            {
                canRun = true;
            }
            else
            {
                int runChance = ((int)(attackerSpeed*128/targetSpeed)+(30*action.attackingPokemon.attemptsToRun)) % 256;
                int rand = UnityEngine.Random.Range(0,256);
                if (rand > runChance) canRun = true;
                Debug.Log($"Calculating {rand} > {runChance}. canRun={canRun}");
            }
            action.attackingPokemon.attemptsToRun++;
            
            if (canRun)
            {
                state = BattleState.End;
                yield return StartCoroutine(EndBattle(EndType.Flee));
            }
        }
        else
        {
            yield return StartCoroutine(Typewriter.Instance.WriteText($"You cannot run from a trainer battle!"));
        }
    }
    IEnumerator PerformActionMove(BattleAction action, int order, int actionsRemaining)
    {
        //save some characters...
        BattlePokemon attacker = action.attackingPokemon;
        BattlePokemon target = GetPokemonAtSlot(action.targetTeam);
        string attackerName = $"{GetPrefix(attacker)}{attacker.displayName}";
        string targetName = $"{GetPrefix(target)}{target.displayName}";

        // Before a Pok??mon uses an attack, the following effects are checked, in this order:
        // Freeze / Sleep
        if (attacker.basePartyPokemon.status == Status.Frozen)
        {
            // User thaws out if Flame Wheel or Sacred Fire was chosen for use
            if (UnityEngine.Random.Range(0f, 1f) < 0.2f ||
                action.move.identifier.Equals("flame-wheel") ||
                action.move.identifier.Equals("sacred-fire"))
            {
                attacker.basePartyPokemon.status = Status.None;
                yield return SetStatus(attacker, Status.None);
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} thawed!"));
            }
            else
            {
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} is frozen solid!"));
                yield break;
            }
        }
        if (attacker.basePartyPokemon.status == Status.Sleep)
        {
            if (attacker.remainingSleepTurns == 0)
            {
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} woke up!"));
                yield return SetStatus(attacker, Status.None);
                attacker.remainingSleepTurns--;
            }
            else if (attacker.remainingSleepTurns > 0)
            {
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} is fast asleep."));
                //TODO: ZZZ particle
                Debug.Log($"attacker has {attacker.remainingSleepTurns} of sleep left");
                attacker.remainingSleepTurns--;
                yield break;
            }
        }
        // Truant
        // Disable
        // Imprison
        // Heal Block
        // Confusion
        // Flinching
        // Taunt
        // Gravity
        // Attract
        // Paralysis
        if (attacker.basePartyPokemon.status == Status.Paralyzed)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.25f)
            {
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} is paralyzed! It can't move!"));
                yield break;
            }
        }
        
        // TODO: Obedience check
        // TODO: The above check is not done if the Pok??mon uses Pursuit as another Pok??mon is about to switch.
        if (!action.move.identifier.Equals("struggle"))
        {
            if (action.attackingPokemon.basePartyPokemon.GetMoves()[action.moveSlot].pp == 0)
            {
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} is out of PP and cannot use that move!"));
                yield break;
            }
            if (target.basePartyPokemon.ability.identifier.Equals("pressure")) attacker.basePartyPokemon.ReduceMovePP(action.moveSlot, 2);
            else attacker.basePartyPokemon.ReduceMovePP(action.moveSlot, 1);
        }
        yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} used {action.move.name}!"));
        
        //TODO: targetting for multi-battles
        //If an attack has no target, the attack does nothing.
        if (!target.basePartyPokemon.UsableInBattle())
        {
            yield return StartCoroutine(Typewriter.Instance.WriteText("But it did nothing!"));
            yield break;
        }

        bool isLastHit = (actionsRemaining == 0);
        bool doesHit = (UnityEngine.Random.Range(0f, 1f) < BattleCalculator.CalculateAccuracy(attacker, target, action.move, isLastHit));
        bool isAttackMove = (action.move.damageClass == DamageClass.Physical || action.move.damageClass == DamageClass.Special);
        if (!doesHit)
        {
            if (isAttackMove) yield return StartCoroutine(Typewriter.Instance.WriteText("But it failed."));
            else yield return StartCoroutine(Typewriter.Instance.WriteText("But it missed."));
            yield break;
        }

        //If the attack hits and the attack deals damage, its damage is calculated.
        //Otherwise, if the attack hits, the attack???s effect happens, if possible.
        if (isAttackMove)
        {
            Effectiveness effective = Effectiveness.Regular;
            bool isCrit = false;
            int numHits = BattleCalculator.GetNumHits(action.move);
            int actualDamage = 0;
            int healFromDrain = 0;

            //calculate damage
            for (int i = 0; i < numHits; i++)
            {
                int startHP = target.basePartyPokemon.GetCurrentHP();
                (int damage, Effectiveness e, bool c) = BattleCalculator.CalculateDamage(attacker, target, action.move, currentWeather);
                actualDamage = Mathf.Min(target.basePartyPokemon.GetCurrentHP(), damage);
                int endHP = Mathf.Max(target.basePartyPokemon.GetCurrentHP()-actualDamage, 0);
                Debug.Log($"Resulting HP: {endHP}");

                effective = e;
                if (e == Effectiveness.Ineffective) continue;
                isCrit = c;

                yield return StartCoroutine(AnimationGenericSpecialMove(attacker.transform, target.transform, action.move));
                yield return StartCoroutine(AnimationTakeDamage(target));
                yield return StartCoroutine(AnimationChangeHP(target, endHP));
            }
            if (numHits > 1) yield return StartCoroutine(Typewriter.Instance.WriteText($"It hit {numHits} time(s)!"));
            healFromDrain = BattleCalculator.CalculateDrainAmount(action.move, actualDamage);
            if (healFromDrain != 0)
            {
                Debug.Log($"Heal amount from drain: {healFromDrain}");
                int resultingHP = Mathf.Clamp(attacker.basePartyPokemon.GetCurrentHP()+healFromDrain, 0, attacker.basePartyPokemon.GetStatTuple(1).actual);
                Debug.Log(resultingHP);
                yield return StartCoroutine(AnimationChangeHP(attacker, resultingHP));
            }
            if (healFromDrain < 0) yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName} took damage from recoil."));
            else if (healFromDrain > 0) yield return StartCoroutine(Typewriter.Instance.WriteText($"{targetName} had its energy drained!"));

            // TODO: Chople Berry and the like
            if (isCrit) yield return StartCoroutine(Typewriter.Instance.WriteText("It was a critical hit!")); // Critical hit message
            // TODO: Anger Point
            // TODO: Endure

            // Show "It???s "super effective" or "It???s not effective", as applicable
            switch (effective)
            {
                case Effectiveness.SuperEffective:
                    yield return StartCoroutine(Typewriter.Instance.WriteText("It was super-effective!"));
                    break;
                case Effectiveness.NotEffective:
                    yield return StartCoroutine(Typewriter.Instance.WriteText("It was not very effective."));
                    break;
                case Effectiveness.Ineffective:
                    yield return StartCoroutine(Typewriter.Instance.WriteText("It had no effect!"));
                    break;
                default:
                    break;
            }

            // "If this attack is successful" effects
            // Additional effects
            // An additional effect won???t occur on a target if it has zero HP remaining, but will occur on its user (such as Metal Claw???s additional effect) even if the target is defeated.
            if (IsEffectSuccessful(action)) yield return StartCoroutine(ApplyMoveEffect(action, actualDamage));

            // TODO: Rage
            // TODO: Ability effects, using the ability the target/user had when the damage was dealt
            // TODO: Enigma Berry, Jaboca Berry, Rowap Berry, Sticky Barb
            //if (!target.basePartyPokemon.UsableInBattle()) yield return StartCoroutine(PerformFaint(target));
            // The target thaws out if attack is a Fire-type attack
            if (target.basePartyPokemon.status == Status.Frozen && GameManager.Instance.registry.GetTypeByID(action.move.typeID).identifier.Equals("fire"))
            {
                SetStatus(target, Status.None);
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{attackerName}'s attack thawed {targetName}!"));
            }
            // Life Orb/Shell Bell (only if attacker has at least 0 HP)
            if (attacker.IsHoldingItem("life-orb"))
            {
                float hpLoss = (float)attacker.basePartyPokemon.GetStatTuple(1).actual*0.1f;
                int newHP = attacker.basePartyPokemon.GetCurrentHP()-(int)hpLoss;
                yield return StartCoroutine(AnimationChangeHP(attacker, Mathf.Min(1, newHP)));
            }
            if (attacker.IsHoldingItem("shell-bell"))
            {
                float hpGain = actualDamage*0.125f;
                int newHP = attacker.basePartyPokemon.GetCurrentHP()+(int)hpGain;
                yield return StartCoroutine(AnimationChangeHP(attacker, Mathf.Max(attacker.basePartyPokemon.GetStatTuple(1).actual, newHP)));
            }
            // TODO: For Selfdestruct/Explosion/Memento, the above effects resolve for each target before the user faints.
            target.lastMoveUsedAgainstThis = action.move;
        }
        else //if it is a status move
        {
            yield return StartCoroutine(AnimationGenericSelfStatusMove(attacker.gameObject));
            if (IsEffectSuccessful(action)) yield return StartCoroutine(ApplyMoveEffect(action, 0));
            else yield return StartCoroutine(Typewriter.Instance.WriteText("But it failed!"));
        }

        int heal = BattleCalculator.CalcuateHealAmount(attacker, action.move);
        if (heal > 0)
        {
            Debug.Log($"Heal amount from drain: {heal}");
            int resultingHP = Mathf.Clamp(attacker.basePartyPokemon.GetCurrentHP()+heal, 0, attacker.basePartyPokemon.GetStatTuple(1).actual);
            yield return StartCoroutine(AnimationChangeHP(attacker, resultingHP));
        }

        //self destruct effect calculation. will occur regardless of hit or miss
        if (action.move.effect.id == 8) yield return StartCoroutine(AnimationChangeHP(attacker, 0, 2f));
        yield return new WaitForSeconds(WAIT_TIME);
    }

    IEnumerator PostActionPhase()
    {
        state = BattleState.PostAction;
        List<BattlePokemon> fieldPokemon = GetAllFieldPokemon();

        //apply any post-action effect for each pokemon
        foreach (BattlePokemon pokemon in fieldPokemon)
        {
            // TODO: Reflect
            // TODO: Light Screen
            // TODO: Mist
            // TODO: Safeguard
            // TODO: Tailwind
            // TODO: Lucky Chant
            // TODO: Wish
            // TODO: Weather effect continues or ends
            // TODO: Dry Skin/Rain Dish/Hydration/Ice Body (no order between them, resolved in turn order)
            // TODO: Gravity
            // Resolved in turn order:
            // TODO: Ingrain
            // TODO: Aqua Ring
            // TODO: Shed Skin/Speed Boost/Truant
            // TODO: Black Sludge/Leftovers
            // Poison
            if (pokemon.basePartyPokemon.status == Status.Poisoned && pokemon.basePartyPokemon.UsableInBattle())
            {
                float damage = ((float)pokemon.basePartyPokemon.GetStatTuple(1).actual/8f);
                int resultingHP = Mathf.Max(pokemon.basePartyPokemon.GetCurrentHP()-(int)damage, 0);
                //TODO: poison particle
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(pokemon)}{pokemon.displayName} was hurt by poison!"));
                yield return StartCoroutine(AnimationChangeHP(pokemon, resultingHP));
            }
            // TODO: Toxic
            // Burn
            if (pokemon.basePartyPokemon.status == Status.Burnt && pokemon.basePartyPokemon.UsableInBattle())
            {
                float damage = ((float)pokemon.basePartyPokemon.GetStatTuple(1).actual/8f);
                int resultingHP = Mathf.Max(pokemon.basePartyPokemon.GetCurrentHP()-(int)damage, 0);
                //TODO: burn particle
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(pokemon)}{pokemon.displayName} was damaged by a burn!"));
                yield return StartCoroutine(AnimationChangeHP(pokemon, resultingHP));
            }
            // TODO: Nightmare
            // TODO: Curse
            // TODO: Multi-turn attacks
            // TODO: HP loss from opposing Pok??mon???s Bad Dreams
            // TODO: Outrage and the like
            // TODO: Uproar
            // TODO: Disable
            // TODO: Encore
            // TODO: Taunt
            // TODO: Magnet Rise
            // TODO: Heal Block
            // TODO: Embargo
            // TODO: Yawn
            // TODO: Sticky Barb
            // TODO: Future Sight/Doom Desire (resolved in targets' Turn order)
            // TODO: Perish Song (resolved in turn order)
            // TODO: Ideally after fainted pokemon switch: Effects from abilities, form changes, items (see above). For Pok??mon that have Slow Start and just entered the battle from fainted ones, the Slow Start effect begins (in turn order).
            if (!pokemon.basePartyPokemon.UsableInBattle()) yield return StartCoroutine(PerformFaint(pokemon));
        }

        //clear fainted pokemon from the field
        //distribute exp to player
        foreach (BattlePokemon pokemon in fieldPokemon)
        {
            if (!pokemon.basePartyPokemon.UsableInBattle())
            {
                //exp gain sequence for each fainted opponent to player
                if (allyPokemon.basePartyPokemon.UsableInBattle() && pokemon.team == Team.Foe)
                {
                    BattleCalculator.ApplyEVs(allyPokemon.basePartyPokemon, pokemon.basePartyPokemon);
                    yield return StartCoroutine(AnimationGainExp(
                        allyPokemon,
                        BattleCalculator.CalculateExpGain(allyPokemon.basePartyPokemon, pokemon.basePartyPokemon))
                    );
                }
                CleanUpPokemon(pokemon);
            }
        }

        if (player.GetNumberUsablePokemon() == 0)
        {
            yield return StartCoroutine(Typewriter.Instance.WriteText($"You are out of usable Pokemon!"));
            state = BattleState.End;
            yield return EndBattle(EndType.Loss);
        }
    }

    IEnumerator SwitchOutPhase()
    {
        if (!foePokemon.basePartyPokemon.UsableInBattle())
        {
            int nextPokemonIndex = BattleCalculator.AITrainerGetIndexNextPokemon();
            //if -1 is returned, all of the pokemon have fainted
            if (nextPokemonIndex == -1)
            {
                state = BattleState.End;
                yield return StartCoroutine(EndBattle(EndType.Win));
            }
            yield return StartCoroutine(Typewriter.Instance.WriteText($"{foe} is about to send out {foe.party[nextPokemonIndex].GetName()}."));

            if (allyPokemon.basePartyPokemon.UsableInBattle())
            {
                if (player.GetNumberUsablePokemon() > 1)
                {
                    yield return StartCoroutine(Typewriter.Instance.WriteText($"Would you like to switch out {allyPokemon.displayName}?"));
                    Typewriter.Instance.ShowOptions("Switch out Pokemon", "Keep Going");
                    PartyMenu.Instance.currentSwapType = SwapType.BattleActionSwap;
                    UISwitchPerformed = false;
                    yield return new WaitUntil(() => Typewriter.Instance.GetSelectedDialog() != null);
                    Typewriter.Instance.HideOptions();
                    if (Typewriter.Instance.GetSelectedDialog() == DialogOption.Yes)
                    {
                        ShowPartyMenuForSwitchOut();
                        yield return new WaitUntil(() => UISwitchPerformed);
                    }
                    UISwitchPerformed = true;
                }
            }
            else
            {
                PartyMenu.Instance.currentSwapType = SwapType.OnFaintedAlly;
                UISwitchPerformed = false;
                ShowPartyMenuForSwitchOut();
                yield return new WaitUntil(() => UISwitchPerformed);
                UISwitchPerformed = true;
            }
            yield return StartCoroutine(SummonPokemon(foe, nextPokemonIndex));
        }
        else //foe is alive
        {
            if (!allyPokemon.basePartyPokemon.UsableInBattle())
            {
                PartyMenu.Instance.currentSwapType = SwapType.OnFaintedAlly;
                UISwitchPerformed = false;
                ShowPartyMenuForSwitchOut();
                yield return new WaitUntil(() => UISwitchPerformed);
                UISwitchPerformed = true;
            }
        }
    }
    IEnumerator PerformFaint(BattlePokemon victim)
    {
        yield return StartCoroutine(AnimationFaint(victim));
        if (victim.team == Team.Ally) yield return StartCoroutine(Typewriter.Instance.WriteText($"Oh no! {allyPokemon.displayName} fainted!"));
        else yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(foePokemon)}{foePokemon.displayName} has fainted!"));
    }
    IEnumerator EndBattle(EndType endType)
    {
        if (state != BattleState.End) yield break;
        switch (endType)
        {
            case EndType.Win:
                if (battleType == BattleType.Trainer)
                {
                    int moneyGain = foe.MoneyAfterBattleLoss();
                    yield return StartCoroutine(Typewriter.Instance.WriteText($"{foe} has been defeated!"));
                    //TODO: slide in trainer
                    yield return StartCoroutine(Typewriter.Instance.WriteText(foe.dialogLoseBattle));
                    yield return StartCoroutine(Typewriter.Instance.WriteText($"{player} got ${moneyGain} for winning."));
                }
                break;
            case EndType.Loss:
                int moneyLoss = player.MoneyAfterBattleLoss();
                GameManager.Instance.player.UpdateMoney(-player.MoneyAfterBattleLoss());
                yield return StartCoroutine(Typewriter.Instance.WriteText($"You lost ${moneyLoss} for losing the battle."));
                //TODO: scurry to pokemon center text
                break;
            case EndType.Flee:
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{player} fled."));
                break;
        }
        //TODO: fade out scene
        GameManager.Instance.CleanUpBattle();
        SceneManager.LoadScene("MainMenu");
    }

    /*
    .##.....##.########.##.......########..########.########...######.
    .##.....##.##.......##.......##.....##.##.......##.....##.##....##
    .##.....##.##.......##.......##.....##.##.......##.....##.##......
    .#########.######...##.......########..######...########...######.
    .##.....##.##.......##.......##........##.......##...##.........##
    .##.....##.##.......##.......##........##.......##....##..##....##
    .##.....##.########.########.##........########.##.....##..######.
    */
    public BattlePokemon GetPokemonAtSlot(Team targetTeam, int targetIndex=0)
    {
        //for single battles only
        if (targetTeam == Team.Foe)
        {
            return foePokemon;
        }
        else
        {
            return allyPokemon;
        }
    }
    public List<BattlePokemon> GetAllAlivePokemon()
    {
        // TODO: If more than one Pok??mon is switched in from fainted ones at the same time, new ones are switched in in the order of leader-first, opposing-first, leader-second, opposing-second. Each Pok??mon switched in this way enters the battle before the next Pok??mon is chosen.
        List<BattlePokemon> activePokemon = new List<BattlePokemon>();
        if (allyPokemon.basePartyPokemon.UsableInBattle())
        {
            activePokemon.Add(allyPokemon);
        }
        if (foePokemon.basePartyPokemon.UsableInBattle())
        {
            activePokemon.Add(foePokemon);
        }
        return activePokemon;
    }
    public List<BattlePokemon> GetAllFieldPokemon()
    {
        List<BattlePokemon> pokemon = new List<BattlePokemon>();
        pokemon.Add(allyPokemon);
        pokemon.Add(foePokemon);
        return pokemon;
    }
    public string GetPrefix(BattlePokemon pokemon)
    {
        if (pokemon.team == Team.Foe) return (battleType == BattleType.Wild) ? "The wild " : "The foe ";
        else return "";
    }
    private void SetButtonsClickable(bool clickable)
    {
        foreach (Button button in moveButtons)
        {
            button.interactable = clickable;
        }
        partyButton.interactable = clickable;
        itemsButton.interactable = clickable;
        fleeButton.interactable = clickable;
    }

    private void SetButtonVisable(bool visable)
    {
        foreach (Button button in moveButtons)
        {
            button.gameObject.SetActive(visable);
        }
        partyButton.gameObject.SetActive(visable);
        itemsButton.gameObject.SetActive(visable);
        fleeButton.gameObject.SetActive(visable);
    }

    IEnumerator SummonTrainer(Trainer trainer)
    {
        if (trainer.team == Team.Foe)
        {
            
        }
        else
        {
            
        }
        yield return null;
    }

    IEnumerator RecallTrainer(Trainer trainer)
    {
        if (trainer.team == Team.Foe)
        {
            
        }
        else
        {
            
        }
        yield return null;
    }

    //bring out a pokemon from a trainer's party based on the given party index
    IEnumerator SummonPokemon(Trainer trainer, int partyIndex, int fieldIndex=0)
    {
        if (trainer.team == Team.Foe)
        {
            CreatePokemon(foePlacement, trainer, partyIndex, fieldIndex);
            if (battleType == BattleType.Trainer)
            {
                yield return StartCoroutine(AnimationGrowPokemon(foePokemon.gameObject));
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GameManager.Instance.foeTrainer} sent out {foePokemon.displayName}!"));
            }
            else yield return StartCoroutine(Typewriter.Instance.WriteText($"A wild {foePokemon.displayName} appeared!"));
            CreateHUD3D(foePokemon);
            Debug.Log(foePokemon.basePartyPokemon.FullString());
        }
        else
        {
            CreatePokemon(allyPlacement, trainer, partyIndex, fieldIndex);
            yield return StartCoroutine(AnimationGrowPokemon(allyPokemon.gameObject));
            yield return StartCoroutine(Typewriter.Instance.WriteText($"Go, {allyPokemon.displayName}!"));
            CreateHUD3D(allyPokemon);
            Debug.Log(allyPokemon.basePartyPokemon.FullString());
        }
    }

    //fill a sprite prefab based on a pokemon
    private void CreatePokemon(Transform location, Trainer trainer, int partyIndex, int fieldIndex=0)
    {
        GameObject createdPokemon = Instantiate(spritePlane, location); 
        BattlePokemon battlePokemon = createdPokemon.GetComponent<BattlePokemon>();

        battlePokemon.SetPokemon(trainer.party[partyIndex], trainer.team, partyIndex);
        if (trainer.team == Team.Foe)
        {
            createdPokemon.GetComponentInChildren<SpriteRenderer>().sprite = SpriteLookup.Instance.GetPokemonSprite(SpriteSide.Front, battlePokemon.basePartyPokemon);
            foePokemon = battlePokemon;
        }
        else
        {
            createdPokemon.GetComponentInChildren<SpriteRenderer>().sprite = SpriteLookup.Instance.GetPokemonSprite(SpriteSide.Back, battlePokemon.basePartyPokemon);
            allyPokemon = battlePokemon;
        }
        
        float onGroundOffset = ((createdPokemon.GetComponentInChildren<SpriteRenderer>().sprite.rect.height/2)-(2+DataUtility.GetPadding(createdPokemon.GetComponentInChildren<SpriteRenderer>().sprite).y))/createdPokemon.GetComponentInChildren<SpriteRenderer>().sprite.pixelsPerUnit*2;
        createdPokemon.transform.position = new Vector3(location.position.x, location.position.y+onGroundOffset, location.position.z);
        createdPokemon.GetComponentInChildren<SpriteRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    private void CreateHUD3D(BattlePokemon pokemon)
    {
        if (pokemon.team == Team.Foe)
        {
            GameObject hud3d = Instantiate(HUD3D, foeHUD3DPlacement);
            foeHUD3D = hud3d.GetComponent<Battle3DHUD>();
            foeHUD3D.SetHUD(pokemon);
            pokemon.SetAccompanyingHUD(foeHUD3D);
        }
        else
        {
            GameObject hud3d = Instantiate(HUD3D, allyHUD3DPlacement);
            allyHUD3D = hud3d.GetComponent<Battle3DHUD>();
            allyHUD3D.SetHUD(pokemon);
            pokemon.SetAccompanyingHUD(allyHUD3D);
        }
    }

    //when the pokemon is recalled from the field from an action party switch or move
    IEnumerator RecallPokemon(BattlePokemon pokemon)
    {
        if (pokemon == null)
        {
            Debug.LogWarning("Attempt to recall pokemon failed; pokemon does not exist");
            yield break;
        }
        SpriteRenderer sprite = pokemon.GetComponentInChildren<SpriteRenderer>();
        float counter = 0f;
        float duration = 0.15f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            pokemon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, counter / duration);
            yield return null;
        }
        CleanUpPokemon(pokemon);
    }

    //when the pokemon is removed from the field via switch or faint. called at the end of that function
    private void CleanUpPokemon(BattlePokemon pokemon)
    {
        Debug.Log($"Removing {pokemon.displayName} from field");
        Destroy(pokemon.GetAccompanyingHUD().gameObject);
        Destroy(pokemon.gameObject);
    }
    public void ShowPartyMenuForSwitchOut()
    {
        Debug.Log("showing menu on foe faint");
        PartyMenu.Instance.currentSwapType = SwapType.OnFaintedFoe;
        PartyMenu.Instance.ShowMenu();
    }
    public int GetActivePartyIndex(Team team)
    {
        if (team == Team.Ally) return allyPokemon.partyIndex;
        else return foePokemon.partyIndex; 
    }
    public IEnumerator CancelSwitch()
    {
        UISwitchPerformed = true;
        yield return null;
    }
    IEnumerator AwaitSelections()
    {
        yield return new WaitUntil(() => (foeSelectedAction != null && allySelectedAction != null));
        Debug.Log($"Ally: {allySelectedAction}");
        Debug.Log($"Foe: {foeSelectedAction}");
    }

    /*
    ....###....##....##.####.##.....##....###....########.####..#######..##....##..######.
    ...##.##...###...##..##..###...###...##.##......##.....##..##.....##.###...##.##....##
    ..##...##..####..##..##..####.####..##...##.....##.....##..##.....##.####..##.##......
    .##.....##.##.##.##..##..##.###.##.##.....##....##.....##..##.....##.##.##.##..######.
    .#########.##..####..##..##.....##.#########....##.....##..##.....##.##..####.......##
    .##.....##.##...###..##..##.....##.##.....##....##.....##..##.....##.##...###.##....##
    .##.....##.##....##.####.##.....##.##.....##....##....####..#######..##....##..######.
    */

    public IEnumerator SetStatus(BattlePokemon victim, Status status)
    {
        victim.basePartyPokemon.status = status;
        victim.GetAccompanyingHUD().SetStatus();

        switch (status)
        {
            case Status.None:
                //the pokemon was released from whatever ailment they had, text is elsewhere
                break;
            case Status.Sleep:
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(victim)}{victim.displayName} fell asleep!"));
                //TODO: particle
                break;
            case Status.Poisoned:
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(victim)}{victim.displayName} has been poisoned!"));
                //TODO: particle
                break;                
            case Status.Burnt:
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(victim)}{victim.displayName} has been burned!"));
                //TODO: particle
                break;
            case Status.Paralyzed:
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{GetPrefix(victim)}{victim.displayName} has been paralyzed! Maybe it can't attack!"));
                //TODO: particle
                break;
        }

    }
    IEnumerator AnimationGrowPokemon(GameObject spriteObject)
    {
        SpriteRenderer sprite = spriteObject.GetComponentInChildren<SpriteRenderer>();
        Transform particleSpawn = sprite.transform;
        ParticleSystem particle = Instantiate(pokeballParticle, particleSpawn);
        float counter = 0f;
        float duration = 0.15f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            spriteObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, counter / duration);
            yield return null;
        }
        //TODO: pokemon cry
        yield return new WaitForSeconds(WAIT_TIME);
        Destroy(particle);
    }
    IEnumerator AnimationGenericSpecialMove(Transform source, Transform target, Move move)
    {
        Vector3 direction = target.position - source.position;
        float angle = Vector3.Angle(direction, Vector3.forward);
        if (Vector3.Dot(direction, source.right) < 0) angle = -angle;
        Color particleColor = GameManager.Instance.registry.GetTypeByID(move.typeID).color;
        ParticleSystem attackParticle = Instantiate(genericAttack, source.position, Quaternion.AngleAxis(angle, Vector3.up));
        ParticleSystem.MainModule main = attackParticle.main;
        main.startColor = particleColor;
        yield return new WaitForSeconds(attackParticle.main.duration*2);
        Destroy(attackParticle.gameObject);
        yield return null;
    }

    IEnumerator AnimationGenericSelfStatusMove(GameObject spriteObject)
    {
        SpriteRenderer sprite = spriteObject.GetComponentInChildren<SpriteRenderer>();
        Vector3 orignalScale = spriteObject.transform.localScale;
        float counter = 0f;
        float duration = 0.20f;
        float growth = 1.333f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            spriteObject.transform.localScale = Vector3.Lerp(orignalScale, orignalScale*growth, counter / duration);
            yield return null;
        }
        counter = 0f;
        yield return new WaitForSeconds(0.75f);
        while (counter < duration)
        {
            counter += Time.deltaTime;
            spriteObject.transform.localScale = Vector3.Lerp(orignalScale*growth, orignalScale, counter / duration);
            yield return null;
        }
        yield return new WaitForSeconds(WAIT_TIME);
    }

    IEnumerator AnimationFaint(BattlePokemon faintedPokemon)
    {
        //TODO: pokemon cry
        yield return new WaitForSeconds(WAIT_TIME);
        
        SpriteRenderer sprite = faintedPokemon.GetComponentInChildren<SpriteRenderer>();
        Vector3 startPos = faintedPokemon.transform.position;
        Vector3 endPos = new Vector3(faintedPokemon.transform.position.x, faintedPokemon.transform.position.y-3, faintedPokemon.transform.position.z);
        float counter = 0f;
        float duration = 0.4f;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            faintedPokemon.transform.position = Vector3.Lerp(startPos, endPos, counter / duration);
            yield return null;
        }
    }
    IEnumerator AnimationTakeDamage(BattlePokemon victim)
    {
        SpriteRenderer s = victim.gameObject.GetComponentInChildren<SpriteRenderer>();
        int numTotalBlinks = 6;
        int numCompletedBlinks = 0;
        float blinkStepDuration = 0.1f;
        while (numCompletedBlinks <= numTotalBlinks)
        {
            s.enabled = (numCompletedBlinks % 2 == 0);
            yield return new WaitForSeconds(blinkStepDuration);
            numCompletedBlinks++;
        }
        s.enabled = true;
    }

    IEnumerator AnimationStatChange(Transform source, int netStageChange)
    {
        if (netStageChange > 0)
        {
            Vector3 spawn = new Vector3 (source.position.x, 0, source.position.z);
            ParticleSystem attackParticle = Instantiate(statBuffParticle, spawn, Quaternion.identity);
            yield return new WaitForSeconds(3f);
            Destroy(attackParticle.gameObject);
        }
        else
        {
            Vector3 spawn = new Vector3(source.position.x, 2, source.position.z);
            ParticleSystem attackParticle = Instantiate(statReduceParticle, spawn, Quaternion.identity);
            yield return new WaitForSeconds(3f);
            Destroy(attackParticle.gameObject);
        }
    }

    IEnumerator AnimationChangeHP(BattlePokemon pokemon, int endHP, float speedMultiplier=1f)
    {
        int startHP = pokemon.basePartyPokemon.GetCurrentHP();
        int hp = startHP;
        float counter = 0f;
        float duration = Mathf.Abs(startHP-endHP)/32f/speedMultiplier;
        
        while (hp != endHP)
        {
            counter += Time.deltaTime;
            hp = (int)Mathf.Lerp(startHP, endHP, counter/duration);
            pokemon.basePartyPokemon.SetHP(hp);
            pokemon.GetAccompanyingHUD().SetHP();
            yield return null;
        }
    }

    IEnumerator AnimationGainExp(BattlePokemon playerPokemon, int gain)
    {
        Debug.Log(playerPokemon.basePartyPokemon.GetCurrentExperienceString());
        yield return StartCoroutine(Typewriter.Instance.WriteText($"{playerPokemon.displayName} gained {gain} EXP. points!"));        
        int unrecognizedExp = gain;
        do
        {
            int startExp = playerPokemon.basePartyPokemon.GetCurrentExperience();
            int exp = playerPokemon.basePartyPokemon.GetCurrentExperience();
            int endExp = (int)Mathf.Min(unrecognizedExp+exp, playerPokemon.basePartyPokemon.GetExperienceOfNextLevel());
            int leftover = Mathf.Max(unrecognizedExp+exp - playerPokemon.basePartyPokemon.GetExperienceOfNextLevel(), 0);
            float speed = (float)unrecognizedExp/(float)(playerPokemon.basePartyPokemon.GetExperienceOfNextLevel() - playerPokemon.basePartyPokemon.GetExperienceOfCurrentLevel())*2f;
            float counter = 0f;

            Debug.Log($"EXP gains: start:{startExp}, curr:{exp}, end:{endExp}, leftover:{leftover}, unrecognizedExp:{unrecognizedExp}");
            while (exp < endExp)
            {
                counter += Time.deltaTime;
                exp = (int)Mathf.Lerp(startExp, endExp, counter/speed);
                playerPokemon.basePartyPokemon.SetExperience(exp);
                playerPokemon.GetAccompanyingHUD().SetExp();
                yield return null;
            }
            playerPokemon.basePartyPokemon.SetExperience(exp);
            unrecognizedExp = leftover;
            if (leftover > 0)
            {
                Debug.Log($"Before level up: " + playerPokemon.basePartyPokemon.StatsListString());
                playerPokemon.basePartyPokemon.LevelUp();
                Debug.Log($"After level up: " + playerPokemon.basePartyPokemon.StatsListString());
                playerPokemon.GetAccompanyingHUD().UpdateLevel();
                yield return StartCoroutine(Typewriter.Instance.WriteText($"{playerPokemon.displayName} has reached level {playerPokemon.basePartyPokemon.GetLevel()}!"));
                List<Move> learnedMoves = playerPokemon.basePartyPokemon.GetMovesOnLevelUp();
                if (learnedMoves.Count > 0)
                {
                    foreach (Move move in learnedMoves)
                    {
                        yield return StartCoroutine(Typewriter.Instance.NewMoveDialog(playerPokemon.basePartyPokemon, move));
                    }
                }
            }
        } while (unrecognizedExp > 0);
        playerPokemon.basePartyPokemon.GetCurrentExperienceString();
        yield return new WaitForSeconds(1f);
    }

    /*
    .########.########.########.########..######..########..######.
    .##.......##.......##.......##.......##....##....##....##....##
    .##.......##.......##.......##.......##..........##....##......
    .######...######...######...######...##..........##.....######.
    .##.......##.......##.......##.......##..........##..........##
    .##.......##.......##.......##.......##....##....##....##....##
    .########.##.......##.......########..######.....##.....######.
    */

    /*
    select id as moveID, damage_class_id as class, identifier as id, effect_id as effect, effect_chance as eff_chance, short_effect as short,
		effect, power, pp, accuracy as acc, min_hits as minH, max_hits as maxH, min_turns as minT, max_turns as maxT, drain, healing as heal,
		crit_rate as crit, ailment_chance as ail_c, flinch_chance as fli_c, stat_chance as stat_c
    from moves m, move_meta mm, move_effect_prose me
    where m.id=mm.move_id and m.effect_id=me.move_effect_id and local_language_id=9
    order by move_effect_id, id
    */

    private bool IsEffectSuccessful(BattleAction action)
    {
        bool isDamagingAttack = action.move.damageClass == DamageClass.Physical || action.move.damageClass == DamageClass.Special;
        Move move = action.move;
        BattlePokemon target = GetPokemonAtSlot(action.targetTeam);
        BattlePokemon attacker = action.attackingPokemon;
        switch (move.effect.id)
        {
            case 1: return true;
            case 2: return (target.basePartyPokemon.status != Status.Sleep);
            case 3: return (target.basePartyPokemon.status != Status.Poisoned);
            case 4: return true;
            case 5: return (target.basePartyPokemon.status != Status.Burnt);
            case 6: return (target.basePartyPokemon.status != Status.Frozen);
            case 7: return (target.basePartyPokemon.status != Status.Paralyzed);
            case 8: return true;
            case 9: return (target.basePartyPokemon.status == Status.Sleep);
            case 10: return (action.attackingPokemon.lastMoveUsedAgainstThis != null);
            default: return true;
        }
    }

    //the use of this function assumes that the effect is successful.
    //even if a stage is at maximum, stage changes will still occur, but when they are updated in a different function, nothing will happen
    IEnumerator ApplyMoveEffect(BattleAction action, int damageDealt)
    {
        bool willOccur = (UnityEngine.Random.Range(0f, 1f) < BattleCalculator.CalculateEffectChance(action.move) &&
                            GetPokemonAtSlot(action.targetTeam).basePartyPokemon.GetCurrentHP() > 0);
        if (!willOccur)
        {
            Debug.Log("Effect chance failed. No effect will happen");
            yield break;
        }
        Move move = action.move;
        BattlePokemon attacker = action.attackingPokemon;
        BattlePokemon target = GetPokemonAtSlot(action.targetTeam);
        
        switch (move.effect.id)
        {
            case 1:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}.");
                break;
            case 2:
                Debug.Log("Puts the target to [sleep]{mechanic:sleep}.");
                yield return StartCoroutine(SetStatus(target, Status.Sleep));
                target.remainingSleepTurns = UnityEngine.Random.Range(2,5);
                break;
            case 3:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}. Has a $effect_chance% chance to [poison]{mechanic:poison} the target.");
                yield return StartCoroutine(SetStatus(target, Status.Poisoned));
                break;
            case 4:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}.  [Drains]{mechanic:drain} half the damage inflicted to heal the user.");
                //handled in drain part of perform action move function
                break;
            case 5:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}.  Has a $effect_chance% chance to [burn]{mechanic:burn} the target.");
                yield return StartCoroutine(SetStatus(target, Status.Burnt));
                break;
            case 6:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}.  Has a $effect_chance% chance to [freeze]{mechanic:freeze} the target.");
                yield return StartCoroutine(SetStatus(target, Status.Frozen));
                break;
            case 7:
                Debug.Log("Inflicts [regular damage]{mechanic:regular-damage}.  Has a $effect_chance% chance to [paralyze]{mechanic:paralyze} the target.");
                yield return StartCoroutine(SetStatus(target, Status.Paralyzed));
                break;
            case 8:
                Debug.Log("User [faint]{mechanic:faint}s, even if the attack [fail]{mechanic:fail}s or [miss]{mechanic:miss}es.  Inflicts [regular damage]{mechanic:regular-damage}.");
                //handled in perform action move function
                break;
            case 9:
                Debug.Log("User [faint]{mechanic:faint}s, even if the attack [fail]{mechanic:fail}s or [miss]{mechanic:miss}es.  Inflicts [regular damage]{mechanic:regular-damage}.");
                int newHP = Mathf.Max(attacker.basePartyPokemon.GetStatTuple(1).actual, (damageDealt/2)+attacker.basePartyPokemon.GetCurrentHP());
                yield return StartCoroutine(AnimationChangeHP(attacker, newHP));
                break;
            default:
                Debug.LogWarning($"The effect for {move.name} has not been implemented!");
                break;
        }
        yield return null;
    }
}
