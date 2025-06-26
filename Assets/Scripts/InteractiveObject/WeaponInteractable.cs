using UnityEngine;
using System.Collections;

public class WeaponInteractable : Interactable
{
    [Header("Параметры оружия")]
    public WeaponParametrs weaponParameters;

    private bool isEquipped = false;
    private GameObject weaponInstance;

    void Start()
    {
        // Если оружие находится в руках игрока (в холдере), не делаем проверку на удаление
        if (transform.parent != null && transform.parent.name.Contains("Holder"))
        {
            Debug.Log($"[WeaponInteractable] {gameObject.name} находится в руках игрока (parent: {transform.parent.name}), не проверяем на удаление.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(InitializeWeaponState());
            }
            TryRestoreAmmoFromSave();
            return;
        }

        // Проверка: если это оружие уже у игрока, удаляем его со сцены (ТОЛЬКО ПРИ ЗАГРУЗКЕ)
        if (GameManager.Instance != null && GameManager.Instance.IsLoading && GameManager.CurrentSaveData != null)
        {
            string secID = GameManager.CurrentSaveData.secondaryWeaponUniqueID;
            if (!string.IsNullOrEmpty(secID) && uniqueID == secID)
            {
                Debug.Log($"[WeaponInteractable] Оружие с uniqueID={uniqueID} уже у игрока (secondary={secID}), удаляю дубликат со сцены во время загрузки!");
                Destroy(gameObject);
                return;
            }
        }
        // При загрузке сцены применяем сохраненное состояние
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartCoroutine(InitializeWeaponState());
        }
        // Восстанавливаем патроны из сохранения
        TryRestoreAmmoFromSave();
    }

    private IEnumerator InitializeWeaponState()
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

    private void TryRestoreAmmoFromSave()
    {
        var save = GameManager.CurrentSaveData;
        if (save != null && save.sceneWeaponAmmoList != null)
        {
            var ammoSave = save.sceneWeaponAmmoList.Find(x => x.uniqueID == uniqueID);
            if (ammoSave != null && weaponParameters != null)
            {
                weaponParameters.ammoInClip = ammoSave.ammoInClip;
                weaponParameters.ammoTotal = ammoSave.ammoTotal;
            }
        }
    }

    public void SaveCurrentAmmoToGameManager()
    {
        if (GameManager.Instance != null && GameManager.CurrentSaveData != null)
        {
            var list = GameManager.CurrentSaveData.sceneWeaponAmmoList;
            var ammoSave = list.Find(x => x.uniqueID == uniqueID);
            if (ammoSave != null)
            {
                ammoSave.ammoInClip = weaponParameters.ammoInClip;
                ammoSave.ammoTotal = weaponParameters.ammoTotal;
            }
            else
            {
                list.Add(new SceneWeaponAmmoSave {
                    uniqueID = uniqueID,
                    ammoInClip = weaponParameters.ammoInClip,
                    ammoTotal = weaponParameters.ammoTotal
                });
            }
        }
    }

    public override void Interact()
    {
        if (isEquipped) return;

        PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.PickupWeapon(weaponParameters);
            isEquipped = true;

            // Сохраняем информацию о том, что это оружие было подобрано
            if (GameManager.Instance != null && !string.IsNullOrEmpty(uniqueID))
            {
                GameManager.Instance.UpdateInteractiveObjectState(uniqueID, "picked_up");
                // Убираем автоматическое сохранение - это перезаписывало основной сейв
                // GameManager.Instance.SaveGame(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }
    }

    // Переопределяем ApplySavedState для обработки состояния оружия
    public override void ApplySavedState(string stateData)
    {
        // Оружие больше не удаляем при загрузке!
    }
}