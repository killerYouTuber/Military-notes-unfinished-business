using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Добавляем для работы с UI
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public Image progressBar;
    public TextMeshProUGUI loadingText;
    public Button continueButton; // Ссылка на кнопку "Продолжить"
    public Slider loadingSlider; // Ссылка на UI слайдер загрузки

    private AsyncOperation _asyncLoad; // Добавляем эту строку

    void Start()
    {
        // Убедимся, что курсор видим и разблокирован во время загрузки
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Убедимся, что кнопка продолжить изначально неактивна
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        // Убедимся, что слайдер загрузки изначально неактивен, если он не должен быть виден сразу
        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(true); // Активируем слайдер сразу, чтобы показывать прогресс
            loadingSlider.value = 0; // Начинаем с нуля
        }

        StartCoroutine(LoadYourAsyncScene());
    }

    IEnumerator LoadYourAsyncScene()
    {
        // Проверяем, есть ли сцена для загрузки
        if (string.IsNullOrEmpty(GameManager.SceneToLoadAfterBriv1))
        {
            Debug.LogError("[LoadingSceneManager] Нет сцены для загрузки после briv1. Возвращаемся в меню.");
            SceneManager.LoadScene(GameManager.Instance.menuSceneName);
            yield break;
        }

        Debug.Log($"[LoadingSceneManager] Начинаю асинхронную загрузку сцены: {GameManager.SceneToLoadAfterBriv1}");
        _asyncLoad = SceneManager.LoadSceneAsync(GameManager.SceneToLoadAfterBriv1);
        _asyncLoad.allowSceneActivation = false;

        // Ждем, пока сцена будет загружена
        while (!_asyncLoad.isDone)
        {
            // Обновляем значение слайдера (прогресс Unity идет до 0.9, затем резко до 1)
            float progress = Mathf.Clamp01(_asyncLoad.progress / 0.9f);
            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }
            
            // Когда загрузка почти завершена (или полностью завершена на стороне Unity)
            if (_asyncLoad.progress >= 0.9f)
            {
                // Активируем кнопку "Продолжить", если она еще не активна
                if (continueButton != null && !continueButton.gameObject.activeSelf)
                {
                    continueButton.gameObject.SetActive(true);
                    Debug.Log("[LoadingSceneManager] Сцена загружена на 90%. Кнопка 'Продолжить' активирована.");
                }

                // Если слайдер дошел до 100% и сцена готова, можем обновить его до конца
                if (loadingSlider != null && loadingSlider.value < 1.0f)
                {
                    loadingSlider.value = 1.0f;
                }
            }
            yield return null;
        }

        Debug.Log($"[LoadingSceneManager] Сцена {GameManager.SceneToLoadAfterBriv1} загружена и активирована.");
    }

    public void OnContinueButtonClicked()
    {
        Debug.Log("[LoadingSceneManager] Кнопка 'Продолжить' нажата. Активация сцены.");
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError("[LoadingSceneManager] AsyncOperation is null. Cannot activate scene.");
        }
    }
} 