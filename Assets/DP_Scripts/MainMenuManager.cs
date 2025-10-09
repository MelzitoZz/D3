using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// Load the main game scene.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    /// <summary>
    /// Load the settings scene.
    /// </summary>
    public void OpenSettings()
    {
        SceneManager.LoadScene("SettingsScene");
    }

    /// <summary>
    /// Close the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}