using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleInteractable : Interactable
{
    [Header("Настройки дыры")]
    public float interactionDelay = 0.5f;
    public GameObject repairEffect;

    void Start()
    {
        // При загрузке сцены применяем сохраненное состояние
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(InitializeHoleState());
        }
    }

    private IEnumerator InitializeHoleState()
    {
        // Ждем один кадр, чтобы GameManager успел загрузить данные
        yield return null; 

        if (GameManager.CurrentSaveData != null)
        {
            InteractiveObjectState? savedState = GameManager.CurrentSaveData.interactiveObjectStates.Find(
                s => s.uniqueID == uniqueID
            );

            if (savedState.HasValue)
            {
                ApplySavedState(savedState.Value.stateData);
            }
        }
    }

    public override void Interact()
    {
        StartCoroutine(RepairHole());
    }

    private IEnumerator RepairHole()
    {
        // Воспроизведение эффекта ремонта
        if (repairEffect != null)
        {
            Instantiate(repairEffect, transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(interactionDelay);

        // Сохраняем информацию о том, что эта дыра была отремонтирована
        if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
        {
            GameManager.Instance.UpdateInteractiveObjectState(uniqueID, "repaired");
            // Убираем автоматическое сохранение - это перезаписывало основной сейв
            // GameManager.Instance.SaveGame(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // Уничтожаем объект дыры
        Destroy(gameObject);
    }

    // Переопределяем ApplySavedState для обработки состояния дыры
    public override void ApplySavedState(string stateData)
    {
        if (stateData == "repaired")
        {
            // Если дыра была отремонтирована, уничтожаем ее
            Debug.Log($"[HoleInteractable] Applying saved state: Destroying {uniqueID} as it was repaired.");
            Destroy(gameObject);
        }
    }
}