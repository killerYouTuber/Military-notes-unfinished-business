using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static bool IsPaused { get; set; }
    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;
    public static event System.Action OnResumeGame;
    public static event System.Action OnPauseGame;

    void Start()
    {
        // Проверяем, нужно ли начать игру на паузе (после загрузки через кнопку 'Продолжить')
        // if (GameManager.ShouldStartPaused)
        // {
        //     Pause();
        //     GameManager.ShouldStartPaused = false; // Сбрасываем флаг после использования
        // }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        OnResumeGame?.Invoke();
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        IsPaused = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnPauseGame?.Invoke();
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        if (settingsMenuUI != null) settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenuUI != null) settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void LoadMenu()
    {
        Debug.Log("[PauseManager] Сохранение игры перед выходом в меню...");

        // Просто вызываем метод сохранения из GameManager. Он сам соберет все данные.
        GameManager.Instance.SaveGame(SceneManager.GetActiveScene().name);

        // Сбрасываем состояние паузы и загружаем меню
        Time.timeScale = 1f;
        IsPaused = false; // Убедимся, что сбросили состояние паузы
        GameManager.Instance.LoadMenuScene();
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }

    // Добавляем метод для включения меню паузы при загрузке
    public void ShowPauseMenu()
    {
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            IsPaused = true;
            Debug.Log("[PauseManager] Меню паузы активировано");
        }
        else
        {
            Debug.LogWarning("[PauseManager] Объект меню паузы не найден");
        }
    }
} 