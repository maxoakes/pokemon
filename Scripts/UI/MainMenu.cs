using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("Main Menu awake");
        GameManager.Instance.gameMode = Mode.MainMenu;
    }
    public void OnContinueButton()
    {
        Debug.Log("Not implemented");
    }
    public void OnNewGameButton()
    {
        Debug.Log("Not implemented");
    }
    public void OnBattleSimButton()
    {
        Debug.Log("BattleSim");
        SceneManager.LoadScene("BattleSimPartyPicker");
    }
    public void OnOptionsButton()
    {
        Debug.Log("Not implemented");
    }
    public void OnQuitButton()
    {
        Application.Quit();
        Debug.Log("Quit button pressed");
    }
}
