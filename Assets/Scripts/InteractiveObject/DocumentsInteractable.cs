using UnityEngine;
using System.Collections; // Добавляем для использования IEnumerator
using System.Linq; // Добавляем для использования LINQ, если потребуется для отладки Find или других операций

public class DocumentsInteractable : Interactable
{
    /* void Start()
    {
        Debug.Log($"[DocumentsInteractable] Start вызван для {uniqueID}. Начинаю инициализацию состояния.");
        // При загрузке сцены применяем сохраненное состояние
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(InitializeDocumentsState());
        }
        else
        {
            Debug.LogWarning($"[DocumentsInteractable] GameManager.Instance не найден для {uniqueID} в Start().");
        }
    }

    private IEnumerator InitializeDocumentsState()
    {
        Debug.Log($"[DocumentsInteractable] Запущена InitializeDocumentsState для {uniqueID}.");
        // Ждём, пока GameManager.CurrentSaveData не станет доступен (но не бесконечно)
        int maxWaitFrames = 60; // например, ждём до 1 секунды (60 кадров)
        int waited = 0;
        while (GameManager.CurrentSaveData == null && waited < maxWaitFrames)
        {
            yield return null;
            waited++;
        }

        if (GameManager.CurrentSaveData == null)
        {
            Debug.LogWarning($"[DocumentsInteractable] GameManager.CurrentSaveData всё ещё равен null после ожидания в InitializeDocumentsState для {uniqueID}.");
            yield break;
        }

        Debug.Log($"[DocumentsInteractable] GameManager.CurrentSaveData НЕ равен null. Состояний интерактивных объектов: {GameManager.CurrentSaveData.interactiveObjectStates?.Count ?? 0}");
        if (GameManager.CurrentSaveData.interactiveObjectStates != null)
        {
            foreach (var state in GameManager.CurrentSaveData.interactiveObjectStates)
            {
                Debug.Log($"[DocumentsInteractable] Сохраненное состояние в CurrentSaveData: ID='{state.uniqueID}', State='{state.stateData}'");
            }

            InteractiveObjectState? savedState = GameManager.CurrentSaveData.interactiveObjectStates.Find(
                s => s.uniqueID == uniqueID
            );

            if (savedState.HasValue)
            {
                Debug.Log($"[DocumentsInteractable] Найдено сохраненное состояние для {uniqueID}: '{savedState.Value.stateData}'. Применяю.");
                ApplySavedState(savedState.Value.stateData);
            }
            else
            {
                Debug.Log($"[DocumentsInteractable] Не найдено сохраненное состояние для {uniqueID}.");
            }
        }
        else
        {
            Debug.LogWarning($"[DocumentsInteractable] interactiveObjectStates равен null в CurrentSaveData для {uniqueID}.");
        }
    } */

    public override void Interact()
    {
        // Проверяем, существует ли MissionManager и вызываем метод сбора документов
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnDocumentsCollected();
        }

        // Сохраняем информацию о том, что этот объект был собран
        if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            GameManager.Instance.UpdateInteractiveObjectState(uniqueID, "collected");
            // GameManager.Instance.SaveGame(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // Уничтожаем объект документов после взаимодействия
        Debug.Log($"[DocumentsInteractable] Объект {uniqueID} должен быть уничтожен.");
        Destroy(gameObject);
    }

    // Переопределяем ApplySavedState для обработки состояния документа
    public override void ApplySavedState(string stateData)
    {
        Debug.Log($"[DocumentsInteractable] Вызван ApplySavedState для {uniqueID} со stateData: '{stateData}'.");
        if (stateData == "collected")
        {
            // Если документ был собран, уничтожаем его
            Debug.Log($"[DocumentsInteractable] Applying saved state: Destroying {uniqueID} as it was collected.");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[DocumentsInteractable] ApplySavedState для {uniqueID}: stateData не 'collected'. Объект не уничтожен.");
        }
    }
} 