using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    public PauseManager pauseManager;

    public void OnResumeButton()
    {
        pauseManager.Resume();
    }

    public void OnSettingsButton()
    {
        pauseManager.OpenSettings();
    }

    public void OnMenuButton()
    {
        pauseManager.LoadMenu();
    }

    public void OnQuitButton()
    {
        pauseManager.QuitGame();
    }
} 