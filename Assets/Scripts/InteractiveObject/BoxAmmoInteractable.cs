using UnityEngine;
using System.Collections;

public class BoxAmmoInteractable : Interactable
{
    [Header("Настройки ящика с патронами")]
    [Tooltip("Максимальное количество использований этого ящика.")]
    public int maxUses = 3;

    [Tooltip("Подсказка, когда ящик пуст.")]
    public string depletedPrompt = "Ящик пуст";

    private int currentUses;
    private bool isDepleted = false;
    private string originalPrompt;

    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;
    private Coroutine hideMessageCoroutine;

    void Start()
    {
        // Сохраняем оригинальную подсказку
        originalPrompt = interactionPrompt;

        // Загружаем состояние при старте
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(InitializeState());
        }
        else
        {
            // Если GameManager еще не готов, устанавливаем по умолчанию
            currentUses = maxUses;
        }

        // Находим компоненты игрока один раз
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerShooting = player.GetComponent<PlayerShooting>();
        }
    }

    private IEnumerator InitializeState()
    {
        yield return null; // Ждем один кадр

        if (GameManager.CurrentSaveData != null)
        {
            var savedState = GameManager.CurrentSaveData.interactiveObjectStates.Find(s => s.uniqueID == uniqueID);
            if (!string.IsNullOrEmpty(savedState.stateData))
            {
                ApplySavedState(savedState.stateData);
            }
            else
            {
                // Если сохраненного состояния нет, используем максимальное значение
                currentUses = maxUses;
            }
        }
        else
        {
            currentUses = maxUses;
        }

        UpdateState();
    }

    public override void OnLook()
    {
        if (playerHealth == null) return;

        if (hideMessageCoroutine != null)
        {
            StopCoroutine(hideMessageCoroutine);
            hideMessageCoroutine = null;
        }

        string message = isDepleted 
            ? "Ящик пуст" 
            : $"Восполнит бойзапас ({currentUses} раз(а))";
        
        playerHealth.ShowMessage(message, 10f);
    }

    public override void OnLookAway()
    {
        if (playerHealth == null) return;
        
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

    public override void Interact()
    {
        if (playerHealth == null || playerShooting == null)
        {
            Debug.LogError("[BoxAmmo] Не найдены компоненты игрока!");
            return;
        }

        if (isDepleted)
        {
            playerHealth.ShowMessage("Ящик пуст", 2f);
            Debug.Log("[BoxAmmo] Ящик пуст, взаимодействие невозможно.");
            return;
        }

        if (playerShooting.HasMaxAmmoOnAllWeapons)
        {
            playerHealth.ShowMessage("У вас полный боезапас", 2f);
            Debug.Log("[BoxAmmo] У игрока уже полный боезапас.");
            return;
        }

        // Пополняем патроны и гранаты
        playerShooting.RestoreAllAmmoToMax();
        playerShooting.SwitchToWeapon(playerShooting.currentWeapon);
        
        GrenadeSystem grenadeSystem = FindObjectOfType<GrenadeSystem>();
        if (grenadeSystem != null)
        {
            grenadeSystem.RestoreAllGrenadesToMax();
        }
        
        // Принудительно обновляем UI для текущего активного режима (оружие или гранаты)
        if (grenadeSystem != null && grenadeSystem.isGrenadeMode)
        {
            grenadeSystem.UpdateGrenadeUI();
        }
        else if (playerShooting != null)
        {
            playerShooting.RefreshAmmoText();
        }

        // Уменьшаем количество использований
        currentUses--;

        // Обновляем и сохраняем состояние
        UpdateState();
        GameManager.Instance.UpdateInteractiveObjectState(uniqueID, currentUses.ToString());

        // После успешного взаимодействия обновляем подсказку
        OnLook();
    }

    private void UpdateState()
    {
        if (currentUses <= 0)
        {
            currentUses = 0;
            isDepleted = true;
            interactionPrompt = depletedPrompt; // Меняем подсказку на "Ящик пуст"
        }
        else
        {
            isDepleted = false;
            interactionPrompt = originalPrompt; // Возвращаем стандартную подсказку
        }
    }

    public override void ApplySavedState(string stateData)
    {
        if (int.TryParse(stateData, out int savedUses))
        {
            currentUses = savedUses;
            Debug.Log($"[BoxAmmo] Загружено состояние для '{uniqueID}': {currentUses} использований осталось.");
        }
        else
        {
            // Если данные некорректны, используем значение по умолчанию
            currentUses = maxUses;
        }
        UpdateState();
    }
} 