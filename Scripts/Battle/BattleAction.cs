using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Fight,
    Bag,
    Party,
    Run
}
// public class BattleAction
// {
//     public readonly ActionType action;
//     public readonly Team attackingTeam;
//     public readonly Team targetTeam;
//     public readonly Move move;
//     public readonly int moveSlot;
//     public readonly int priority;
//     public readonly int swapWithPartyIndex;

//     public BattleAction(ActionType action, Team attackingTeam, Team targetTeam, Move move=null, int moveSlot=0, int partyIndex=0)
//     {
//         if (action == ActionType.Bag) this.priority = 10;
//         if (action == ActionType.Party) this.priority = 9;
//         if (action == ActionType.Fight) this.priority = move.priority;
//         this.action = action;
//         this.attackingTeam = attackingTeam;
//         this.targetTeam = targetTeam;
//         this.move = move;
//         this.moveSlot = moveSlot;
//         this.swapWithPartyIndex = partyIndex;
//     }

//     public override string ToString()
//     {
//         return $"{this.action.ToString()} from {this.attackingTeam.ToString()} to {this.targetTeam.ToString()}. Move:{move}, MoveSlot:{moveSlot}, pi:{swapWithPartyIndex}";
//     }
// }

public class BattleAction
{
    public readonly ActionType action;
    public readonly BattlePokemon attackingPokemon;
    public readonly Team targetTeam;
    public readonly int targetIndex;
    public readonly Move move;
    public readonly int moveSlot;
    public readonly int priority;
    public readonly int swapWithPartyIndex;

    public BattleAction(ActionType action, BattlePokemon attackingPokemon, Team targetTeam, int targetIndex, Move move=null, int moveSlot=0, int partyIndex=0)
    {
        if (action == ActionType.Run) this.priority = 11;
        if (action == ActionType.Bag) this.priority = 10;
        if (action == ActionType.Party) this.priority = 9;
        if (action == ActionType.Fight) this.priority = move.priority;
        this.action = action;
        this.attackingPokemon = attackingPokemon;
        this.targetIndex = targetIndex;
        this.targetTeam = targetTeam;
        this.move = move;
        this.moveSlot = moveSlot;
        this.swapWithPartyIndex = partyIndex;
    }

    public override string ToString()
    {
        return $"{this.action.ToString()} from {this.attackingPokemon} to {this.targetTeam.ToString()}:{this.targetIndex}. Move:{move}, MoveSlot:{moveSlot}, pi:{swapWithPartyIndex}";
    }
}
