using TMPro;
using UnityEngine;
// using UnityEngine.SceneManagement; // Не требуется для этого скрипта, так как событие статическое
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire Modes")]
    public FireMode fireMode = FireMode.SemiAuto;
    private int burstShotsLeft;
    private bool isBurstFiring;
    private bool isAutoFiring;
    private bool isAiming = false;
    private float defaultFOV;
    private Vector3 originalWeaponPosition;
    private Quaternion originalWeaponRotation;

    [Header("References")]
    public TMP_Text totalAmmoText;
    public TMP_Text clipAmmoText;
    public TMP_Text fireModeText;
    public WeaponParametrs currentWeapon;
    public WeaponParametrs.WeaponParametersData currentWeaponData; // Копия параметров для работы
    public bool weaponReady;
    public Transform hitPoint;
    private AudioSource au;
    private Camera mainCamera;
    private GrenadeSystem grenadeSystem;

    [Header("Оружие (Префабы)")]
    public WeaponParametrs primaryWeaponPrefab;    // Префаб основного оружия (Colt)
    public WeaponParametrs secondaryWeaponPrefab;  // Префаб подбираемого оружия
    public WeaponParametrs knifePrefab;            // Префаб ножа

    // Ссылки на инстанцированные объекты оружия в сцене
    public WeaponParametrs _instantiatedPrimaryWeapon;
    public WeaponParametrs _instantiatedSecondaryWeapon;
    public WeaponParametrs _instantiatedKnife;

    [Header("Точки крепления")]
    public Transform primaryWeaponHolder;    // Точка крепления основного оружия
    public Transform secondaryWeaponHolder;  // Точка крепления подбираемого оружия (устаревшая, для обратной совместимости)
    public Transform knifeHolder;            // Точка крепления ножа

    [System.Serializable]
    public class WeaponHolderMapping
    {
        public WeaponParametrs weaponPrefab; // Префаб оружия, для которого предназначена эта точка
        public Transform weaponHolder;       // Точка крепления для этого типа оружия
    }
    
    [Header("Система множественных точек крепления второстепенного оружия")]
    public List<WeaponHolderMapping> secondaryWeaponHolders = new List<WeaponHolderMapping>();

    [Header("Список всех префабов оружия")]
    public List<WeaponParametrs> weaponPrefabs;

    private Dictionary<string, (int ammoTotal, int ammoInClip)> weaponAmmoDict = new();

    public enum FireMode
    {
        Auto,       // Автоматический - стрельба при зажатой кнопке
        SemiAuto,   // Полуавтоматический - стрельба при нажатии
        Burst       // Очередью - по 3 выстрела на нажатие
    }

    public WeaponParametrs pendingWeaponToActivate = null; // Оружие, подобранное в режиме гранаты
    public WeaponParametrs previousWeaponBeforeGrenadeMode = null; // Оружие, которое было в руках до режима гранат

    public bool HasMaxAmmoOnAllWeapons
    {
        get
        {
            bool primaryFull = true;
            if (_instantiatedPrimaryWeapon != null && !_instantiatedPrimaryWeapon.isKnife)
            {
                // Если основное оружие сейчас в руках, берем актуальные данные из currentWeaponData.
                // Иначе - берем данные из самого объекта.
                int totalAmmo = (currentWeapon == _instantiatedPrimaryWeapon)
                    ? currentWeaponData.ammoTotal
                    : _instantiatedPrimaryWeapon.ammoTotal;
                
                primaryFull = totalAmmo >= _instantiatedPrimaryWeapon.initialTotalAmmo;
            }

            bool secondaryFull = true;
            if (_instantiatedSecondaryWeapon != null && !_instantiatedSecondaryWeapon.isKnife)
            {
                // Аналогичная логика для второстепенного оружия.
                int totalAmmo = (currentWeapon == _instantiatedSecondaryWeapon)
                    ? currentWeaponData.ammoTotal
                    : _instantiatedSecondaryWeapon.ammoTotal;

                secondaryFull = totalAmmo >= _instantiatedSecondaryWeapon.initialTotalAmmo;
            }
            
            return primaryFull && secondaryFull;
        }
    }

    void Start()
    {
        Debug.Log($"[PlayerShooting:Start] primaryWeaponHolder: {primaryWeaponHolder}, secondaryWeaponHolder: {secondaryWeaponHolder}, knifeHolder: {knifeHolder}");
        Debug.Log($"[PlayerShooting:Start] primaryWeaponPrefab: {primaryWeaponPrefab}, secondaryWeaponPrefab: {secondaryWeaponPrefab}, knifePrefab: {knifePrefab}");
        
        // Валидируем настройки точек крепления
        ValidateWeaponHolders();
        
        au = GetComponent<AudioSource>();
        if (au == null)
        {
            au = gameObject.AddComponent<AudioSource>();
        }

        // --- НАЧАЛО ИЗМЕНЕНИЙ: АВТОМАТИЧЕСКИЙ ПОИСК UI ---
        if (totalAmmoText == null)
        {
            GameObject obj = GameObject.Find("TotalAmmo_Text");
            if (obj != null) totalAmmoText = obj.GetComponent<TMP_Text>();
            else Debug.LogError("PlayerShooting: UI элемент 'TotalAmmo_Text' не найден на сцене! Проверьте имя объекта.");
        }

        if (clipAmmoText == null)
        {
            GameObject obj = GameObject.Find("ClipAmmo_Text");
            if (obj != null) clipAmmoText = obj.GetComponent<TMP_Text>();
            else Debug.LogError("PlayerShooting: UI элемент 'ClipAmmo_Text' не найден на сцене! Проверьте имя объекта.");
        }

        if (fireModeText == null)
        {
            GameObject obj = GameObject.Find("FireMode_Text");
            if (obj != null) fireModeText = obj.GetComponent<TMP_Text>();
            else Debug.LogError("PlayerShooting: UI элемент 'FireMode_Text' не найден на сцене! Проверьте имя объекта.");
        }
        // --- КОНЕЦ ИЗМЕНЕНИЙ ---

        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            defaultFOV = mainCamera.fieldOfView;
        }

        // Инициализируем систему гранат
        grenadeSystem = GetComponent<GrenadeSystem>();

        // Этот блок инициализации теперь будет вызываться только если нет сохраненных данных,
        // иначе ApplySaveData() возьмет на себя инициализацию оружия.
        if (GameManager.CurrentSaveData == null || string.IsNullOrEmpty(GameManager.CurrentSaveData.sceneName))
        {
            // Попытка найти существующее основное оружие на префабе в его точке крепления
            if (primaryWeaponHolder != null)
            {
                _instantiatedPrimaryWeapon = primaryWeaponHolder.GetComponentInChildren<WeaponParametrs>();
            }

            // Если основное оружие не найдено на префабе, но префаб указан, инстанцируем его
            if (_instantiatedPrimaryWeapon == null && primaryWeaponPrefab != null)
            {
                _instantiatedPrimaryWeapon = InstantiateWeapon(primaryWeaponPrefab, primaryWeaponHolder);
                if (_instantiatedPrimaryWeapon != null)
                {
                    _instantiatedPrimaryWeapon.ammoInClip = _instantiatedPrimaryWeapon.clipSize;
                    _instantiatedPrimaryWeapon.ammoTotal = _instantiatedPrimaryWeapon.initialTotalAmmo;
                }
            }

            // Инстанцируем нож, если префаб задан
            if (knifePrefab != null)
            {
                _instantiatedKnife = InstantiateWeapon(knifePrefab, knifeHolder);
            }

            // Устанавливаем текущее оружие при старте новой игры
            if (_instantiatedPrimaryWeapon != null)
            {
                SwitchToWeapon(_instantiatedPrimaryWeapon);
            }
            else if (_instantiatedKnife != null)
            {
                SwitchToWeapon(_instantiatedKnife);
            }
            else // Если нет ни основного, ни ножа, устанавливаем currentWeapon в null (нет оружия в руках)
            {
                currentWeapon = null;
            }
        }
    }

    void Update()
    {
        if (PauseManager.IsPaused) return;
        
        HandleAiming();
        
        // --- НОВАЯ ЛОГИКА УПРАВЛЕНИЯ ---
        if (grenadeSystem != null && grenadeSystem.isGrenadeMode)
        {
            // В режиме гранат: 1 - Осколочная, 2 - Дымовая
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                grenadeSystem.SwitchToGrenadeType(GrenadeSystem.GrenadeType.Frag);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                grenadeSystem.SwitchToGrenadeType(GrenadeSystem.GrenadeType.Smoke);
            }
        }
        else
        {
            // В обычном режиме: 1, 2, 3 - переключение оружия
            if (Input.GetKeyDown(KeyCode.Alpha1) && _instantiatedPrimaryWeapon != null)
            {
                SwitchToWeapon(_instantiatedPrimaryWeapon);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && _instantiatedSecondaryWeapon != null)
            {
                SwitchToWeapon(_instantiatedSecondaryWeapon);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) && _instantiatedKnife != null)
            {
                SwitchToWeapon(_instantiatedKnife);
            }
        }
    }

    public void SaveAllWeaponsAmmoToDict()
    {
        if (_instantiatedPrimaryWeapon != null)
            weaponAmmoDict[_instantiatedPrimaryWeapon.name] = (_instantiatedPrimaryWeapon.ammoTotal, _instantiatedPrimaryWeapon.ammoInClip);
        if (_instantiatedSecondaryWeapon != null)
            weaponAmmoDict[GetAmmoKey(_instantiatedSecondaryWeapon)] = (_instantiatedSecondaryWeapon.ammoTotal, _instantiatedSecondaryWeapon.ammoInClip);
        if (_instantiatedKnife != null)
            weaponAmmoDict[GetAmmoKey(_instantiatedKnife)] = (_instantiatedKnife.ammoTotal, _instantiatedKnife.ammoInClip);
    }

    public Dictionary<string, (int ammoTotal, int ammoInClip)> GetWeaponAmmoDict()
    {
        return weaponAmmoDict;
    }

    public void SetWeaponAmmoDict(List<WeaponAmmoSave> weaponAmmoList)
    {
        weaponAmmoDict.Clear();
        if (weaponAmmoList != null)
        {
            foreach (var w in weaponAmmoList)
            {
                weaponAmmoDict[w.weaponId] = (w.ammoTotal, w.ammoInClip);
            }
        }
    }

    private WeaponParametrs InstantiateWeapon(WeaponParametrs prefab, Transform holder)
    {
        var weapon = Instantiate(prefab, holder);
        string ammoKey = (prefab == primaryWeaponPrefab) ? prefab.name : GetAmmoKey(prefab);
        if (weaponAmmoDict.TryGetValue(ammoKey, out var ammo))
        {
            weapon.ammoTotal = ammo.ammoTotal;
            weapon.ammoInClip = ammo.ammoInClip;
        }
        else
        {
            weapon.ammoTotal = prefab.initialTotalAmmo;
            weapon.ammoInClip = prefab.clipSize;
        }
        return weapon;
    }

    public void SwitchToWeapon(WeaponParametrs weaponToActivate)
    {
        // --- ИЗМЕНЕНИЕ: Выключаем режим гранат при выборе оружия ---
        if (grenadeSystem != null)
        {
            grenadeSystem.isGrenadeMode = false;
        }

        // Сбрасываем прицеливание перед сменой оружия
        SetAiming(false);

        if (currentWeapon != null && !currentWeapon.isKnife)
        {
            string ammoKey = (currentWeapon == _instantiatedPrimaryWeapon) ? currentWeapon.name : GetAmmoKey(currentWeapon);
            Debug.Log($"[SwitchToWeapon] Сохраняю патроны: key={ammoKey}, total={currentWeaponData.ammoTotal}, clip={currentWeaponData.ammoInClip}");
            weaponAmmoDict[ammoKey] = (currentWeaponData.ammoTotal, currentWeaponData.ammoInClip);
            currentWeapon.ammoTotal = currentWeaponData.ammoTotal;
            currentWeapon.ammoInClip = currentWeaponData.ammoInClip;
        }
        if (_instantiatedPrimaryWeapon != null) _instantiatedPrimaryWeapon.gameObject.SetActive(false);
        if (_instantiatedSecondaryWeapon != null) _instantiatedSecondaryWeapon.gameObject.SetActive(false);
        if (_instantiatedKnife != null) _instantiatedKnife.gameObject.SetActive(false);

        if (weaponToActivate != null)
        {
            currentWeapon = weaponToActivate;
            currentWeapon.gameObject.SetActive(true);
            currentWeapon.transform.localScale = currentWeapon.handScale;
            currentWeaponData = new WeaponParametrs.WeaponParametersData(currentWeapon); // Копируем параметры
            string newAmmoKey = (currentWeapon == _instantiatedPrimaryWeapon) ? currentWeapon.name : GetAmmoKey(currentWeapon);
            if (!currentWeapon.isKnife && weaponAmmoDict.TryGetValue(newAmmoKey, out var ammo))
            {
                Debug.Log($"[SwitchToWeapon] Загружаю патроны: key={newAmmoKey}, total={ammo.ammoTotal}, clip={ammo.ammoInClip}");
                currentWeaponData.ammoTotal = ammo.ammoTotal;
                currentWeaponData.ammoInClip = ammo.ammoInClip;
            }
            else
            {
                Debug.Log($"[SwitchToWeapon] Нет сохранённых патронов для key={newAmmoKey}, используются значения по умолчанию: total={currentWeaponData.ammoTotal}, clip={currentWeaponData.ammoInClip}");
            }
        }
        else
        {
            currentWeapon = null;
            currentWeaponData = null;
        }
        RefreshAmmoText();
        UpdateFireModeDisplay();
        weaponReady = true;

        SetWeaponUIVisibility(true); // Показываем UI оружия при переключении на него
    }

    /// <summary>
    /// Управляет видимостью всех элементов UI, связанных с оружием.
    /// </summary>
    public void SetWeaponUIVisibility(bool visible)
    {
        if (totalAmmoText != null) totalAmmoText.gameObject.SetActive(visible);
        if (clipAmmoText != null) clipAmmoText.gameObject.SetActive(visible);
        if (fireModeText != null) fireModeText.gameObject.SetActive(visible);
    }

    public void HandleShootingStartInput()
    {
        if (!weaponReady || isBurstFiring) return;
        if (currentWeapon == null) return;

        // Если в руках нож — вызываем Melee
        if (currentWeapon.isKnife)
        {
            Melee();
            return;
        }

        switch (fireMode)
        {
            case FireMode.SemiAuto:
                TryShoot();
                break;
            case FireMode.Burst:
                if (!isBurstFiring)
                {
                    burstShotsLeft = currentWeapon.burstCount;
                    isBurstFiring = true;
                    StartCoroutine(BurstFireCoroutine());
                }
                break;
            case FireMode.Auto:
                isAutoFiring = true;
                break;
        }
    }

    public void HandleContinuousShootingInput()
    {
        if (currentWeapon == null) return; // Не стреляем, если нет оружия
        if (isAutoFiring && CanShoot())
        {
            Shoot();
        }
    }

    public void HandleShootingEndInput()
    {
        if (fireMode == FireMode.Auto)
        {
            isAutoFiring = false;
        }
        // В Burst режиме, если отпускаем кнопку, когда очередь еще не закончилась, она останавливается.
        // isBurstFiring теперь управляется только корутиной BurstFireCoroutine
        // if (fireMode == FireMode.Burst)
        // {
        //     isBurstFiring = false;
        //     burstShotsLeft = 0;
        // }
    }

    public void TryShoot()
    {
        if (currentWeapon == null) return; // Не стреляем, если нет оружия

        if (CanShoot())
        {
            Shoot();
        }
        else if (currentWeaponData.ammoInClip <= 0 && !currentWeaponData.isKnife)
        {
            Reload();
            RefreshAmmoText();
        }
        else
        {
            RefreshAmmoText();
        }
    }

    private bool CanShoot()
    {
        if (currentWeapon == null) return false; // Не можем стрелять, если нет оружия

        if (fireMode == FireMode.Burst)
        {
            return (currentWeaponData.ammoInClip > 0 || currentWeaponData.isKnife);
        }
        
        return weaponReady &&
               (currentWeaponData.ammoInClip > 0 || currentWeaponData.isKnife) &&
               Time.time >= currentWeapon.lastTimeShoot;
    }

    public void PickupWeapon(WeaponParametrs newWeaponOnField)
    {
        if (newWeaponOnField == null) return;

        // --- УЛУЧШЕННЫЙ ПОИСК ПРЕФАБА ---
        // Ищем префаб по имени объекта на земле или по его полю weaponPrefab
        string weaponNameToFind = newWeaponOnField.weaponPrefab != null 
            ? newWeaponOnField.weaponPrefab.name.Replace("(Clone)", "").Trim() 
            : newWeaponOnField.name.Replace("(Clone)", "").Trim();

        WeaponParametrs prefab = weaponPrefabs.Find(p => 
            p.name.Replace("(Clone)", "").Trim() == weaponNameToFind || 
            (p.weaponPrefab != null && p.weaponPrefab.name.Replace("(Clone)", "").Trim() == weaponNameToFind)
        );

        if (prefab == null)
        {
            Debug.LogError($"[PickupWeapon] Префаб для оружия '{weaponNameToFind}' не найден в списке weaponPrefabs!");
            return;
        }
        
        Transform weaponHolder = GetWeaponHolder(prefab);
        if (weaponHolder == null)
        {
            Debug.LogError($"[PickupWeapon] Не найдена точка крепления для оружия: {prefab.name}");
            return;
        }

        string pickedUniqueID = newWeaponOnField.GetComponent<WeaponInteractable>()?.uniqueID;

        // Если уже есть второе оружие, выбрасываем его
        if (_instantiatedSecondaryWeapon != null)
        {
            Debug.Log($"[PickupWeapon] Выбрасываю текущее второе оружие: {_instantiatedSecondaryWeapon.name}");
            DropWeapon(_instantiatedSecondaryWeapon);
        }

        // Инстанцируем новое оружие в руках
        GameObject weaponObj = Instantiate(prefab.gameObject, weaponHolder.position, weaponHolder.rotation, weaponHolder);
        weaponObj.transform.localPosition = Vector3.zero;
        weaponObj.transform.localRotation = Quaternion.identity;
        weaponObj.transform.localScale = prefab.handScale;

        _instantiatedSecondaryWeapon = weaponObj.GetComponent<WeaponParametrs>();
        if (_instantiatedSecondaryWeapon == null)
        {
            Debug.LogError("[PickupWeapon] На инстанцированном оружии отсутствует компонент WeaponParametrs!");
            Destroy(weaponObj);
            return;
        }

        // Настраиваем новое оружие
        _instantiatedSecondaryWeapon.name = prefab.name; // Используем чистое имя префаба
        _instantiatedSecondaryWeapon.weaponPrefab = prefab.weaponPrefab ?? prefab; // Важно: восстанавливаем ссылку на префаб!
        
        // Переносим патроны и uniqueID с подобранного оружия
        _instantiatedSecondaryWeapon.ammoInClip = newWeaponOnField.ammoInClip;
        _instantiatedSecondaryWeapon.ammoTotal = newWeaponOnField.ammoTotal;

        var interactableInHand = weaponObj.GetComponent<WeaponInteractable>() ?? weaponObj.AddComponent<WeaponInteractable>();
        interactableInHand.uniqueID = pickedUniqueID;
        interactableInHand.weaponParameters = _instantiatedSecondaryWeapon;

        // Отключаем физику
        if (weaponObj.GetComponent<Rigidbody>() is Rigidbody rb) rb.isKinematic = true;
        if (weaponObj.GetComponent<Collider>() is Collider col) col.enabled = false;

        // Уничтожаем объект на земле
        Destroy(newWeaponOnField.gameObject);

        // Переключаемся на новое оружие (если не в режиме гранаты)
        if (grenadeSystem == null || !grenadeSystem.IsGrenadeMode)
        {
            SwitchToWeapon(_instantiatedSecondaryWeapon);
        }
        else
        {
            pendingWeaponToActivate = _instantiatedSecondaryWeapon;
            _instantiatedSecondaryWeapon.gameObject.SetActive(false);
        }
        
        RefreshAmmoText();
    }

    // --- Новый метод ---
    public void ActivatePendingWeapon()
    {
        if (pendingWeaponToActivate != null)
        {
            pendingWeaponToActivate.gameObject.SetActive(true);
            SwitchToWeapon(pendingWeaponToActivate);
            Debug.Log($"[PlayerShooting] Активировано ранее подобранное оружие: {pendingWeaponToActivate.name}");
            pendingWeaponToActivate = null;
        }
    }

    public void DropWeapon(WeaponParametrs weaponToDrop)
    {
        if (weaponToDrop == null) return;

        // --- УЛУЧШЕННЫЙ ПОИСК ПРЕФАБА ---
        string weaponNameToFind = (weaponToDrop.weaponPrefab != null) 
            ? weaponToDrop.weaponPrefab.name.Replace("(Clone)", "").Trim() 
            : weaponToDrop.name.Replace("(Clone)", "").Trim();

        WeaponParametrs prefab = weaponPrefabs.Find(p => 
            p.name.Replace("(Clone)", "").Trim() == weaponNameToFind || 
            (p.weaponPrefab != null && p.weaponPrefab.name.Replace("(Clone)", "").Trim() == weaponNameToFind)
        );
        
        if (prefab == null)
        {
            Debug.LogError($"[DropWeapon] Префаб для оружия '{weaponNameToFind}' не найден в списке! Выброс отменен.");
            return;
        }

        // Создаём копию на основе префаба, а не объекта в руках, чтобы избежать проблем с дочерними объектами
        GameObject droppedWeaponObj = Instantiate(prefab.gameObject, transform.position + transform.forward * 1.5f + Vector3.up * 0.5f, Quaternion.identity);
        
        // Переносим состояние (патроны, uniqueID) с оружия в руках на выброшенное
        WeaponParametrs droppedWeaponParams = droppedWeaponObj.GetComponent<WeaponParametrs>();
        droppedWeaponParams.ammoInClip = weaponToDrop.ammoInClip;
        droppedWeaponParams.ammoTotal = weaponToDrop.ammoTotal;
        droppedWeaponParams.weaponPrefab = prefab.weaponPrefab ?? prefab;

        var oldInteractable = weaponToDrop.GetComponent<WeaponInteractable>();
        var newInteractable = droppedWeaponObj.GetComponent<WeaponInteractable>() ?? droppedWeaponObj.AddComponent<WeaponInteractable>();
        newInteractable.uniqueID = oldInteractable?.uniqueID ?? System.Guid.NewGuid().ToString();
        newInteractable.weaponParameters = droppedWeaponParams;

        // Включаем физику
        if (droppedWeaponObj.GetComponent<Rigidbody>() is Rigidbody rb)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
        }
        if (droppedWeaponObj.GetComponent<Collider>() is Collider col) col.enabled = true;

        // Уничтожаем объект оружия из рук игрока
        Destroy(weaponToDrop.gameObject);

        // Обнуляем ссылки
        if (weaponToDrop == _instantiatedSecondaryWeapon)
        {
            _instantiatedSecondaryWeapon = null;
        }
    }

    public void SwitchFireMode()
    {
        if (currentWeapon != null && currentWeapon.isKnife)
        {
            Debug.Log("Нельзя менять режим стрельбы для ножа.");
            return;
        }

        fireMode = (FireMode)(((int)fireMode + 1) % 3);

        isAutoFiring = false;
        isBurstFiring = false;
        burstShotsLeft = 0;

        UpdateFireModeDisplay();
    }

    public void UpdateFireModeDisplay()
    {
        if (fireModeText != null)
        {
            // Проверяем, находимся ли мы в режиме гранат
            if (grenadeSystem != null && grenadeSystem.IsGrenadeMode)
            {
                // В режиме гранат показываем тип гранаты
                switch (grenadeSystem.CurrentGrenadeType)
                {
                    case GrenadeSystem.GrenadeType.Frag:
                        fireModeText.text = "Осколочная";
                        break;
                    case GrenadeSystem.GrenadeType.Smoke:
                        fireModeText.text = "Дымовая";
                        break;
                    default:
                        fireModeText.text = "Граната";
                        break;
                }
                return;
            }

            // Обычный режим - показываем режим стрельбы
            if (currentWeapon != null && currentWeapon.isKnife)
            {
                fireModeText.text = ""; // Скрываем текст режима стрельбы для ножа
                return;
            }
            switch (fireMode)
            {
                case FireMode.Auto:
                    fireModeText.text = "AUTO";
                    break;
                case FireMode.SemiAuto:
                    fireModeText.text = "SEMI";
                    break;
                case FireMode.Burst:
                    if (currentWeapon != null)
                    {
                        fireModeText.text = $"BURST {currentWeapon.burstCount}";
                    }
                    else
                    {
                        fireModeText.text = "BURST (оружие не готово)";
                    }
                    break;
            }
        }
    }

    void NotifyEnemiesOfShot()
{
    if (currentWeapon.alertsEnemies)
    {
        // Рассчитываем радиус шума здесь, а не берем из несуществующего свойства
        float noiseRadius = currentWeapon.hasSilencer 
            ? currentWeapon.shotSoundRadius * currentWeapon.silencerMultiplier 
            : currentWeapon.shotSoundRadius;

        Debug.Log($"[Noise] Создан шум от ВЫСТРЕЛА с радиусом {noiseRadius}");
        NoiseManager.EmitNoise(transform.position, noiseRadius);
    }
}

    public void Shoot(bool decreaseAmmo = true)
    {
        if (currentWeapon == null) return; // Не стреляем, если нет оружия
        if (fireMode != FireMode.Burst && !CanShoot()) return;

        if (currentWeapon.shootClip != null &&
            currentWeapon.shootClip.Length > 0)
        {
            au.PlayOneShot(currentWeapon.shootClip[Random.Range(0, currentWeapon.shootClip.Length)]);
        }

        if (currentWeapon.muzzleFlash.Length > 0)
        {
            Instantiate(
                currentWeapon.muzzleFlash[Random.Range(0, currentWeapon.muzzleFlash.Length)],
                currentWeapon.shootPoint.position,
                Quaternion.identity
            );
        }

        if (decreaseAmmo && !currentWeaponData.isKnife)
        {
            currentWeaponData.ammoInClip--;
            RefreshAmmoText();
        }

        if (fireMode != FireMode.Burst)
        {
            currentWeapon.lastTimeShoot = Time.time + currentWeaponData.shootCoolDown;
        }

        // --- Новая логика выстрела ---
        if (currentWeaponData.isShotgun)
        {
            int pelletCount = Mathf.Max(1, currentWeaponData.pelletCount);
            float damagePerPellet = currentWeaponData.damage / pelletCount;

            for (int i = 0; i < pelletCount; i++)
            {
                FireSingleProjectile(damagePerPellet);
            }
        }
        else
        {
            FireSingleProjectile(currentWeaponData.damage);
        }

        HandleShotAlert(currentWeapon);
    }

    private void FireSingleProjectile(float damage)
    {
        Vector3 shootDirection = mainCamera.transform.forward;
        float currentSpread = isAiming ? currentWeaponData.aimSpread : currentWeaponData.spread;
        float spreadX = Random.Range(-currentSpread, currentSpread);
        float spreadY = Random.Range(-currentSpread, currentSpread);
        shootDirection = Quaternion.Euler(spreadY, spreadX, 0) * shootDirection;

        RaycastHit hit;
        if (Physics.Raycast(mainCamera.transform.position, shootDirection, out hit, currentWeaponData.range))
        {
            hitPoint.position = hit.point;
        }
        else
        {
            hitPoint.position = mainCamera.transform.position + shootDirection * currentWeaponData.range;
        }

        if (currentWeaponData.bulletPrefab != null)
        {
            GameObject bullet = Instantiate(
                currentWeaponData.bulletPrefab,
                currentWeapon.shootPoint.position,
                Quaternion.LookRotation(shootDirection)
            );

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = damage;
                bulletScript.speed = currentWeaponData.bulletSpeed;
            }
        }
    }

    public void Reload()
    {
        if (currentWeapon == null || currentWeaponData.isKnife) return;

        if (currentWeaponData.ammoTotal > 0 && currentWeaponData.ammoInClip < currentWeaponData.clipSize)
        {
            int ammoToReload = Mathf.Min(currentWeaponData.clipSize - currentWeaponData.ammoInClip, currentWeaponData.ammoTotal);
            currentWeaponData.ammoInClip += ammoToReload;
            currentWeaponData.ammoTotal -= ammoToReload;
            RefreshAmmoText();
        }
    }

    public void RestoreAllAmmoToMax()
    {
        if (_instantiatedPrimaryWeapon != null)
        {
            // Обновляем патроны для всех оружий в словаре, который используется при сохранении.
            var keys = new List<string>(weaponAmmoDict.Keys);
            foreach (string weaponKey in keys)
            {
                string cleanKey = weaponKey.Replace("(Clone)", "").Trim();
                var prefab = weaponPrefabs.FirstOrDefault(p => GetAmmoKey(p) == cleanKey);
                if (prefab != null)
                {
                    weaponAmmoDict[weaponKey] = (prefab.initialTotalAmmo, weaponAmmoDict[weaponKey].ammoInClip);
                }
            }

            // После обновления словаря, обновляем данные для текущего оружия в руках.
            if (currentWeapon != null && !currentWeapon.isKnife)
            {
                string currentWeaponKey = (currentWeapon == _instantiatedPrimaryWeapon) ? currentWeapon.name : GetAmmoKey(currentWeapon);
                if(weaponAmmoDict.ContainsKey(currentWeaponKey))
                {
                    currentWeaponData.ammoTotal = currentWeapon.initialTotalAmmo;
                    currentWeapon.ammoTotal = currentWeapon.initialTotalAmmo;
                    currentWeaponData.ammoInClip = currentWeapon.clipSize;
                    currentWeapon.ammoInClip = currentWeapon.clipSize;
                }
            }
            
            RefreshAmmoText();
        }
    }

    public void RefreshAmmoText()
    {
        // Проверяем, находимся ли мы в режиме гранат
        if (grenadeSystem != null && grenadeSystem.IsGrenadeMode)
        {
            // В режиме гранат показываем количество гранат
            if (totalAmmoText != null)
            {
                totalAmmoText.text = $"Осколочные: {grenadeSystem.CurrentFragGrenades} | Дымовые: {grenadeSystem.CurrentSmokeGrenades}";
            }
            if (clipAmmoText != null)
            {
                clipAmmoText.text = "";
            }
            return;
        }

        // Обычный режим - показываем патроны
        if (currentWeaponData != null && currentWeaponData.isKnife)
        {
            if (totalAmmoText != null) totalAmmoText.text = "";
            if (clipAmmoText != null) clipAmmoText.text = "";
            return; 
        }

        if (totalAmmoText != null)
        {
            totalAmmoText.text = $"Количество патронов в запасе: {currentWeaponData?.ammoTotal ?? 0}";
        }
        if (clipAmmoText != null)
        {
            clipAmmoText.text = $"Патроны в магазине: {currentWeaponData?.ammoInClip ?? 0}";
        }
    }

    public void Aim()
    {
        // Логика прицеливания
    }

    public void Melee()
    {
        Debug.Log("[PlayerShooting] Попытка удара ножом/прикладом!");

        float meleeRange = 2f;
        float meleeDamage = 50f;
        RaycastHit hit;
        if (mainCamera != null && Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out hit, meleeRange))
        {
            var enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            var enemyAI = hit.collider.GetComponent<EnemyAI>();
            if (enemyHealth != null && enemyAI != null)
            {
                if (enemyAI.IsVulnerableToStealthKill())
                {
                    enemyHealth.KillSilent();
                    Debug.Log("Ваншот ножом! Враг был не агрессивен.");
                }
                else
                {
                    enemyHealth.TakeDamage(meleeDamage);
                    Debug.Log("Нанесён обычный урон ножом.");
                }
            }
            else
            {
                Debug.Log("Удар ножом не попал во врага.");
            }
        }
        else
        {
            Debug.Log("Удар ножом не попал ни во что.");
        }
    }

    public void ThrowGrenade()
    {
        // Проверяем, есть ли система гранат
        if (grenadeSystem != null)
        {
            // Переключаемся в режим гранат
            grenadeSystem.ToggleGrenadeMode();
        }
        else
        {
            // Fallback для старой системы
            Debug.Log("Система гранат не найдена. Используйте GrenadeSystem компонент.");
        }
    }

    private IEnumerator BurstFireCoroutine()
    {
        if (CanShoot())
        {
            isBurstFiring = true;
            for (int i = 0; i < 3; i++)
            {
                if (currentWeaponData.ammoInClip > 0)
                {
                    Shoot();
                    yield return new WaitForSeconds(0.1f);
                }
                else break;
            }
            isBurstFiring = false;
        }
    }

    // Новый метод для применения сохраненных данных оружия
    public void ApplySaveData(SaveData data)
    {
        if (data == null) return;

        // Восстанавливаем словарь патронов ПЕРЕД инстанцированием оружия
        SetWeaponAmmoDict(data.weaponAmmoList);

        // Удаляем старые экземпляры оружия из холдеров
        if (_instantiatedPrimaryWeapon != null)
            Destroy(_instantiatedPrimaryWeapon.gameObject);
        if (_instantiatedSecondaryWeapon != null)
            Destroy(_instantiatedSecondaryWeapon.gameObject);
        if (_instantiatedKnife != null)
            Destroy(_instantiatedKnife.gameObject);
        _instantiatedPrimaryWeapon = null;
        _instantiatedSecondaryWeapon = null;
        _instantiatedKnife = null;

        // Удаляем старые экземпляры выброшенного оружия с совпадающим uniqueID
        var idsFromSave = data.sceneWeaponAmmoList.Select(x => x.uniqueID).ToHashSet();
        foreach (var wi in FindObjectsOfType<WeaponInteractable>())
        {
            if (wi.transform.parent != null && wi.transform.parent.name.Contains("Holder"))
                continue; // Не трогаем оружие в руках игрока
            if (idsFromSave.Contains(wi.uniqueID))
                Destroy(wi.gameObject);
        }

        // ВОССТАНАВЛИВАЕМ ВЫБРОШЕННОЕ ОРУЖИЕ НА СЦЕНЕ
        // Создаём объекты оружия по sceneWeaponAmmoList, если их uniqueID не совпадает с оружием у игрока
        string secondaryID = data.secondaryWeaponUniqueID;
        foreach (var weaponSave in data.sceneWeaponAmmoList)
        {
            if (!string.IsNullOrEmpty(secondaryID) && weaponSave.uniqueID == secondaryID)
                continue; // Не спавним то, что у игрока в руках
            Debug.Log($"[LOAD] Восстанавливаю оружие: uniqueID={weaponSave.uniqueID}, type={weaponSave.weaponTypeName}, pos={weaponSave.position}");
            // Ищем префаб по weaponTypeName из сохранения
            var prefab = weaponPrefabs.Find(p => 
                p.name.Replace("(Clone)", "").Trim() == weaponSave.weaponTypeName ||
                (p.weaponPrefab != null && p.weaponPrefab.name.Replace("(Clone)", "").Trim() == weaponSave.weaponTypeName)
            );
            if (prefab == null) continue;
            var weaponObj = Instantiate(prefab, weaponSave.position, weaponSave.rotation);
            weaponObj.name = prefab.name.Replace("(Clone)", "").Trim();
            var interactable = weaponObj.GetComponent<WeaponInteractable>();
            if (interactable != null)
            {
                interactable.uniqueID = weaponSave.uniqueID;
                interactable.weaponParameters.ammoInClip = weaponSave.ammoInClip;
                interactable.weaponParameters.ammoTotal = weaponSave.ammoTotal;
            }
        }

        // 1. Инстанцируем основное оружие, если оно должно быть
        if (primaryWeaponPrefab != null)
        {
            _instantiatedPrimaryWeapon = InstantiateWeapon(primaryWeaponPrefab, primaryWeaponHolder);
            // Загружаем патроны для основного оружия с защитой от нулей
            if (_instantiatedPrimaryWeapon != null)
            {
                _instantiatedPrimaryWeapon.ammoInClip = data.primaryWeaponAmmoInClip > 0 ? data.primaryWeaponAmmoInClip : _instantiatedPrimaryWeapon.clipSize;
                _instantiatedPrimaryWeapon.ammoTotal = data.primaryWeaponTotalAmmo > 0 ? data.primaryWeaponTotalAmmo : _instantiatedPrimaryWeapon.initialTotalAmmo;
            }
        }

        // 2. Инстанцируем нож, если он должен быть
        if (knifePrefab != null)
        {
            _instantiatedKnife = InstantiateWeapon(knifePrefab, knifeHolder);
            // Загружаем патроны для ножа
            if (_instantiatedKnife != null)
            {
                _instantiatedKnife.ammoInClip = data.knifeAmmoInClip;
                _instantiatedKnife.ammoTotal = data.knifeTotalAmmo;
            }
        }

        // 3. Ищем второе оружие по имени в списке weaponPrefabs
        if (!string.IsNullOrEmpty(data.secondaryWeaponId))
        {
            var prefabToLoad = weaponPrefabs.Find(p => 
                p.name.Replace("(Clone)", "").Trim() == data.secondaryWeaponId ||
                (p.weaponPrefab != null && p.weaponPrefab.name.Replace("(Clone)", "").Trim() == data.secondaryWeaponId)
            );

            if (prefabToLoad != null)
            {
                Transform holder = GetWeaponHolder(prefabToLoad);
                if (holder != null)
                {
                    _instantiatedSecondaryWeapon = InstantiateWeapon(prefabToLoad, holder);
                    if (_instantiatedSecondaryWeapon != null)
                    {
                        // ВАЖНО: Восстанавливаем ссылку на префаб
                        _instantiatedSecondaryWeapon.weaponPrefab = prefabToLoad.weaponPrefab ?? prefabToLoad;
                        
                        // Загружаем патроны и uniqueID
                        _instantiatedSecondaryWeapon.ammoInClip = data.secondaryWeaponAmmoInClip;
                        _instantiatedSecondaryWeapon.ammoTotal = data.secondaryWeaponTotalAmmo;
                        var interactable = _instantiatedSecondaryWeapon.GetComponent<WeaponInteractable>() ?? _instantiatedSecondaryWeapon.gameObject.AddComponent<WeaponInteractable>();
                        interactable.uniqueID = data.secondaryWeaponUniqueID;
                    }
                }
            }
        }

        // 4. Переключаемся на сохраненное активное оружие
        switch (data.activeWeaponId)
        {
            case "primary":
                if (_instantiatedPrimaryWeapon != null) SwitchToWeapon(_instantiatedPrimaryWeapon);
                else if (_instantiatedKnife != null) SwitchToWeapon(_instantiatedKnife); // Fallback
                else currentWeapon = null;
                break;
            case "secondary":
                if (_instantiatedSecondaryWeapon != null) SwitchToWeapon(_instantiatedSecondaryWeapon);
                else if (_instantiatedPrimaryWeapon != null) SwitchToWeapon(_instantiatedPrimaryWeapon); // Fallback
                else if (_instantiatedKnife != null) SwitchToWeapon(_instantiatedKnife); // Fallback
                else currentWeapon = null;
                break;
            case "knife":
                if (_instantiatedKnife != null) SwitchToWeapon(_instantiatedKnife);
                else if (_instantiatedPrimaryWeapon != null) SwitchToWeapon(_instantiatedPrimaryWeapon); // Fallback
                else currentWeapon = null;
                break;
            default:
                if (_instantiatedPrimaryWeapon != null) SwitchToWeapon(_instantiatedPrimaryWeapon); // Fallback
                else if (_instantiatedKnife != null) SwitchToWeapon(_instantiatedKnife); // Fallback
                else currentWeapon = null;
                break;
        }

        // 5. Загружаем режим огня (его надо применить после активации оружия)
        fireMode = (FireMode)data.fireMode;
        // Обновляем UI после загрузки
        RefreshAmmoText();
        UpdateFireModeDisplay(); 
    }

    public void ResetWeaponAmmoDict()
    {
        weaponAmmoDict.Clear();
    }

    private string GetAmmoKey(WeaponParametrs weapon)
    {
        if (weapon == null) return "";
        if (weapon.weaponPrefab != null)
            return weapon.weaponPrefab.name.Replace("(Clone)", "").Trim();
        return weapon.name.Replace("(Clone)", "").Trim();
    }

    // Метод для получения точки крепления по типу оружия
    private Transform GetWeaponHolder(WeaponParametrs weaponPrefab)
    {
        if (weaponPrefab == null) 
        {
            Debug.LogWarning("[GetWeaponHolder] weaponPrefab == null, возвращаю secondaryWeaponHolder");
            return secondaryWeaponHolder;
        }

        string weaponName = weaponPrefab.name.Replace("(Clone)", "").Trim();
        Debug.Log($"[GetWeaponHolder] Ищем точку крепления для оружия: '{weaponName}'");
        
        // Ищем в новой системе множественных точек крепления по имени префаба
        var mapping = secondaryWeaponHolders.Find(m => 
            m.weaponPrefab != null && 
            m.weaponPrefab.name.Replace("(Clone)", "").Trim() == weaponName);
        
        if (mapping != null && mapping.weaponHolder != null)
        {
            Debug.Log($"[GetWeaponHolder] Найдена специальная точка крепления: '{mapping.weaponHolder.name}' для '{weaponName}'");
            return mapping.weaponHolder;
        }
        
        // Fallback на старую систему
        if (secondaryWeaponHolder != null)
        {
            Debug.Log($"[GetWeaponHolder] Используется fallback точка крепления: '{secondaryWeaponHolder.name}' для '{weaponName}'");
            return secondaryWeaponHolder;
        }
        
        // Если ничего не найдено, возвращаем null
        Debug.LogError($"[GetWeaponHolder] Не найдена точка крепления для оружия типа: {weaponName}");
        return null;
    }

    // Метод для валидации настроек точек крепления
    private void ValidateWeaponHolders()
    {
        Debug.Log($"[ValidateWeaponHolders] Проверяю настройки точек крепления...");
        
        // Проверяем новые точки крепления на ошибки
        if (secondaryWeaponHolders != null && secondaryWeaponHolders.Count > 0)
        {
            Debug.Log($"[ValidateWeaponHolders] Найдено {secondaryWeaponHolders.Count} настроенных точек крепления:");
            foreach (var mapping in secondaryWeaponHolders)
            {
                if (mapping.weaponPrefab == null)
                {
                    Debug.LogError($"[ValidateWeaponHolders] Префаб оружия не указан для точки крепления {(mapping.weaponHolder != null ? mapping.weaponHolder.name : "неизвестно")}!");
                }
                else if (mapping.weaponHolder == null)
                {
                    Debug.LogError($"[ValidateWeaponHolders] Точка крепления для '{mapping.weaponPrefab.name}' не настроена!");
                }
                else
                {
                    Debug.Log($"[ValidateWeaponHolders] ✓ {mapping.weaponPrefab.name} -> {mapping.weaponHolder.name}");
                }
            }
        }
        else
        {
            Debug.Log("[ValidateWeaponHolders] Специальные точки крепления не настроены. Будет использоваться secondaryWeaponHolder.");
        }
    }

    public void SetAiming(bool aiming)
    {
        if (currentWeapon != null && currentWeapon.isKnife) return;
        isAiming = aiming;
    }

    private void HandleAiming()
    {
        if (currentWeapon == null || currentWeaponData == null || currentWeapon.isKnife) return;

        // Определяем целевые позицию, вращение и FOV
        Vector3 targetPosition = isAiming ? currentWeaponData.aimPosition : Vector3.zero;
        Quaternion targetRotation = isAiming ? Quaternion.Euler(currentWeaponData.aimRotation) : Quaternion.identity;
        float targetFOV = isAiming ? currentWeaponData.aimFOV : defaultFOV;

        // Плавное перемещение оружия
        currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, targetPosition, Time.deltaTime * currentWeaponData.aimSpeed);
        currentWeapon.transform.localRotation = Quaternion.Slerp(currentWeapon.transform.localRotation, targetRotation, Time.deltaTime * currentWeaponData.aimSpeed);

        // Плавное изменение FOV камеры
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * currentWeaponData.aimSpeed);
        }
    }

    public void SaveCurrentWeaponAsPrevious()
    {
        previousWeaponBeforeGrenadeMode = currentWeapon;
    }

    public void RestorePreviousWeapon()
    {
        if (previousWeaponBeforeGrenadeMode != null)
        {
            SwitchToWeapon(previousWeaponBeforeGrenadeMode);
            previousWeaponBeforeGrenadeMode = null;
        }
    }

    private void HandleShotAlert(WeaponParametrs weapon)
{
    if (weapon.alertsEnemies)
    {
        float noiseRadius = weapon.hasSilencer ? weapon.shotSoundRadius * weapon.silencerMultiplier : weapon.shotSoundRadius;
        Debug.Log($"[Noise] Создан шум от ВЫСТРЕЛА с радиусом {noiseRadius}");
        NoiseManager.EmitNoise(transform.position, noiseRadius);
    }
}

    void ProcessWeaponSwitch()
    {
        // ... existing code ...
    }
}