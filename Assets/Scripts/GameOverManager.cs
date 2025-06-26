using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;

    private void Start()
    {
        // Если панель не назначена в инспекторе, ищем ее на сцене
        if (gameOverPanel == null)
        {
            // Предполагаем, что панель лежит на главном Canvas в сцене
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // Ищем дочерний объект с именем "GameOverPanel"
                Transform panelTransform = canvas.transform.Find("GameOverPanel"); 
                if (panelTransform != null)
                {
                    gameOverPanel = panelTransform.gameObject;
                    Debug.Log("Панель 'GameOverPanel' была найдена автоматически.");
                }
            }
        }

        if (gameOverPanel == null)
        {
            Debug.LogError("GameOverManager: Не удалось найти панель 'GameOverPanel'!");
        }
        else
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f; // Ставим игру на паузу
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("Game Over. Экран показан, игра на паузе.");
        }
    }

    // --- Методы для кнопок ---

    // Продолжить (загружает последнее сохранение: обычное или чекпоинт)
    public void Continue()
    {
        Time.timeScale = 1f;
        
        // Проверяем, есть ли вообще какие-либо сохранения
        if (!GameManager.Instance.HasSave() && !GameManager.Instance.HasCheckpoint())
        {
            Debug.Log("[GameOverManager] Нет сохранений. Начинаем уровень с начала.");
            GameManager.Instance.RestartLevel();
        }
        else
        {
            GameManager.Instance.LoadLastSave();
        }
    }

    // Продолжить с контрольной точки
    public void ContinueFromCheckpoint()
    {
        Time.timeScale = 1f;
        
        // Проверяем, есть ли чекпоинт
        if (!GameManager.Instance.HasCheckpoint())
        {
            Debug.Log("[GameOverManager] Чекпоинт не найден. Начинаем уровень с начала.");
            GameManager.Instance.RestartLevel();
        }
        else
        {
            GameManager.Instance.LoadCheckpoint();
        }
    }

    // Начать с начала
    public void Restart()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RestartLevel();
    }

    // Выйти в главное меню
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.LoadMenuScene();
    }

    // Выйти на рабочий стол
    public void Quit()
    {
        GameManager.Instance.QuitGame();
    }
} 