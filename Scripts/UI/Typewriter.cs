using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum DialogOption
{
    Yes,
    No
}
public class Typewriter : MonoBehaviour
{
    public static Typewriter Instance;
    public TextMeshProUGUI mainDialog;

    //dialog option menu
    public GameObject options;
    private DialogOption? selectedOption;

    //move menu
    public GameObject moveList;
    private int? selectedMoveSlot;
    private int? chosenMoveSlot;
    private PartyPokemon evaluatingPokemon;
    private Move evaluatingMove;


    //general dialog settings
	public float delayBeforeStart = 0f;
	public float baseCharDelta = 0.025f;
    public float speedUpFactor = 2f;
    private float actualCharDelta;
	public string cursor = "";

	void Start()
	{
		Debug.Log("Typewriting dialogue started");
        DontDestroyOnLoad(this.gameObject);
        if (Typewriter.Instance == null) Instance = this;
        else UnityEngine.Object.Destroy(gameObject);
        HideDialog();
        HideOptions();
        HideMoves();
        StartCoroutine(SpeedUpText());
	}

    public IEnumerator WriteText(string text)
    {
        mainDialog.gameObject.SetActive(true);
        mainDialog.text = cursor;
        yield return new WaitForSeconds(delayBeforeStart);

		foreach (char c in text)
		{
			if (mainDialog.text.Length > 0)
			{
				mainDialog.text = mainDialog.text.Substring(0, mainDialog.text.Length - cursor.Length);
			}
			mainDialog.text += c;
			mainDialog.text += cursor;
			yield return new WaitForSeconds(actualCharDelta);
		}

		if (cursor != "") mainDialog.text = mainDialog.text.Substring(0, mainDialog.text.Length - cursor.Length);
        yield return new WaitUntil(() => Input.anyKeyDown);
	}

    public void SetText(string text)
    {
        mainDialog.gameObject.SetActive(true);
        mainDialog.text = text;
	}

    private IEnumerator SpeedUpText()
    {
        while (true)
        {
            if (Input.anyKey) actualCharDelta = baseCharDelta/speedUpFactor;
            else actualCharDelta = baseCharDelta;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }

    public void SetDialogOption(int d)
    {
        switch (d)
        {
            case 0:
                selectedOption = DialogOption.Yes;
                break;
            case 1:
                selectedOption = DialogOption.No;
                break;
            default:
                selectedOption = null;
                break;
        }
    }
    public void ShowOptions(string yes, string no)
    {
        selectedOption = null;
        options.SetActive(true);
        TextMeshProUGUI positive = options.transform.Find("Yes").GetComponentInChildren<TextMeshProUGUI>();
        positive.text = yes;
        TextMeshProUGUI negative = options.transform.Find("No").GetComponentInChildren<TextMeshProUGUI>();
        negative.text = no;
    }

    public void HideOptions()
    {
        options.SetActive(false);
    }

    public DialogOption? GetSelectedDialog()
    {
        return selectedOption;
    }

    public void HideDialog()
    {
        mainDialog.gameObject.SetActive(false);
    }

    public IEnumerator NewMoveDialog(PartyPokemon pokemon, Move newMove)
    {
        evaluatingPokemon = pokemon;
        evaluatingMove = newMove;
        if (pokemon.GetMoves().Count < 4)
        {
            pokemon.AddMove(newMove);
            yield return StartCoroutine(this.WriteText($"{pokemon.GetName()} learned {newMove.name}!"));
        }
        else
        {
            yield return StartCoroutine(this.WriteText($"{pokemon.GetName()} wants to learn {newMove.name}, but already knows four moves."));
            yield return StartCoroutine(this.WriteText($"Should {pokemon.GetName()} learn {newMove.name}?"));
            ShowOptions("Forget An Old Move", "Do Not Learn New Move");
            yield return new WaitUntil(() => GetSelectedDialog() != null);
            Typewriter.Instance.HideOptions();
            if (GetSelectedDialog() == DialogOption.Yes)
            {
                HideOptions();
                yield return StartCoroutine(this.WriteText($"Which move should be forgotten in order to learn {newMove.name}?"));
                ShowMoveList();
                yield return new WaitUntil(() => chosenMoveSlot != null);

                HideMoves();
                selectedMoveSlot = null;
                evaluatingMove = null;
                evaluatingPokemon = null;

                if (chosenMoveSlot == 4)
                {
                    chosenMoveSlot = null;
                    yield return StartCoroutine(this.WriteText($"Are you sure you want to give up on learning {newMove.name}?"));
                    ShowOptions($"Yes, forget learning {newMove.name}", "No, forget an old move");
                    yield return new WaitUntil(() => GetSelectedDialog() != null);
                    Typewriter.Instance.HideOptions();
                    if (GetSelectedDialog() == DialogOption.No)
                    {
                        //TODO: this works, but opens the door for a stack overflow
                        yield return StartCoroutine(NewMoveDialog(pokemon, newMove));
                    }
                }
                else
                {
                    pokemon.ReplaceMove(newMove, chosenMoveSlot);
                    chosenMoveSlot = null;
                    yield return StartCoroutine(this.WriteText($"1.."));
                    yield return StartCoroutine(this.WriteText($"2.."));
                    yield return StartCoroutine(this.WriteText($"3.."));
                    yield return StartCoroutine(this.WriteText($"..."));
                    yield return StartCoroutine(this.WriteText($"{pokemon.GetName()} learned {newMove.name}!"));
                }
                
            }
        }
    }

    public void HideMoves()
    {
        moveList.SetActive(false);
    }

    public void ShowMoveList()
    {
        moveList.SetActive(true);
        SetMoveDescription(0);
        for (int i = 0; i < 4; i++)
        {
            moveList.transform.Find("m"+i).GetComponentInChildren<TextMeshProUGUI>().text = evaluatingPokemon.GetMoves()[i].m.name;
        }
        moveList.transform.Find("m4").GetComponentInChildren<TextMeshProUGUI>().text = evaluatingMove.name;
    }

    public void SetMoveDescription(int i)
    {
        Move viewingMove = (i == 4) ? evaluatingMove : evaluatingPokemon.GetMoves()[i].m;
        selectedMoveSlot = i;
        moveList.transform.Find("Power").GetComponentInChildren<TextMeshProUGUI>().text = $"Power: {viewingMove.power}";
        moveList.transform.Find("PP").GetComponentInChildren<TextMeshProUGUI>().text = $"PP: {viewingMove.pp}";
        moveList.transform.Find("Accuracy").GetComponentInChildren<TextMeshProUGUI>().text = $"Acc: {viewingMove.accuracy}";
        moveList.transform.Find("Type").GetComponentInChildren<TextMeshProUGUI>().text = $"{GameManager.Instance.registry.GetTypeByID(viewingMove.typeID).name}";
        moveList.transform.Find("Description").GetComponentInChildren<TextMeshProUGUI>().text = $"{viewingMove.description}";        
    }

    public void OnConfirmMoveChange()
    {
        chosenMoveSlot = selectedMoveSlot;
    }
}