using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Nombres de las escenas")]
    [SerializeField] private string platformerScene = "PlatformerLevel";
    [SerializeField] private string combatScene = "TurnBasedCombat";

    [Header("Paneles")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    public void PlayPlatformer()
    {
        SceneManager.LoadScene(platformerScene);
    }

    public void PlayTurnBasedCombat()
    {
        SceneManager.LoadScene(combatScene);
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}