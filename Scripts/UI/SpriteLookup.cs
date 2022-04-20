using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public enum SpriteSide
{
    Front,
    Back
}

public class SpriteLookup : MonoBehaviour
{
    public static SpriteLookup Instance;
    [SerializeField] private SpriteAtlas back;
    [SerializeField] private SpriteAtlas backFemale;
    [SerializeField] private SpriteAtlas backShinyFemale;
    [SerializeField] private SpriteAtlas backShiny;
    [SerializeField] private SpriteAtlas front;
    [SerializeField] private SpriteAtlas frontShiny;
    [SerializeField] private SpriteAtlas frontShinyFemale;
    [SerializeField] private SpriteAtlas frontFemale;
    [SerializeField] private SpriteAtlas footprints;
    [SerializeField] private SpriteAtlas item;
    [SerializeField] private SpriteAtlas icon;
    [SerializeField] private SpriteAtlas iconFemale;

    void Awake()
    {
        Debug.Log("Sprite Lookup awoken.");
        DontDestroyOnLoad(this.gameObject);
        if (SpriteLookup.Instance == null) Instance = this;
        else UnityEngine.Object.Destroy(gameObject);
    }

    public Sprite GetPokemonSprite(SpriteSide side, PartyPokemon pokemon)
    {
        bool needsFemaleVersion = pokemon.basePokemon.species.hasGenderDifferences && (pokemon.gender == Gender.Female);
        bool shiny = pokemon.isShiny;
        string f = pokemon.GetFormIdentifier();
        string form = (f.Equals("")) ? "" : $"-{f}";
        string spriteID = $"{pokemon.basePokemon.species.id}{form}";

        if (side == SpriteSide.Front)
        {
            if      (needsFemaleVersion && shiny) return frontShinyFemale.GetSprite(spriteID);
            else if (!needsFemaleVersion && shiny) return frontShiny.GetSprite(spriteID);
            else if (needsFemaleVersion && !shiny) return frontFemale.GetSprite(spriteID);
            else if (!needsFemaleVersion && !shiny) return front.GetSprite(spriteID);
        }
        else
        {
            if      (needsFemaleVersion && shiny) return backShinyFemale.GetSprite(spriteID);
            else if (!needsFemaleVersion && shiny) return backShiny.GetSprite(spriteID);
            else if (needsFemaleVersion && !shiny) return backFemale.GetSprite(spriteID);
            else if (!needsFemaleVersion && !shiny) return back.GetSprite(spriteID);
        }
        return front.GetSprite("0");
    }
    public Sprite GetItemSprite(string identifier)
    {
        return item.GetSprite(identifier);
    }

    public Sprite GetIcon(PartyPokemon pokemon)
    {
        bool needsFemaleVersion = pokemon.basePokemon.species.hasGenderDifferences && (pokemon.gender == Gender.Female);
        string f = pokemon.GetFormIdentifier();
        string form = (f.Equals("")) ? "" : $"-{f}";
        string spriteID = $"{pokemon.basePokemon.species.id}{form}";

        Sprite chosenSprite;
        if (needsFemaleVersion)
        {
            chosenSprite = iconFemale.GetSprite(spriteID);
            if (chosenSprite == null) chosenSprite = icon.GetSprite(spriteID);
        }
        else chosenSprite = icon.GetSprite(spriteID);

        return chosenSprite;
    }

    public Sprite GetFootprint(PartyPokemon pokemon)
    {
        return item.GetSprite(pokemon.basePokemon.species.id.ToString());
    }
}
