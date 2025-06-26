using UnityEngine;
using System.Collections; // Добавляем для использования IEnumerator

// Изменяем класс, чтобы он наследовался от Interactable
public class MedkitPickup : Interactable
{
    private PlayerHealth playerHealth;
    private Coroutine hideMessageCoroutine;

    void Start()
    {
        // Находим PlayerHealth один раз при старте для оптимизации
        playerHealth = FindObjectOfType<PlayerHealth>();

        // При загрузке сцены применяем сохраненное состояние,
        // чтобы уничтожить уже подобранные аптечки.
        // Используем корутину для надежности, чтобы GameManager успел загрузить данные.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(InitializeState());
        }
    }

    private IEnumerator InitializeState()
    {
        // Ждем один кадр, чтобы дать GameManager время загрузить данные
        yield return null;

        if (GameManager.CurrentSaveData != null && GameManager.CurrentSaveData.collectedMedkitIDs.Contains(uniqueID))
        {
            // Если ID этой аптечки есть в списке собранных, уничтожаем ее
            Debug.Log($"[MedkitPickup] Аптечка с ID '{uniqueID}' уже собрана. Уничтожаю.");
            Destroy(gameObject);
        }
    }

    public override void OnLook()
    {
        if (playerHealth == null) return;
        
        // Если есть запланированное скрытие, отменяем его
        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
            hideMessageCoroutine = null;
        }

        playerHealth.ShowMessage("Восстановит все аптечки", 10f); // Показываем надолго
    }

    public override void OnLookAway()
    {
        if (playerHealth == null) return;

        // Запускаем корутину, которая скроет сообщение через 1 секунду
        hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay(1f));
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerHealth != null)
        {
            playerHealth.HideMessage();
        }
    }

    // Этот метод будет вызван, когда игрок наведет курсор и нажмет "E"
    public override void Interact()
    {
        if (playerHealth == null)
        {
            Debug.LogWarning("[MedkitPickup] Не удалось найти PlayerHealth на сцене.");
            return;
        }

        // Проверяем, не достигнут ли лимит аптечек
        if (playerHealth.HasMaxMedkits)
        {
            // Если аптечек максимум, показываем сообщение и не подбираем аптечку
            playerHealth.ShowMessage("Максимум аптечек", 2f);
            Debug.Log("[MedkitPickup] У игрока уже максимум аптечек.");
            return;
        }

        // Если место есть, добавляем одну аптечку
        playerHealth.AddMedkit(1);
        Debug.Log($"[MedkitPickup] Аптечка подобрана.");
        
        // Добавляем ID этой аптечки в список собранных в сохранении
        if (GameManager.Instance != null && GameManager.CurrentSaveData != null && !string.IsNullOrEmpty(uniqueID))
        {
            if (!GameManager.CurrentSaveData.collectedMedkitIDs.Contains(uniqueID))
            {
                GameManager.CurrentSaveData.collectedMedkitIDs.Add(uniqueID);
                Debug.Log($"[MedkitPickup] ID аптечки '{uniqueID}' добавлен в сохранение.");
            }
        }

        // Уничтожаем объект аптечки после подбора
        Destroy(gameObject);
    }

    // Этот метод нужен для системы сохранений, чтобы обрабатывать состояния,
    // но в нашем случае основная логика в Start()
    public override void ApplySavedState(string stateData)
    {
        // Если по какой-то причине состояние "collected" было сохранено через
        // UpdateInteractiveObjectState, мы также должны уничтожить объект.
        if (stateData == "collected")
        {
            Destroy(gameObject);
        }
    }
}