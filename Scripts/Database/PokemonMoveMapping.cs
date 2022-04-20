using System.Collections;
using System.Collections.Generic;

public enum MoveLearnMethod
{
    LevelUp,
    Egg,
    Tutor,
    Machine,
    LightBallEgg,
    FormChange,
    Other
}
public class PokemonMoveMapping
{
    public readonly int pokemonID;
    public readonly int versionGroup;
    public readonly int moveID;
    public readonly MoveLearnMethod method;
    public readonly int level;
    public readonly int order;

    public PokemonMoveMapping(int pokemonID, int versionGroup, int moveID, int methodID, int level, int order)
    {
        this.pokemonID = pokemonID;
        this.versionGroup = versionGroup;
        this.moveID = moveID;
        switch (methodID)
        {
            case 1:
                this.method = MoveLearnMethod.LevelUp;
                break;
            case 2:
                this.method = MoveLearnMethod.Egg;
                break;
            case 3:
                this.method = MoveLearnMethod.Tutor;
                break;
            case 4:
                this.method = MoveLearnMethod.Machine;
                break;
            case 6:
                this.method = MoveLearnMethod.LightBallEgg;
                break;
            case 10:
                this.method = MoveLearnMethod.FormChange;
                break;
            default:
                this.method = MoveLearnMethod.Other;
                break;
        }
        this.level = level;
        this.order = order;
    }

    public override string ToString()
    {
        return $"{this.pokemonID}({this.versionGroup}) -> {this.moveID}";
    }
}
