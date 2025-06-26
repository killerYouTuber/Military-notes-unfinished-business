using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    private float debugSpeedLogTimer = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Подписываемся на событие загрузки сцены, чтобы восстановить состояние игрока
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Регистрация игрока в PlayerManager при старте
        if (PlayerManager.instance != null)
        {
            PlayerManager.instance.SetPlayer(gameObject);
            Debug.Log($"[PlayerController] Player registered with PlayerManager: {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("[PlayerController] PlayerManager instance is null during Start. Player might not be registered.");
        }

        // Если есть сохраненные данные, восстанавливаем состояние
        if (GameManager.CurrentSaveData != null)
        {
            ApplySaveData(GameManager.CurrentSaveData);
            Debug.Log("[PlayerController] Applied saved data from GameManager.CurrentSaveData.");
            // GameManager.Instance.ClearCurrentSaveData(); // Очищаем данные после применения
        }
        else
        {
            Debug.Log("[PlayerController] No save data found in GameManager.CurrentSaveData. Resetting player state for new game.");
            // Сброс состояния игрока для новой игры
            var playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.currentHealth = playerHealth.maxHealth;
                Debug.Log($"[PlayerController] Player health reset to {playerHealth.currentHealth}");
            }
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Этот метод теперь не нужен, так как PlayerController сам берет данные
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    public void ApplySaveData(SaveData data)
    {
        if (data != null)
        {
            Debug.Log($"[ApplySaveData] secondaryWeaponId: '{data.secondaryWeaponId}'");
            Debug.Log($"[ApplySaveData] activeWeaponId: '{data.activeWeaponId}'");
            Debug.Log($"[ApplySaveData] SaveData JSON: {JsonUtility.ToJson(data, true)}");
            transform.position = data.playerPosition;
            transform.rotation = Quaternion.Euler(data.playerRotationX, data.playerRotationY, data.playerRotationZ);
            
            var playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.currentHealth = (int)data.playerHealth;
                playerHealth.SetMedkitCount(data.playerMedkits);
            }
            
            var playerShooting = GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                // Передаем все данные сохранения в PlayerShooting для обработки оружия
                playerShooting.ApplySaveData(data);
            }

            var gameMotor = GetComponent<GameMotor>();
            if (gameMotor != null)
            {
                gameMotor.SetCurrentSprintTime(data.sprintTime);
                gameMotor.SetCrouchingState(data.isCrouching); // Загружаем состояние приседания
            }

            // --- ЗАГРУЗКА ДАННЫХ О ГРАНАТАХ ---
            var grenadeSystem = GetComponent<GrenadeSystem>();
            if (grenadeSystem != null)
            {
                grenadeSystem.ApplySaveData(data.fragGrenades, data.smokeGrenades, data.currentGrenadeType);
            }
            // ------------------------------------

            Debug.Log($"[PlayerController] Loaded player position: {transform.position}, health: {playerHealth?.currentHealth}, sprint time: {gameMotor?.GetCurrentSprintTime()}, crouching: {gameMotor?.GetIsCrouching()}");
        }
    }

    public void CollectAndSetSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError("[PlayerController] CollectAndSetSaveData был вызван с null saveData. Создание нового экземпляра.");
            saveData = new SaveData();
        }

        saveData.sceneName = SceneManager.GetActiveScene().name;
        saveData.playerPosition = transform.position;
        
        // Сохраняем компоненты поворота игрока
        Vector3 playerRotation = transform.rotation.eulerAngles;
        saveData.playerRotationX = playerRotation.x;
        saveData.playerRotationY = playerRotation.y;
        saveData.playerRotationZ = playerRotation.z;
        
        // Сохраняем компоненты поворота камеры
        Vector3 cameraRotation = Camera.main.transform.rotation.eulerAngles;
        saveData.cameraRotationX = cameraRotation.x;
        saveData.cameraRotationY = cameraRotation.y;
        saveData.cameraRotationZ = cameraRotation.z;
        
        // Получаем здоровье из PlayerHealth
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            saveData.playerHealth = playerHealth.currentHealth;
            saveData.playerMedkits = playerHealth.GetMedkitCount();
        }

        // Получаем данные оружия
        PlayerShooting playerShooting = GetComponent<PlayerShooting>();
        if (playerShooting != null)
        {
            // Сохраняем актуальные патроны для всех инстанцированных оружий
            playerShooting.SaveAllWeaponsAmmoToDict();
            // Сохраняем weaponAmmoDict для всех типов оружия
            saveData.weaponAmmoList.Clear();
            foreach (var kvp in playerShooting.GetWeaponAmmoDict())
            {
                saveData.weaponAmmoList.Add(new WeaponAmmoSave {
                    weaponId = kvp.Key,
                    ammoTotal = kvp.Value.ammoTotal,
                    ammoInClip = kvp.Value.ammoInClip
                });
            }

            // Сохраняем патроны для текущего оружия (как было)
            if (playerShooting.currentWeapon != null && playerShooting.currentWeaponData != null)
            {
                saveData.ammoInClip = playerShooting.currentWeaponData.ammoInClip;
                saveData.totalAmmo = playerShooting.currentWeaponData.ammoTotal;
            }
            else
            {
                saveData.ammoInClip = 0;
                saveData.totalAmmo = 0;
            }
            saveData.fireMode = (int)playerShooting.fireMode;

            // Сохраняем активное оружие
            if (playerShooting._instantiatedPrimaryWeapon == playerShooting.currentWeapon)
            {
                saveData.activeWeaponId = "primary";
            }
            else if (playerShooting._instantiatedSecondaryWeapon == playerShooting.currentWeapon)
            {
                saveData.activeWeaponId = "secondary";
            }
            else if (playerShooting._instantiatedKnife == playerShooting.currentWeapon)
            {
                saveData.activeWeaponId = "knife";
            }
            else
            {
                saveData.activeWeaponId = "none";
            }

            // Сохраняем данные основного оружия
            if (playerShooting._instantiatedPrimaryWeapon != null)
            {
                string key = playerShooting._instantiatedPrimaryWeapon.name;
                if (playerShooting.GetWeaponAmmoDict().TryGetValue(key, out var ammo))
                {
                    saveData.primaryWeaponAmmoInClip = ammo.ammoInClip;
                    saveData.primaryWeaponTotalAmmo = ammo.ammoTotal;
                }
                else
                {
                    saveData.primaryWeaponAmmoInClip = playerShooting._instantiatedPrimaryWeapon.ammoInClip;
                    saveData.primaryWeaponTotalAmmo = playerShooting._instantiatedPrimaryWeapon.ammoTotal;
                }
            }
            else
            {
                saveData.primaryWeaponAmmoInClip = 0;
                saveData.primaryWeaponTotalAmmo = 0;
            }

            // Сохраняем данные второго оружия, если оно есть
            if (playerShooting._instantiatedSecondaryWeapon != null)
            {
                saveData.secondaryWeaponId = playerShooting._instantiatedSecondaryWeapon.name.Replace("(Clone)", "").Trim();
                var interactable = playerShooting._instantiatedSecondaryWeapon.GetComponent<WeaponInteractable>();
                if (interactable != null)
                {
                    saveData.secondaryWeaponUniqueID = interactable.uniqueID;
                }
                Debug.Log($"[Save] secondaryWeaponId: '{saveData.secondaryWeaponId}' (из {playerShooting._instantiatedSecondaryWeapon.name})");
                string key = playerShooting._instantiatedSecondaryWeapon.name;
                if (playerShooting.GetWeaponAmmoDict().TryGetValue(key, out var ammo))
                {
                    saveData.secondaryWeaponAmmoInClip = ammo.ammoInClip;
                    saveData.secondaryWeaponTotalAmmo = ammo.ammoTotal;
                }
                else
                {
                    saveData.secondaryWeaponAmmoInClip = playerShooting._instantiatedSecondaryWeapon.ammoInClip;
                    saveData.secondaryWeaponTotalAmmo = playerShooting._instantiatedSecondaryWeapon.ammoTotal;
                }
            }
            else
            {
                saveData.secondaryWeaponId = "";
                saveData.secondaryWeaponUniqueID = "";
                saveData.secondaryWeaponAmmoInClip = 0;
                saveData.secondaryWeaponTotalAmmo = 0;
            }

            // Сохраняем данные ножа
            if (playerShooting._instantiatedKnife != null)
            {
                saveData.knifeAmmoInClip = playerShooting._instantiatedKnife.ammoInClip;
                saveData.knifeTotalAmmo = playerShooting._instantiatedKnife.ammoTotal;
            }
            else
            {
                saveData.knifeAmmoInClip = 0;
                saveData.knifeTotalAmmo = 0;
            }

            // --- СБОР ДАННЫХ О ГРАНАТАХ ---
            var grenadeSystem = GetComponent<GrenadeSystem>();
            if (grenadeSystem != null)
            {
                saveData.fragGrenades = grenadeSystem.CurrentFragGrenades;
                saveData.smokeGrenades = grenadeSystem.CurrentSmokeGrenades;
                saveData.currentGrenadeType = (int)grenadeSystem.CurrentGrenadeType;
            }
            // ---------------------------------

            // Сохраняем патроны для всего оружия на сцене
            saveData.sceneWeaponAmmoList.Clear();
            foreach (var wi in FindObjectsOfType<WeaponInteractable>())
            {
                if (wi.transform.parent != null && wi.transform.parent.name.Contains("Holder"))
                    continue;
                if (wi.weaponParameters != null)
                {
                    // Проверка на дубликаты по uniqueID
                    if (saveData.sceneWeaponAmmoList.Any(x => x.uniqueID == wi.uniqueID))
                        continue;
                    Debug.Log($"[SAVE] Сохраняю оружие: uniqueID={wi.uniqueID}, type={wi.weaponParameters.name.Replace("(Clone)", "").Trim()}, pos={wi.transform.position}");
                    saveData.sceneWeaponAmmoList.Add(new SceneWeaponAmmoSave {
                        uniqueID = wi.uniqueID,
                        ammoInClip = wi.weaponParameters.ammoInClip,
                        ammoTotal = wi.weaponParameters.ammoTotal,
                        position = wi.transform.position,
                        rotation = wi.transform.rotation,
                        weaponTypeName = wi.weaponParameters.name.Replace("(Clone)", "").Trim()
                    });
                }
            }

            // Сохраняем состояние всех дверей на сцене
            saveData.sceneDoorStateList.Clear();
            foreach (var door in FindObjectsOfType<DoorInteractable>())
            {
                if (saveData.sceneDoorStateList.Any(x => x.uniqueID == door.uniqueID))
                    continue;
                saveData.sceneDoorStateList.Add(new SceneDoorStateSave {
                    uniqueID = door.uniqueID,
                    isOpen = GetPrivateIsOpen(door),
                    position = door.transform.position,
                    rotation = door.transform.rotation
                });
            }
        }

        // Получаем данные выносливости и приседания из GameMotor
        GameMotor gameMotor = GetComponent<GameMotor>();
        if (gameMotor != null)
        {
            saveData.sprintTime = gameMotor.GetCurrentSprintTime();
            saveData.isCrouching = gameMotor.GetIsCrouching(); // Сохраняем состояние приседания
        }

        Debug.Log($"[CollectAndSetSaveData] SaveData JSON: {JsonUtility.ToJson(saveData, true)}");
        GameManager.Instance.SetLastKnownPlayerSaveData(saveData);
    }

    // Вспомогательный метод для доступа к приватному полю isOpen
    private bool GetPrivateIsOpen(DoorInteractable door)
    {
        var field = typeof(DoorInteractable).GetField("isOpen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null && (bool)field.GetValue(door);
    }

    void Update()
    {
        debugSpeedLogTimer -= Time.deltaTime;
        if (debugSpeedLogTimer <= 0f)
        {
            var motor = GetComponent<GameMotor>();
            if (motor != null)
            {
                Debug.Log($"[PlayerController] Скорость игрока: {motor.CurrentVelocity:F2}");
            }
            debugSpeedLogTimer = 1f;
        }
    }
} 