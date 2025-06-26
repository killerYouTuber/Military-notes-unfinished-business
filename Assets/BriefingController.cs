using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BriefingController : MonoBehaviour
{
    public Slider loadingSlider; // Ссылка на UI слайдер
    public Text loadingText; // Опционально: текст с процентами
    
    private AsyncOperation asyncLoad;
    private bool isLoaded = false;

    void Start()
    {
        // Убедимся, что курсор виден и разблокирован в сцене брифинга
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Начинаем загрузку следующей сцены
        StartCoroutine(LoadLevelAsync());
    }

    public void ContinueToLevel()
    {
        if (isLoaded && asyncLoad != null)
        {
            // Скрываем и блокируем курсор перед переходом в игровую сцену
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            asyncLoad.allowSceneActivation = true;
        }
    }

    IEnumerator LoadLevelAsync()
    {
        asyncLoad = SceneManager.LoadSceneAsync(2); // Индекс сцены Level 1
        asyncLoad.allowSceneActivation = false; // Не переключаемся на сцену автоматически

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Обновляем UI
            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }
            
            if (loadingText != null)
            {
                loadingText.text = $"{progress * 100:F0}%";
            }

            // Когда загрузка достигает 100%
            if (asyncLoad.progress >= 0.9f && !isLoaded)
            {
                isLoaded = true;
                
                
                if (loadingSlider != null)
                {
                    loadingSlider.value = 1.0f;
                }
                
                if (loadingText != null)
                {
                    loadingText.text = "100%";
                }
            }

            yield return null;
        }
    }
} 