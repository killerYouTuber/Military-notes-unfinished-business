using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq; // Добавляем для использования LINQ
using System; // Для использования DateTime

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Изменяем свойство для добавления логирования
    private static SaveData _currentSaveData;
    public static SaveData CurrentSaveData
    {
        get { return _currentSaveData; }
        private set
        {
            _currentSaveData = value;
        }
    }
    private GameObject playerInstance; // Добавляем поле для хранения ссылки на игрока

    [Header("Настройки игры")]
    public GameObject playerPrefab;
    public string gameSceneName = "Level 1";
    public string menuSceneName = "menu";
    public string loadingSceneName = "briv1";

    [Header("Настройки сохранения")]
    private const string SAVE_KEY = "SaveData";
    private const string CHECKPOINT_KEY = "CheckpointData";
    private const string SETTINGS_KEY = "GameSettings";

    [Header("Ссылки на менеджеры")]
    public GameOverManager gameOverManager; // Ссылка на менеджер "Game Over"

    public static string SceneToLoadAfterBriv1 { get; set; }
    private bool ShouldStartPaused { get; set; }

    // Новое поле, чтобы отслеживать, новая ли это игра
    private bool isNewGame = false;
    public bool IsLoading { get; private set; } // Флаг для отслеживания состояния загрузки

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Страховка: если мы запускаем игру прямо из редактора в игровой сцене,
            // а данных сохранения нет, создадим их, чтобы избежать ошибок.
            if (CurrentSaveData == null && SceneManager.GetActiveScene().name == gameSceneName)
            {
                // UnityEngine.Debug.LogWarning("[GameManager] CurrentSaveData не найден. Создаю новый экземпляр для текущей сессии.");
                CurrentSaveData = new SaveData();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FindGameOverManager()
    {
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
            if (gameOverManager == null)
            {
                // UnityEngine.Debug.LogWarning("[GameManager] GameOverManager не найден на сцене.");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == menuSceneName)
        {
            ResetGameState();
            return;
        }

        // Защита: выполняем логику только на основной игровой сцене
        if (scene.name != gameSceneName)
        {
            return;
        }

        IsLoading = true;
        Debug.Log("[GameManager] Началась загрузка сцены, IsLoading = true");

        // --- Логика для игровой сцены ---
        FindGameOverManager();

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        bool applySavedPosition = false;

        if (isNewGame)
        {
            // --- НОВАЯ ИГРА ---
            GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
            if (spawnPoint != null)
            {
                spawnPosition = spawnPoint.transform.position;
                spawnRotation = spawnPoint.transform.rotation;
                // UnityEngine.Debug.Log($"[GameManager] Новая игра. Игрок будет размещен на SpawnPoint: {spawnPosition}");
            }
            else
            {
                // UnityEngine.Debug.LogWarning("[GameManager] SpawnPoint не найден на сцене! Игрок спавнится на стандартной позиции.");
            }
        }
        else if (CurrentSaveData != null)
        {
            // --- ЗАГРУЗКА СОХРАНЕНИЯ ---
            spawnPosition = CurrentSaveData.playerPosition;
            spawnRotation = Quaternion.Euler(CurrentSaveData.playerRotationX, CurrentSaveData.playerRotationY, CurrentSaveData.playerRotationZ);
            applySavedPosition = true; // Флаг для применения сохраненной позиции
            StartCoroutine(ApplyInteractiveObjectStates());
            StartCoroutine(ApplyEnemyStates());
        }
        else
        {
            // UnityEngine.Debug.LogError("[GameManager] Критическая ошибка: Нет данных для загрузки и это не новая игра. Возврат в меню.");
            LoadMenuScene();
            return;
        }

        // --- ОБЩАЯ ЛОГИКА СОЗДАНИЯ И НАСТРОЙКИ ИГРОКА ---
        playerInstance = GameObject.FindGameObjectWithTag("Player");
        if (playerInstance == null)
        {
             playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
             playerInstance.name = playerPrefab.name;
        }
        else if (applySavedPosition)
        {
            playerInstance.transform.position = spawnPosition;
            playerInstance.transform.rotation = spawnRotation;
        }

        if (playerInstance != null && CurrentSaveData != null)
        {
            var playerController = playerInstance.GetComponent<PlayerController>();
                    if (playerController != null)
            {
                        playerController.ApplySaveData(CurrentSaveData);
            }
                    
                    Camera mainCamera = Camera.main;
            if (mainCamera != null && !isNewGame && CurrentSaveData.cameraRotationX != 0) 
                    {
                        mainCamera.transform.rotation = Quaternion.Euler(
                            CurrentSaveData.cameraRotationX,
                            CurrentSaveData.cameraRotationY,
                            CurrentSaveData.cameraRotationZ
                        );
                    }
        }
        
        InitializePlayerUI(playerInstance);
        
        // Сбрасываем флаг в самом конце, после всех операций
        if(isNewGame)
        {
            isNewGame = false;
        }

        StartCoroutine(FinishLoading());
    }

    private IEnumerator FinishLoading()
    {
        // Небольшая задержка, чтобы все скрипты успели выполнить свои методы Start()
        yield return new WaitForSeconds(0.2f);
        IsLoading = false;
        Debug.Log("[GameManager] Загрузка сцены завершена, IsLoading = false.");
    }

    public void StartNewGame()
    {
        ResetGameState(); // Сначала сбрасываем все старое
        isNewGame = true; // Потом устанавливаем флаг, что это новая игра
        CurrentSaveData = null;
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.DeleteKey(CHECKPOINT_KEY);
        LoadSceneWithLoadingScreen(gameSceneName);
    }

    public void SaveGame(string sceneName)
    {
        if (CurrentSaveData == null)
        {
            CurrentSaveData = new SaveData();
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.CollectAndSetSaveData(CurrentSaveData);
            }
        }
        
        CurrentSaveData.timestamp = DateTime.UtcNow.Ticks;
        string json = JsonUtility.ToJson(CurrentSaveData, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void SaveCheckpoint()
    {
        if (CurrentSaveData == null)
        {
            CurrentSaveData = new SaveData();
        }
        CollectDataForSave(); // Этот метод теперь просто заполняет CurrentSaveData
        
        string json = JsonUtility.ToJson(CurrentSaveData, true);
        PlayerPrefs.SetString(CHECKPOINT_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[GameManager] Чекпоинт сохранен. Данные: {json}");
    }

    private void CollectDataForSave()
    {
        if (CurrentSaveData == null) CurrentSaveData = new SaveData();

        // 1. Сохранение данных игрока
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.CollectAndSetSaveData(CurrentSaveData);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] Игрок не найден на сцене при попытке сохранения.");
            return;
        }

        // 2. Сохранение данных о врагах
        CurrentSaveData.enemies.Clear();
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        
        foreach (var enemyAI in allEnemies)
        {
            if (string.IsNullOrEmpty(enemyAI.uniqueID))
            {
                Debug.LogWarning($"[GameManager] Пропуск сохранения врага, так как у него нет uniqueID: {enemyAI.gameObject.name}");
                continue;
            }

            var enemyHealth = enemyAI.GetComponent<EnemyHealth>();
            var enemyShooting = enemyAI.GetComponent<EnemyShooting>();

            if (enemyHealth == null || enemyShooting == null)
            {
                 Debug.LogWarning($"[GameManager] Враг {enemyAI.gameObject.name} не имеет компонентов Health или Shooting. Пропускаю.");
                 continue;
            }

            EnemyData enemyData = new EnemyData
            {
                uniqueID = enemyAI.uniqueID,
                position = enemyAI.transform.position,
                rotation = enemyAI.transform.rotation,
                health = enemyHealth.health,
                isAggressive = enemyAI.isAggressive,
                aiState = (int)enemyAI.GetCurrentState(),
                ammoInClip = enemyShooting.weapon != null ? enemyShooting.weapon.ammoInClip : 0,
                totalAmmo = enemyShooting.weapon != null ? enemyShooting.weapon.ammoTotal : 0
            };
            CurrentSaveData.enemies.Add(enemyData);
        }
        Debug.Log($"[GameManager] Сохранено состояние {CurrentSaveData.enemies.Count} врагов.");
    }

    // Загружает последнее сохранение (обычное или чекпоинт)
    public void LoadLastSave()
    {
        string regularSaveJson = PlayerPrefs.GetString(SAVE_KEY, null);
        string checkpointJson = PlayerPrefs.GetString(CHECKPOINT_KEY, null);

        if (string.IsNullOrEmpty(regularSaveJson) && string.IsNullOrEmpty(checkpointJson))
        {
            // Debug.LogWarning("[GameManager] Нет сохранений для загрузки.");
            // Возможно, здесь стоит предложить начать новую игру или показать сообщение
            StartNewGame(); // Например, начинаем новую игру, если сохранений нет
            return;
        }

        SaveData regularData = null;
        SaveData checkpointData = null;

        if (!string.IsNullOrEmpty(regularSaveJson)) regularData = JsonUtility.FromJson<SaveData>(regularSaveJson);
        if (!string.IsNullOrEmpty(checkpointJson)) checkpointData = JsonUtility.FromJson<SaveData>(checkpointJson);
        
        // Определяем, какое сохранение новее
        bool useCheckpoint = false;
        if (regularData != null && checkpointData != null)
        {
            useCheckpoint = checkpointData.timestamp > regularData.timestamp;
        }
        else if (checkpointData != null)
        {
            useCheckpoint = true;
        }

        if (useCheckpoint)
        {
            // Debug.Log("[GameManager] Загружается более новый чекпоинт.");
            LoadCheckpoint();
        }
        else
        {
            CurrentSaveData = regularData;
            // Debug.Log($"[GameManager] Загрузка основного сохранения. Сцена: {CurrentSaveData.sceneName}");
            LoadSceneWithLoadingScreen(CurrentSaveData.sceneName);
        }
    }

    public void LoadCheckpoint()
    {
        if (PlayerPrefs.HasKey(CHECKPOINT_KEY))
        {
            string json = PlayerPrefs.GetString(CHECKPOINT_KEY);
            CurrentSaveData = JsonUtility.FromJson<SaveData>(json);
            // Debug.Log($"[GameManager] Загрузка чекпоинта. Сцена: {CurrentSaveData.sceneName}");
            LoadSceneWithLoadingScreen(CurrentSaveData.sceneName);
        }
        else
        {
            // Debug.LogWarning("[GameManager] Чекпоинт не найден.");
            // Если чекпоинта нет, начинаем уровень с начала
            Debug.Log("[GameManager] Чекпоинт не найден. Начинаем уровень с начала.");
            RestartLevel();
        }
    }

    public void RestartLevel()
    {
        Debug.Log("[GameManager] Перезапуск уровня.");
        CurrentSaveData = null; // Сбрасываем данные сохранения
        isNewGame = true; // Указываем, что нужно начать уровень с начала, а не загружать сейв
        // Удаляем сохранения чекпоинтов, чтобы не было путаницы
        PlayerPrefs.DeleteKey(CHECKPOINT_KEY); 
        LoadSceneWithLoadingScreen(gameSceneName); // Загружаем игровую сцену заново
    }

    public void LoadGame(string sceneName = null)
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
        ResetGameState();

        string jsonData = PlayerPrefs.GetString(SAVE_KEY);
        CurrentSaveData = JsonUtility.FromJson<SaveData>(jsonData);
        
        if (CurrentSaveData != null && CurrentSaveData.interactiveObjectStates != null)
        {
            foreach (var state in CurrentSaveData.interactiveObjectStates)
            {
            }
        }

        SceneToLoadAfterBriv1 = CurrentSaveData.sceneName;

        SceneManager.LoadScene(loadingSceneName);
    }
    }

    public void SetLastKnownPlayerSaveData(SaveData data)
    {
        CurrentSaveData = data;
    }

    public bool HasSave()
    {
        bool hasSave = PlayerPrefs.HasKey(SAVE_KEY);
        return hasSave;
    }

    public bool HasCheckpoint()
    {
        bool hasCheckpoint = PlayerPrefs.HasKey(CHECKPOINT_KEY);
        return hasCheckpoint;
    }

    public void ClearProgress()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        CurrentSaveData = null;

        // Убедимся, что при новой игре или сбросе данных интерактивные объекты также сбрасываются
        CurrentSaveData = new SaveData();
        CurrentSaveData.interactiveObjectStates = new List<InteractiveObjectState>();
    }

    public void ClearCurrentSaveData()
    {
        CurrentSaveData = null;
    }

    public void LoadMenuScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ResetGameState();
        SceneManager.LoadScene(menuSceneName);
    }
    
    private void OnApplicationQuit()
    {
        // При выходе из игры сохраняем текущий прогресс
        if (Instance != null && SceneManager.GetActiveScene().name == gameSceneName)
        {
            SaveGame(gameSceneName);
            Debug.Log("[GameManager] Игра сохранена при выходе.");
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResetGameState()
    {
        // UnityEngine.Debug.Log("[DIAGNOSTIC] ResetGameState вызван.");
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        PauseManager.IsPaused = false;
        CurrentSaveData = null;
        isNewGame = false;
    }

    private void InitializePlayerUI(GameObject playerInstance)
    {
        if (playerInstance == null)
        {
            // Debug.LogError("[GameManager] Экземпляр игрока не найден, UI не может быть инициализирован.");
            return;
        }

        // Для PlayerHealth
        PlayerHealth playerHealth = playerInstance.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            Canvas mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>(); // Сначала ищем по имени
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>(); // Затем ищем по типу
            }

            if (mainCanvas != null)
            {
                Slider healthSlider = mainCanvas.transform.Find("Slider")?.GetComponent<Slider>();
                // if (healthSlider != null)
                // {
                //     playerHealth.SetHealthSlider(healthSlider);
                // }

                Image damageFlash = mainCanvas.transform.Find("DamageFlash")?.GetComponent<Image>();
                // if (damageFlash != null)
                // {
                //     playerHealth.SetDamageFlash(damageFlash);
                // }
            }
            playerHealth.UpdateHealthBar();
        }

        // Для PlayerShooting
        PlayerShooting playerShooting = playerInstance.GetComponent<PlayerShooting>();
        if (playerShooting != null)
        {
            Canvas mainCanvas = GameObject.Find("Canvas")?.GetComponent<Canvas>(); // Сначала ищем по имени
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>(); // Затем ищем по типу
            }

            if (mainCanvas != null)
            {
                // Поиск Total Ammo Text (BulletText)
                TMP_Text totalAmmoText = mainCanvas.transform.Find("BulletText")?.GetComponent<TMP_Text>();
                if (totalAmmoText != null)
                {
                    playerShooting.totalAmmoText = totalAmmoText;
                }

                // Поиск Clip Ammo Text (AmmoText)
                TMP_Text clipAmmoText = mainCanvas.transform.Find("AmmoText")?.GetComponent<TMP_Text>();
                if (clipAmmoText != null)
                {
                    playerShooting.clipAmmoText = clipAmmoText;
                }

                // Обновляем текст с патронами и режимом огня сразу после инициализации UI элементов
                playerShooting.RefreshAmmoText();
                playerShooting.UpdateFireModeDisplay();

                // Поиск Fire Mode Text (FileMode)
                TMP_Text fireModeText = mainCanvas.transform.Find("FileMode")?.GetComponent<TMP_Text>();
                if (fireModeText != null)
                {
                    playerShooting.fireModeText = fireModeText;
                }
            }

            // Применяем сохраненные данные о патронах и режиме огня
            if (CurrentSaveData != null)
            {
                if (playerShooting.currentWeaponData != null)
                {
                    playerShooting.currentWeaponData.ammoInClip = CurrentSaveData.ammoInClip;
                    playerShooting.currentWeaponData.ammoTotal = CurrentSaveData.totalAmmo;
                }
            }
        }
    }

    public void UpdateInteractiveObjectState(string uniqueId, string stateData)
    {
        // Debug.Log($"[GameManager] Попытка обновить состояние интерактивного объекта: ID={uniqueId}, State={stateData}");
        if (CurrentSaveData == null)
        {
            // Debug.LogWarning("[GameManager] CurrentSaveData был null при обновлении состояния объекта. Создаю новый экземпляр для текущей сессии.");
            CurrentSaveData = new SaveData();
        }

        // Находим, есть ли уже запись для этого объекта
        int index = CurrentSaveData.interactiveObjectStates.FindIndex(s => s.uniqueID == uniqueId);

        if (index != -1)
        {
            // Обновляем существующую запись
            CurrentSaveData.interactiveObjectStates[index] = new InteractiveObjectState(uniqueId, stateData);
            Debug.Log($"[GameManager] Состояние для объекта {uniqueId} обновлено на '{stateData}'.");
        }
        else
        {
            // Добавляем новую запись
            CurrentSaveData.interactiveObjectStates.Add(new InteractiveObjectState(uniqueId, stateData));
            Debug.Log($"[GameManager] Новое состояние для объекта {uniqueId} ('{stateData}') добавлено.");
        }

        // Убираем автоматическое сохранение чекпоинта - это перезаписывало чекпоинты при каждом взаимодействии
        // SaveCheckpoint();
    }

    private IEnumerator ApplyInteractiveObjectStates()
    {
        // Даем небольшую задержку, чтобы все объекты на сцене точно успели инициализироваться
        yield return new WaitForSeconds(0.1f);

        if (CurrentSaveData == null || CurrentSaveData.interactiveObjectStates.Count == 0)
        {
            Debug.Log("[GameManager] Нет данных для восстановления состояний интерактивных объектов.");
            yield break;
        }
        
        Debug.Log($"[GameManager] Начало восстановления состояний для {CurrentSaveData.interactiveObjectStates.Count} интерактивных объектов.");

        var allInteractives = FindObjectsOfType<Interactable>();
        
        foreach (var savedState in CurrentSaveData.interactiveObjectStates)
        {
            var interactiveObject = allInteractives.FirstOrDefault(i => i.uniqueID == savedState.uniqueID);
            if (interactiveObject != null)
            {
                interactiveObject.ApplySavedState(savedState.stateData);
            }
            else
            {
                Debug.LogWarning($"[GameManager] Не удалось найти интерактивный объект с ID: {savedState.uniqueID}");
            }
        }
    }

    private IEnumerator ApplyEnemyStates()
    {
        // Ждем кадр, чтобы все враги точно были проинициализированы
        yield return null; 

        if (CurrentSaveData == null || CurrentSaveData.enemies.Count == 0)
        {
            Debug.Log("[GameManager] Нет сохраненных данных о врагах.");
            yield break;
        }

        Debug.Log($"[GameManager] Начало восстановления состояния для {CurrentSaveData.enemies.Count} врагов.");
        
        // 1. Собираем всех врагов на сцене в словарь для быстрого доступа
        var enemiesOnScene = new Dictionary<string, EnemyAI>();
        foreach (var enemyAI in FindObjectsOfType<EnemyAI>())
        {
            if (!string.IsNullOrEmpty(enemyAI.uniqueID) && !enemiesOnScene.ContainsKey(enemyAI.uniqueID))
            {
                enemiesOnScene.Add(enemyAI.uniqueID, enemyAI);
            }
        }

        // 2. Применяем сохраненные данные
        foreach (var savedEnemyData in CurrentSaveData.enemies)
        {
            EnemyAI enemyToUpdate;
            if (enemiesOnScene.TryGetValue(savedEnemyData.uniqueID, out enemyToUpdate))
            {
                // Если у врага 0 HP в сохранении, отключаем его
                if (savedEnemyData.health <= 0)
                {
                    enemyToUpdate.gameObject.SetActive(false);
                }
                else
                {
                    enemyToUpdate.ApplySaveData(savedEnemyData);
                }
                // Убираем из словаря, так как мы его обработали
                enemiesOnScene.Remove(savedEnemyData.uniqueID);
            }
            else
            {
                Debug.LogWarning($"[GameManager] Враг с ID {savedEnemyData.uniqueID} из сохранения не найден на сцене.");
            }
        }

        // 3. Отключаем врагов, которых не было в сохранении (т.е. они были созданы спаунером, но игра была сохранена до их появления)
        // Этот шаг опционален, но помогает поддерживать консистентность.
        // Пока не будем этого делать, чтобы не сломать логику спаунеров, которые активируются после загрузки.
        /*
        foreach (var remainingEnemy in enemiesOnScene.Values)
        {
            Debug.Log($"[GameManager] Отключаем врага {remainingEnemy.uniqueID}, т.к. его не было в сохранении.");
            remainingEnemy.gameObject.SetActive(false);
        }
        */
    }

    private void LoadSceneWithLoadingScreen(string sceneName)
    {
        SceneToLoadAfterBriv1 = sceneName;
        SceneManager.LoadScene(loadingSceneName);
    }
}

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public float playerRotationX;
    public float playerRotationY;
    public float playerRotationZ;
    public float cameraRotationX;
    public float cameraRotationY;
    public float cameraRotationZ;
    public float playerHealth;
    public int ammoInClip;
    public int totalAmmo;
    public int fireMode;
    public float sprintTime;
    public bool isCrouching;
    public List<EnemyData> enemies = new List<EnemyData>();
    public List<InteractiveObjectState> interactiveObjectStates = new List<InteractiveObjectState>(); // Добавляем список для состояний интерактивных объектов

    public string activeWeaponId; // ID активного оружия (primary, secondary, knife)
    public string secondaryWeaponId; // ID префаба второго оружия
    public string secondaryWeaponUniqueID; // Уникальный ID экземпляра второго оружия
    public int secondaryWeaponAmmoInClip; // Патроны в магазине второго оружия
    public int secondaryWeaponTotalAmmo; // Всего патронов у второго оружия

    // Патроны для основного оружия
    public int primaryWeaponAmmoInClip; 
    public int primaryWeaponTotalAmmo;

    // Патроны для ножа
    public int knifeAmmoInClip;
    public int knifeTotalAmmo;

    public List<WeaponAmmoSave> weaponAmmoList = new List<WeaponAmmoSave>();
    public List<SceneWeaponAmmoSave> sceneWeaponAmmoList = new List<SceneWeaponAmmoSave>();
    public List<SceneDoorStateSave> sceneDoorStateList = new List<SceneDoorStateSave>();

    // --- ДАННЫЕ О ГРАНАТАХ ---
    public int fragGrenades;
    public int smokeGrenades;
    public int currentGrenadeType; // 0 for Frag, 1 for Smoke

    // --- АПТЕЧКИ ---
    public int playerMedkits;
    public List<string> collectedMedkitIDs = new List<string>();

    public long timestamp; // Добавляем временную метку

    public override string ToString()
    {
        return $"Сцена: {sceneName}, Позиция: {playerPosition}, Поворот игрока: ({playerRotationX}, {playerRotationY}, {playerRotationZ}), " +
               $"Поворот камеры: ({cameraRotationX}, {cameraRotationY}, {cameraRotationZ}), " +
               $"Здоровье: {playerHealth}, Патроны: {ammoInClip}/{totalAmmo}";
    }
}

[System.Serializable]
public class EnemyData
{
    public string uniqueID; // Уникальный ID врага
    public Vector3 position;
    public Quaternion rotation;
    public float health;
    public bool isAggressive;
    public int aiState; // 0=Idle, 1=Chase, 2=Attack, 3=Investigate, 4=Patrol
    public int ammoInClip;
    public int totalAmmo;
}

[System.Serializable]
public struct InteractiveObjectState
{
    public string uniqueID; // Уникальный идентификатор объекта
    public string stateData; // Состояние объекта (например, "collected", "open", "closed")

    public InteractiveObjectState(string id, string data)
    {
        uniqueID = id;
        stateData = data;
    }
}

[System.Serializable]
public class SceneWeaponAmmoSave
{
    public string uniqueID;
    public int ammoInClip;
    public int ammoTotal;
    public Vector3 position;
    public Quaternion rotation;
    public string weaponTypeName; // Имя/тип оружия для восстановления префаба
}

[System.Serializable]
public class WeaponAmmoSave
{
    public string weaponId;
    public int ammoInClip;
    public int ammoTotal;
}

[System.Serializable]
public class SceneDoorStateSave
{
    public string uniqueID;
    public bool isOpen;
    public Vector3 position;
    public Quaternion rotation;
}