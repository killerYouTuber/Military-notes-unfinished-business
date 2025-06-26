using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public Button continueButton;

    void Start()
    {
        if (GameManager.Instance.HasSave())
            continueButton.interactable = true;
        else
            continueButton.interactable = false;
    }

    public void OnContinueButton()
    {
        GameManager.Instance.LoadGame();
    }

    public void OnNewGameButton()
    {
        Debug.Log("[MainMenuUI] Кнопка 'Начать' нажата. Вызываю GameManager.StartNewGame().");
        GameManager.Instance.StartNewGame();
    }

    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }
} 