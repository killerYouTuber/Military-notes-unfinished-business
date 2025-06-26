// MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            bool hasSave = GameManager.Instance.HasSave();
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = hasSave;
        }
        else
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = false; // Деактивируем на всякий случай, если GameManager не найден
        }
    }

    public void StartGame()
    {
        Debug.Log("[MainMenuController] Нажата кнопка 'Новая игра'. Вызываю GameManager.StartNewGame().");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame(); // Вызываем StartNewGame из GameManager
        }
        else
        {
            Debug.LogError("[MainMenuController] GameManager.Instance не найден. Не могу начать новую игру.");
        }
    }

    public void ContinueGame()
    {
        Debug.Log("[MainMenuController] Нажата кнопка 'Продолжить'.");
        if (GameManager.Instance != null && GameManager.Instance.HasSave())
        {
            Debug.Log("[MainMenuController] Загружаю игру...");
            GameManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("[MainMenuController] Невозможно продолжить: нет сохранения или GameManager не найден.");
        }
    }

    public void ExitGame()
    {
        Debug.Log("[MainMenuController] Нажата кнопка 'Выйти из игры'.");
        GameManager.Instance.QuitGame();
    }
}
