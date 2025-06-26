using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Добавляем для работы с TextMeshPro
using UnityEngine.Rendering; // Добавляем для управления тенями

public class GrenadeSystem : MonoBehaviour
{
    [Header("Настройки гранат")]
    public int maxFragGrenades = 3;
    public int maxSmokeGrenades = 2;
    
    [Header("Префабы гранат")]
    public GameObject fragGrenadePrefab;
    public GameObject smokeGrenadePrefab;
    
    [Header("Настройки броска")]
    public float throwForce = 15f;
    public float throwAngle = 35f; // Угол броска в градусах
    public float maxThrowDistance = 20f;
    public float throwCooldown = 1f; // Задержка между бросками
    public LayerMask trajectoryLayerMask = -1;
    
    [Header("Точки крепления")]
    public Transform grenadeThrowPoint;
    public Transform grenadeDisplayPoint;
    
    [Header("Траектория")]
    public LineRenderer trajectoryLine; // Теперь это поле нужно назначать вручную в инспекторе!
    public int trajectoryPoints = 150; // Увеличил количество точек для более длинной траектории
    public float trajectoryTimeStep = 0.05f;

    [Header("UI для сообщений")]
    public TMP_Text messagesText;
    
    [Header("UI для имени и количества гранат")]
    public TMP_Text grenadeNameText;
    public TMP_Text grenadeCountText;
    public GameObject grenadeUIPanel; // Панель, которая включает/выключает UI гранат
    
    [Header("Стартовое количество гранат")]
    public int startFragGrenades = 1;
    public int startSmokeGrenades = 0;
    
    // Приватные переменные
    public bool isGrenadeMode = false;
    private bool canThrow = true;
    public GrenadeType currentGrenadeType = GrenadeType.Frag;
    private int currentFragGrenades;
    private int currentSmokeGrenades;
    private GameObject currentGrenadeDisplay;
    private Camera playerCamera;
    private PlayerShooting playerShooting;
    private Coroutine messageCoroutine;
    private GrenadeType? lastGrenadeType = null;
    
    public enum GrenadeType { Frag, Smoke }
    
    void Start()
    {
        playerCamera = Camera.main;
        playerShooting = GetComponent<PlayerShooting>();
        
        // Убрали автоматический поиск траектории. Убедитесь, что она назначена в инспекторе!
        if (trajectoryLine != null)
        {
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.gameObject.SetActive(false); // Деактивируем объект траектории при старте

            // Отключаем тени для траектории
            trajectoryLine.shadowCastingMode = ShadowCastingMode.Off;
            trajectoryLine.receiveShadows = false;
        }
        else
        {
            Debug.LogError("GrenadeSystem: LineRenderer для траектории не назначен в инспекторе!");
        }
        
        // Если нет сохранения, инициализируем стартовое количество гранат
        if (GameManager.CurrentSaveData == null)
        {
            currentFragGrenades = Mathf.Clamp(startFragGrenades, 0, maxFragGrenades);
            currentSmokeGrenades = Mathf.Clamp(startSmokeGrenades, 0, maxSmokeGrenades);
        }

        // Автоматический поиск UI для сообщений
        if (messagesText == null)
        {
            GameObject obj = GameObject.Find("Messegs"); // Имя, которое вы указали
            if (obj != null)
            {
                messagesText = obj.GetComponent<TMP_Text>();
                if (messagesText != null)
                    messagesText.gameObject.SetActive(false); // Скрываем при старте
            }
        }

        // При старте UI гранат должен быть выключен
        if (grenadeUIPanel != null)
        {
            grenadeUIPanel.SetActive(false);
        }

        // Автоматический поиск UI для гранат через PlayerShooting
        if (playerShooting != null)
        {
            if (playerShooting.totalAmmoText == null)
            {
                GameObject obj = GameObject.Find("TotalAmmo_Text");
                if (obj != null) playerShooting.totalAmmoText = obj.GetComponent<TMPro.TMP_Text>();
                else Debug.LogError("GrenadeSystem: UI элемент 'TotalAmmo_Text' не найден на сцене!");
            }
            if (playerShooting.clipAmmoText == null)
            {
                GameObject obj = GameObject.Find("ClipAmmo_Text");
                if (obj != null) playerShooting.clipAmmoText = obj.GetComponent<TMPro.TMP_Text>();
                else Debug.LogError("GrenadeSystem: UI элемент 'ClipAmmo_Text' не найден на сцене!");
            }
            if (playerShooting.fireModeText == null)
            {
                GameObject obj = GameObject.Find("FireMode_Text");
                if (obj != null) playerShooting.fireModeText = obj.GetComponent<TMPro.TMP_Text>();
                else Debug.LogError("GrenadeSystem: UI элемент 'FireMode_Text' не найден на сцене!");
            }
        }
    }
    
    void Update()
    {
        if (PauseManager.IsPaused) return;
        
        if (isGrenadeMode)
        {
            HandleGrenadeMode();
        }
    }
    
    public void ToggleGrenadeMode()
    {
        // Если пытаемся включить режим гранат
        if (!isGrenadeMode)
        {
            // Проверяем, есть ли вообще гранаты
            if (currentFragGrenades <= 0 && currentSmokeGrenades <= 0)
            {
                ShowMessage("Гранат нет", 2f);
                return;
            }
            isGrenadeMode = true;
            EnterGrenadeMode();
        }
        else // Если пытаемся выключить
        {
            isGrenadeMode = false;
            ExitGrenadeMode();
        }
    }
    
    public void SwitchGrenadeType(GrenadeType type)
    {
        if (currentGrenadeType == type && GetCurrentGrenadeCount() > 0) return; // Не переключаемся, если тип уже выбран и гранаты есть

        if (GetGrenadeCount(type) > 0)
        {
            // Успешное переключение
            currentGrenadeType = type;
            UpdateGrenadeDisplay();
            UpdateGrenadeUI();
        }
        else
        {
            // Переключиться нельзя, показываем сообщение
            string grenadeName = GetGrenadeTypeName(type);
            ShowMessage($"{grenadeName} гранат нет", 2f);
        }
    }
    
    private void EnterGrenadeMode()
    {
        if (playerShooting != null)
        {
            playerShooting.SaveCurrentWeaponAsPrevious(); // Сохраняем текущее оружие
            if (playerShooting.currentWeapon != null)
            {
                playerShooting.currentWeapon.gameObject.SetActive(false); // Скрываем оружие
            }
            playerShooting.enabled = false;
        }

        // --- Выбор типа гранаты при входе ---
        if (lastGrenadeType.HasValue)
        {
            currentGrenadeType = lastGrenadeType.Value;
        }
        else
        {
            currentGrenadeType = GrenadeType.Frag;
        }
        // ---

        // --- НОВАЯ ЛОГИКА: Проверка гранат при входе в режим ---
        if (GetGrenadeCount(currentGrenadeType) <= 0)
        {
            // У текущего типа нет гранат, пытаемся переключиться на другой
            GrenadeType otherType = (currentGrenadeType == GrenadeType.Frag) ? GrenadeType.Smoke : GrenadeType.Frag;
            if (GetGrenadeCount(otherType) > 0)
            {
                currentGrenadeType = otherType;
            }
            // Если у другого типа тоже нет, ничего не делаем, остаемся на текущем (у которого 0)
        }
        // ----------------------------------------------------
        
        UpdateGrenadeDisplay();
        UpdateGrenadeUI();
        Debug.Log("Включен режим гранат.");
    }
    
    private void ExitGrenadeMode()
    {
        // Если целимся, отменяем бросок
        if (trajectoryLine != null && trajectoryLine.gameObject.activeSelf)
        {
            trajectoryLine.gameObject.SetActive(false);
        }

        if (playerShooting != null)
        {
            playerShooting.enabled = true;
            playerShooting.RestorePreviousWeapon(); // Возвращаем оружие, которое было до режима гранат
            playerShooting.RefreshAmmoText(); // Восстанавливаем UI оружия
            playerShooting.UpdateFireModeDisplay();
            playerShooting.ActivatePendingWeapon();
        }
        
        if (currentGrenadeDisplay != null)
        {
            Destroy(currentGrenadeDisplay);
        }
        
        Debug.Log("Выключен режим гранат");
        lastGrenadeType = currentGrenadeType;
    }
    
    private void HandleGrenadeMode()
    {
        // Прицеливание и показ траектории
        if (Input.GetMouseButtonDown(0))
        {
            if (GetCurrentGrenadeCount() > 0 && trajectoryLine != null)
            {
                trajectoryLine.gameObject.SetActive(true);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (trajectoryLine != null && trajectoryLine.gameObject.activeSelf)
            {
                UpdateTrajectory();
            }
        }
        
        // Бросок гранаты
        if (Input.GetMouseButtonUp(0))
        {
            if (trajectoryLine != null && trajectoryLine.gameObject.activeSelf)
            {
                trajectoryLine.gameObject.SetActive(false);
                ThrowGrenade();
            }
        }

        // Переключение типа гранат
        if (Input.GetKeyDown(KeyCode.G)) // Используем G для переключения
        {
            GrenadeType targetType = (currentGrenadeType == GrenadeType.Frag) ? GrenadeType.Smoke : GrenadeType.Frag;
            SwitchGrenadeType(targetType);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchGrenadeType(GrenadeType.Frag);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchGrenadeType(GrenadeType.Smoke);
        }
    }

    private void ThrowGrenade()
    {
        if (!canThrow) return; // Проверяем, прошла ли задержка
        if (GetCurrentGrenadeCount() <= 0) return;
        
        GameObject grenadePrefab = GetCurrentGrenadePrefab();
        if (grenadePrefab != null && grenadeThrowPoint != null)
        {
            GameObject grenade = Instantiate(grenadePrefab, grenadeThrowPoint.position, grenadeThrowPoint.rotation);
            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            
            if (grenadeRb != null)
            {
                // Применяем угол к направлению броска
                Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, playerCamera.transform.right) * playerCamera.transform.forward;
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
            }
            
            canThrow = false;
            StartCoroutine(ResetThrowCooldown());

            string thrownGrenadeName = GetGrenadeTypeName(currentGrenadeType);
            DecreaseGrenadeCount();

            // Новая проверка: если закончились все типы гранат, выходим из режима
            if (currentFragGrenades <= 0 && currentSmokeGrenades <= 0)
            {
                Debug.Log($"Брошена {thrownGrenadeName} граната. Все гранаты закончились.");
                ToggleGrenadeMode(); // Выключаем режим гранат
                return; // Выходим, чтобы не выполнять остальной код
            }

            // Проверяем, закончились ли гранаты текущего типа, и автоматически переключаемся
            if (GetCurrentGrenadeCount() <= 0)
            {
                GrenadeType otherType = (currentGrenadeType == GrenadeType.Frag) ? GrenadeType.Smoke : GrenadeType.Frag;
                if (GetGrenadeCount(otherType) > 0)
                {
                    currentGrenadeType = otherType;
                }
            }
            
            UpdateGrenadeUI();
            UpdateGrenadeDisplay(); // Обновляем модель в руке
            
            Debug.Log($"Брошена {thrownGrenadeName} граната");
        }
    }
    
    private void UpdateTrajectory()
    {
        // Применяем угол к направлению для расчета траектории
        Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, playerCamera.transform.right) * playerCamera.transform.forward;
        Vector3 startPosition = grenadeThrowPoint.position;
        Vector3 startVelocity = throwDirection * throwForce;

        List<Vector3> points = new List<Vector3>();
        Vector3 previousPoint = startPosition;
        points.Add(previousPoint);

        for (int i = 1; i < trajectoryPoints; i++)
        {
            float t = i * trajectoryTimeStep;
            Vector3 currentPoint = startPosition + startVelocity * t + 0.5f * Physics.gravity * t * t;
            
            if (Physics.Linecast(previousPoint, currentPoint, out RaycastHit hit, trajectoryLayerMask))
            {
                points.Add(hit.point);
                break;
            }
            
            points.Add(currentPoint);
            previousPoint = currentPoint;
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
    }
    
    private void UpdateGrenadeDisplay()
    {
        if (currentGrenadeDisplay != null)
        {
            Destroy(currentGrenadeDisplay);
        }

        if (GetCurrentGrenadeCount() <= 0) return; // Не показывать модель, если гранат нет
        
        GameObject prefab = GetCurrentGrenadePrefab();
        if (prefab != null && grenadeDisplayPoint != null)
        {
            currentGrenadeDisplay = Instantiate(prefab, grenadeDisplayPoint);
            currentGrenadeDisplay.transform.localPosition = Vector3.zero;
            currentGrenadeDisplay.transform.localRotation = Quaternion.identity;

            if (currentGrenadeDisplay.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
            if (currentGrenadeDisplay.TryGetComponent<Collider>(out var col)) col.enabled = false;

            // --- ИСПРАВЛЕНИЕ БАГА ---
            // Отключаем логику на гранате в руке, чтобы она не взрывалась/исчезала сама по себе
            var smokeScript = currentGrenadeDisplay.GetComponent<SmokeGrenade>();
            if (smokeScript != null) smokeScript.enabled = false;

            var fragScript = currentGrenadeDisplay.GetComponent<FragGrenade>();
            if (fragScript != null) fragScript.enabled = false;
            // -------------------------
        }
    }
    
    private GameObject GetCurrentGrenadePrefab()
    {
        return currentGrenadeType == GrenadeType.Frag ? fragGrenadePrefab : smokeGrenadePrefab;
    }
    
    private int GetCurrentGrenadeCount()
    {
        return currentGrenadeType == GrenadeType.Frag ? currentFragGrenades : currentSmokeGrenades;
    }

    private int GetGrenadeCount(GrenadeType type)
    {
        return type == GrenadeType.Frag ? currentFragGrenades : currentSmokeGrenades;
    }
    
    private void DecreaseGrenadeCount()
    {
        if (currentGrenadeType == GrenadeType.Frag)
            currentFragGrenades = Mathf.Max(0, currentFragGrenades - 1);
        else
            currentSmokeGrenades = Mathf.Max(0, currentSmokeGrenades - 1);
        
        // SaveGrenadeCounts(); // Больше не используем PlayerPrefs
        UpdateGrenadeUI();
    }
    
    public void UpdateGrenadeUI()
    {
        if (playerShooting != null)
    {
        if (playerShooting.totalAmmoText != null)
            playerShooting.totalAmmoText.text = "Осколочные: " + currentFragGrenades;
        if (playerShooting.clipAmmoText != null)
            playerShooting.clipAmmoText.text = "Дымовые: " + currentSmokeGrenades;
        if (playerShooting.fireModeText != null)
            playerShooting.fireModeText.text = GetGrenadeTypeName(currentGrenadeType);
        }
    }
    
    private string GetGrenadeTypeName(GrenadeType type)
    {
        switch (type)
        {
            case GrenadeType.Frag:
                return "Осколочная";
            case GrenadeType.Smoke:
                return "Дымовая";
            default:
                return "Неизвестный тип";
        }
    }
    
    public void AddFragGrenades(int count)
    {
        currentFragGrenades = Mathf.Clamp(currentFragGrenades + count, 0, maxFragGrenades);
        if (isGrenadeMode) UpdateGrenadeUI();
    }
    
    public void AddSmokeGrenades(int count)
    {
        currentSmokeGrenades = Mathf.Clamp(currentSmokeGrenades + count, 0, maxSmokeGrenades);
        if (isGrenadeMode) UpdateGrenadeUI();
    }

    // Удаляем систему сохранения через PlayerPrefs
    // private void SaveGrenadeCounts()
    // {
    //     PlayerPrefs.SetInt("FragGrenades", currentFragGrenades);
    //     PlayerPrefs.SetInt("SmokeGrenades", currentSmokeGrenades);
    // }
    
    // private void LoadGrenadeCounts()
    // {
    //     currentFragGrenades = PlayerPrefs.GetInt("FragGrenades", maxFragGrenades);
    //     currentSmokeGrenades = PlayerPrefs.GetInt("SmokeGrenades", maxSmokeGrenades);
    // }

    private void ShowMessage(string message, float duration)
    {
        if (messagesText == null) return;

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message, duration));
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        messagesText.text = message;
        messagesText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messagesText.gameObject.SetActive(false);
    }

    private IEnumerator ResetThrowCooldown()
    {
        yield return new WaitForSeconds(throwCooldown);
        canThrow = true;
    }
    
    // Новый метод для приема данных из основной системы сохранений
    public void ApplySaveData(int fragCount, int smokeCount, int grenadeType)
    {
        currentFragGrenades = fragCount;
        currentSmokeGrenades = smokeCount;
        currentGrenadeType = (GrenadeType)grenadeType;
        
        // Обновляем UI, если игрок загрузился сразу в режиме гранат (маловероятно, но для надежности)
        if (isGrenadeMode)
        {
            UpdateGrenadeUI();
            UpdateGrenadeDisplay();
        }
    }

    // Свойства для доступа из других скриптов
    public bool IsGrenadeMode => isGrenadeMode;
    public GrenadeType CurrentGrenadeType => currentGrenadeType;
    public int CurrentFragGrenades => currentFragGrenades;
    public int CurrentSmokeGrenades => currentSmokeGrenades;

    public void RestoreAllGrenadesToMax()
    {
        currentFragGrenades = maxFragGrenades;
        currentSmokeGrenades = maxSmokeGrenades;
        if (isGrenadeMode)
        {
            UpdateGrenadeUI();
        }
        Debug.Log($"Гранаты пополнены: Осколочные={currentFragGrenades}, Дымовые={currentSmokeGrenades}");
    }

    public void AddGrenades(GrenadeType type, int amount)
    {
        if (type == GrenadeType.Frag)
        {
            currentFragGrenades = Mathf.Clamp(currentFragGrenades + amount, 0, maxFragGrenades);
            if (isGrenadeMode) UpdateGrenadeUI();
        }
        else if (type == GrenadeType.Smoke)
        {
            currentSmokeGrenades = Mathf.Clamp(currentSmokeGrenades + amount, 0, maxSmokeGrenades);
            if (isGrenadeMode) UpdateGrenadeUI();
        }
    }

    /// <summary>
    /// Переключает на указанный тип гранаты.
    /// </summary>
    /// <param name="type">Тип гранаты для переключения.</param>
    public void SwitchToGrenadeType(GrenadeType type)
    {
        if (!isGrenadeMode || currentGrenadeType == type) return;
        currentGrenadeType = type;
        UpdateGrenadeDisplay();
        UpdateGrenadeUI();
        Debug.Log("Сменен тип гранаты на: " + currentGrenadeType);
    }

    public void SwitchGrenadeType()
    {
        if (!isGrenadeMode) return;
        currentGrenadeType = (GrenadeType)(((int)currentGrenadeType + 1) % System.Enum.GetValues(typeof(GrenadeType)).Length);
        UpdateGrenadeDisplay();
        UpdateGrenadeUI();
    }

    public void ThrowCurrentGrenade()
    {
        GameObject currentGrenadePrefab = GetCurrentGrenadePrefab();
        if (currentGrenadePrefab != null && grenadeThrowPoint != null)
        {
            GameObject grenade = Instantiate(currentGrenadePrefab, grenadeThrowPoint.position, grenadeThrowPoint.rotation);
            Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
            
            if (grenadeRb != null)
            {
                // Применяем угол к направлению броска
                Vector3 throwDirection = Quaternion.AngleAxis(-throwAngle, playerCamera.transform.right) * playerCamera.transform.forward;
                grenadeRb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
            }
            
            StartCoroutine(ThrowAfterDelay(currentGrenadePrefab, 0.1f));

            // Обновляем UI после броска
            UpdateGrenadeUI();
        }
    }

    private IEnumerator ThrowAfterDelay(GameObject grenadePrefab, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GetCurrentGrenadeCount() <= 0)
        {
            Debug.Log("Граната закончилась, невозможно выполнить бросок");
            yield break;
        }
        ThrowGrenade();
        UpdateGrenadeUI();
    }

    public void SwitchToGrenadeMode()
    {
        isGrenadeMode = true;
        UpdateGrenadeUI();
        UpdateGrenadeDisplay();
        Debug.Log("Включен режим гранат.");
    }
}