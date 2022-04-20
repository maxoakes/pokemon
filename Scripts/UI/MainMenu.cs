using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public int selectedGen = 4;
    public Slider genSlider;

    void Awake()
    {
        Debug.Log("Main Menu awake");
        GameManager.Instance.gameMode = Mode.MainMenu;
    }
    public void UpdateGeneration()
    {
        this.selectedGen = (int)genSlider.value;
        Debug.Log($"Gen set to {this.selectedGen}");
    }
    public void Continue()
    {
        Debug.Log("Not implemented");
    }
    public void NewGame()
    {
        Debug.Log("Not implemented");
    }

    public void BattleSim()
    {
        Debug.Log("BattleSim");
        SceneManager.LoadScene("BattleSimPartyPicker");
    }

    public void QuitApp()
    {
        Application.Quit();
        Debug.Log("Quit button pressed");
    }
}
