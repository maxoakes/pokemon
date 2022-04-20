using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonType
{
    public readonly int id;
    public readonly string identifier;
    public readonly int introduced;
    public readonly int damageClassID;
    public readonly string name;
    private Dictionary<int, int> damageFactor;
    public readonly Color color;

    public PokemonType(int id, string identifier, int introduced, int damageClassID, string name)
    {
        this.id = id;
        this.identifier = identifier;
        this.introduced = introduced;
        this.name = name;
        this.damageFactor = new Dictionary<int, int>();
        switch (this.identifier)
        {
            case "normal":
                this.color = new Color(186f/255f, 188f/255f, 174f/255f);
                break;
            case "fighting":
                this.color = new Color(241f/255f, 133f/255f, 110f/255f);
                break;
            case "flying":
                this.color = new Color(82f/255f, 198f/255f, 235f/255f);
                break;
            case "poison":
                this.color = new Color(222f/255f, 148f/255f, 245f/255f);
                break;
            case "ground":
                this.color = new Color(231f/255f, 211f/255f, 116f/255f);
                break;
            case "rock":
                this.color = new Color(198f/255f, 162f/255f, 74f/255f);
                break;
            case "bug":
                this.color = new Color(161f/255f, 204f/255f, 135f/255f);
                break;
            case "ghost":
                this.color = new Color(161f/255f, 106f/255f, 252f/255f);
                break;
            case "steel":
                this.color = new Color(186f/255f, 186f/255f, 201f/255f);
                break;
            case "fire":
                this.color = new Color(244f/255f, 144f/255f, 46f/255f);
                break;
            case "water":
                this.color = new Color(114f/255f, 154f/255f, 252f/255f);
                break;
            case "grass":
                this.color = new Color(145f/255f, 229f/255f, 135f/255f);
                break;
            case "electric":
                this.color = new Color(215f/255f, 222f/255f, 30f/255f);
                break;
            case "psychic":
                this.color = new Color(245f/255f, 68f/255f, 172f/255f);
                break;
            case "ice":
                this.color = new Color(49f/255f, 214f/255f, 207f/255f);
                break;
            case "dragon":
                this.color = new Color(110f/255f, 74f/255f, 200f/255f);
                break;
            case "dark":
                this.color = new Color(143f/255f, 133f/255f, 134f/255f);
                break;
            case "fairy":
                this.color = new Color(226f/255f, 141f/255f, 226f/255f);
                break;
            default:
                this.color = Color.white;
                break;
        }
    }

    public void addTargetDamageFactor(int version, int factor)
    {
        this.damageFactor.Add(version, factor);
    }

    public int GetDamageFactor(int targetTypeID)
    {
        return this.damageFactor[targetTypeID];
    }

    public string FullString()
    {
        return $"{this.name} with id {this.id} and damageClass {this.damageClassID}. Damage multiplier against steel: {this.GetDamageFactor(9)}";
    }

    public override string ToString()
    {
        return this.name;
    }
}
