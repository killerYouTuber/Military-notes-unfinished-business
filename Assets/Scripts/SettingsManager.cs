using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject mainMenuButtons;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TextMeshProUGUI errorMessageText; // Текст для отображения ошибок
    
    [Header("Key Binding UI")]
    [SerializeField] private GameObject movementBindingsContainer;  // Контейнер для кнопок движения
    [SerializeField] private GameObject combatBindingsContainer;    // Контейнер для боевых кнопок
    [SerializeField] private Button[] movementBindingButtons;      // Кнопки для движения
    [SerializeField] private Button[] combatBindingButtons;        // Кнопки для боя
    [SerializeField] private TextMeshProUGUI[] movementKeyTexts;  // Тексты клавиш движения
    [SerializeField] private TextMeshProUGUI[] combatKeyTexts;    // Тексты клавиш боя
    [SerializeField] private TextMeshProUGUI[] movementActionTexts; // Описания действий движения
    [SerializeField] private TextMeshProUGUI[] combatActionTexts;  // Описания боевых действий
    
    // Фиксированные разрешения
    private Resolution[] fixedResolutions;
    private readonly Vector2Int[] resolutionOptions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),  // Full HD
        new Vector2Int(1280, 720),   // HD
        new Vector2Int(640, 480)     // VGA
    };

    private Dictionary<string, string> actionDescriptions = new Dictionary<string, string>()
    {
        // Движение
        {"Forward", "Вперёд"},
        {"Backward", "Назад"},
        {"Left", "Влево"},
        {"Right", "Вправо"},
        {"Jump", "Прыжок"},
        {"Crouch", "Присесть"},
        {"Sprint", "Бег"},
        {"Interact", "Взаимодействие"},
        
        // Боевые действия
        {"Fire", "Стрельба"},
        {"Aim", "Прицеливание"},
        {"Reload", "Перезарядка"},
        {"FireMode", "Режим огня"},
        {"Melee", "Ближний бой"},
        {"Grenade", "Граната"},
        {"Medkit", "Аптечка"}
    };
    
    private bool isWaitingForKey = false;
    private Button currentRebindButton = null;
    private string currentBindingAction = null;

    private bool isChangingResolution = false;

    private void Awake()
    {
        // Создаем список фиксированных разрешений
        fixedResolutions = new Resolution[resolutionOptions.Length];
        for (int i = 0; i < resolutionOptions.Length; i++)
        {
            fixedResolutions[i] = new Resolution
            {
                width = resolutionOptions[i].x,
                height = resolutionOptions[i].y,
                refreshRateRatio = Screen.currentResolution.refreshRateRatio
            };
        }
    }

    private void Start()
    {
        // Проверяем, находимся ли мы в меню
        bool isInMenu = resolutionDropdown != null && mainMenuButtons != null && settingsPanel != null;

        if (isInMenu)
        {
            // Инициализируем UI элементы меню
            SetupTextFormatting();
            InitializeResolutions();
            
            if (movementKeyTexts != null && movementKeyTexts.Length > 0)
            {
                UpdateKeyBindingTexts();
            }
        }

        // В любом случае загружаем сохраненные настройки
        LoadSettings();
    }

    private void SetupTextFormatting()
    {
        // Настройка форматирования для всех текстовых элементов
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in allTexts)
        {
            // Основные настройки текста
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Left;
            text.enableAutoSizing = true;
            text.fontSizeMin = 12;
            text.fontSizeMax = 28;
            text.overflowMode = TextOverflowModes.Overflow;
            text.margin = new Vector4(5, 5, 5, 5);
        }

        // Особые настройки для dropdown
        if (resolutionDropdown != null)
        {
            var dropdownText = resolutionDropdown.GetComponentInChildren<TextMeshProUGUI>();
            if (dropdownText != null)
            {
                dropdownText.alignment = TextAlignmentOptions.Center;
                dropdownText.fontSize = 16;
                dropdownText.enableAutoSizing = true;
                dropdownText.fontSizeMin = 12;
                dropdownText.fontSizeMax = 16;
            }

            // Настраиваем размер текста в выпадающем списке
            var itemText = resolutionDropdown.itemText;
            if (itemText != null)
            {
                itemText.fontSize = 16;
                itemText.enableAutoSizing = true;
                itemText.fontSizeMin = 12;
                itemText.fontSizeMax = 16;
            }
        }

        // Настройка текстов действий
        for (int i = 0; i < movementActionTexts.Length; i++)
        {
            if (movementActionTexts[i] != null)
            {
                movementActionTexts[i].alignment = TextAlignmentOptions.Left;
                movementActionTexts[i].fontSize = 24;
                movementActionTexts[i].enableAutoSizing = true;
                movementActionTexts[i].fontSizeMin = 16;
                movementActionTexts[i].fontSizeMax = 24;
            }
        }

        // Настройка текстов клавиш
        for (int i = 0; i < movementKeyTexts.Length; i++)
        {
            if (movementKeyTexts[i] != null)
            {
                movementKeyTexts[i].alignment = TextAlignmentOptions.Center;
                movementKeyTexts[i].fontSize = 20;
                movementKeyTexts[i].enableAutoSizing = true;
                movementKeyTexts[i].fontSizeMin = 14;
                movementKeyTexts[i].fontSizeMax = 20;
            }
        }
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("Resolution dropdown is not assigned!");
            return;
        }

        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // Добавляем фиксированные разрешения в dropdown
        for (int i = 0; i < fixedResolutions.Length; i++)
        {
            string option = $"{fixedResolutions[i].width} x {fixedResolutions[i].height}";
            options.Add(option);

            // Определяем текущее разрешение
            if (Screen.width == fixedResolutions[i].width && 
                Screen.height == fixedResolutions[i].height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void OnEnable()
    {
        // Проверяем, находимся ли мы в меню (есть ли необходимые UI элементы)
        bool isInMenu = resolutionDropdown != null && mainMenuButtons != null && settingsPanel != null;

        // Если мы в меню, настраиваем UI элементы
        if (isInMenu)
        {
            try
            {
                // Добавляем слушатель события изменения значения
                resolutionDropdown.onValueChanged.RemoveListener(SetResolution);
                resolutionDropdown.onValueChanged.AddListener(SetResolution);

                // Проверяем настройки TMP_Dropdown
                var dropdownText = resolutionDropdown.GetComponentInChildren<TextMeshProUGUI>();
                if (dropdownText == null)
                {
                    Debug.LogWarning("У Resolution Dropdown отсутствует компонент TextMeshProUGUI");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Ошибка настройки UI элементов меню: {e.Message}");
            }
        }
        // Если мы не в меню (например, в игровой сцене), просто загружаем сохраненные настройки
        else
        {
            LoadSettings();
        }
    }

    private void OnDisable()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        }
        StopAllCoroutines();
    }

    public void OpenSettings()
    {
        if (mainMenuButtons != null) mainMenuButtons.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (mainMenuButtons != null) mainMenuButtons.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        SaveSettings();
    }

    public void SetResolution(int resolutionIndex)
    {
        if (isChangingResolution) return;
        
        if (resolutionIndex < 0 || resolutionIndex >= fixedResolutions.Length)
        {
            Debug.LogError($"Invalid resolution index: {resolutionIndex}");
            return;
        }

        isChangingResolution = true;
        Resolution resolution = fixedResolutions[resolutionIndex];
        Debug.Log($"Attempting to set resolution to: {resolution.width}x{resolution.height}");

        try
        {
            StopAllCoroutines(); // Останавливаем все предыдущие корутины
            StartCoroutine(SetResolutionDelayed(resolution, resolutionIndex));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to set resolution: {e.Message}");
            isChangingResolution = false;
        }
    }

    private System.Collections.IEnumerator SetResolutionDelayed(Resolution resolution, int resolutionIndex)
    {
        try
        {
            // Сначала переключаемся в оконный режим
            Screen.fullScreenMode = FullScreenMode.Windowed;
            yield return new WaitForSeconds(0.1f);
            
            // Устанавливаем разрешение
            Screen.SetResolution(resolution.width, resolution.height, false);
            yield return new WaitForSeconds(0.2f);
            
            // Переключаемся в полноэкранный режим
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(resolution.width, resolution.height, true);
            
            // Сохраняем настройки
            PlayerPrefs.SetInt("SelectedResolution", resolutionIndex);
            PlayerPrefs.Save();
            
            Debug.Log($"Resolution successfully set to: {Screen.width}x{Screen.height}, Fullscreen: {Screen.fullScreen}");
        }
        finally
        {
            isChangingResolution = false;
        }
    }

    public void LoadSettings()
    {
        try
        {
            // Загружаем сохраненное разрешение
            if (PlayerPrefs.HasKey("SelectedResolution") && resolutionDropdown != null)
            {
                int savedResolutionIndex = PlayerPrefs.GetInt("SelectedResolution");
                if (savedResolutionIndex < fixedResolutions.Length)
                {
                    SetResolution(savedResolutionIndex);
                    resolutionDropdown.value = savedResolutionIndex;
                }
            }

            GlobalSettingsData.LoadSettings();
            UpdateKeyBindingTexts();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading settings: {e.Message}");
        }
    }

    private void SaveSettings()
    {
        try
        {
            GlobalSettingsData.SaveSettings();
            Debug.Log("Settings saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving settings: {e.Message}");
        }
    }

    private void UpdateKeyBindingTexts()
    {
        var keyBindings = GlobalSettingsData.GetAllKeyBindings();

        // Обновляем тексты для движения
        for (int i = 0; i < movementBindingButtons.Length; i++)
        {
            if (movementBindingButtons[i] == null) continue;
            
            string actionName = movementBindingButtons[i].name.Replace("Bind", "");
            if (keyBindings.ContainsKey(actionName))
            {
                if (movementKeyTexts != null && i < movementKeyTexts.Length && movementKeyTexts[i] != null)
                {
                    movementKeyTexts[i].text = keyBindings[actionName].ToString();
                }
                if (movementActionTexts != null && i < movementActionTexts.Length && movementActionTexts[i] != null)
                {
                    movementActionTexts[i].text = actionDescriptions[actionName];
                }
            }
        }

        // Обновляем тексты для боевых действий
        for (int i = 0; i < combatBindingButtons.Length; i++)
        {
            if (combatBindingButtons[i] == null) continue;
            
            string actionName = combatBindingButtons[i].name.Replace("Bind", "");
            if (keyBindings.ContainsKey(actionName))
            {
                if (combatKeyTexts != null && i < combatKeyTexts.Length && combatKeyTexts[i] != null)
                {
                    combatKeyTexts[i].text = keyBindings[actionName].ToString();
                }
                if (combatActionTexts != null && i < combatActionTexts.Length && combatActionTexts[i] != null)
                {
                    combatActionTexts[i].text = actionDescriptions[actionName];
                }
            }
        }
    }

    public void StartRebinding(Button buttonClicked)
    {
        if (isWaitingForKey) return;

        isWaitingForKey = true;
        currentRebindButton = buttonClicked;
        currentBindingAction = buttonClicked.name.Replace("Bind", "");

        // Определяем, к какой группе принадлежит кнопка
        int buttonIndex;
        TextMeshProUGUI[] targetKeyTexts;

        if (movementBindingButtons.Contains(buttonClicked))
        {
            buttonIndex = System.Array.IndexOf(movementBindingButtons, buttonClicked);
            targetKeyTexts = movementKeyTexts;
        }
        else
        {
            buttonIndex = System.Array.IndexOf(combatBindingButtons, buttonClicked);
            targetKeyTexts = combatKeyTexts;
        }

        if (buttonIndex != -1 && targetKeyTexts != null && buttonIndex < targetKeyTexts.Length)
        {
            targetKeyTexts[buttonIndex].text = "Нажмите клавишу...";
        }
    }

    private void Update()
    {
        if (!isWaitingForKey) return;

        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // Проверяем, не используется ли уже эта клавиша
                if (!IsKeyAlreadyUsed(keyCode))
                {
                    // Клавиша свободна, назначаем её
                    GlobalSettingsData.SetKeyForAction(currentBindingAction, keyCode);
                    isWaitingForKey = false;
                    currentBindingAction = null;
                    UpdateKeyBindingTexts();
                }
                break;
            }
        }
    }

    private bool IsKeyAlreadyUsed(KeyCode keyCode)
    {
        var keyBindings = GlobalSettingsData.GetAllKeyBindings();
        foreach (var binding in keyBindings)
        {
            if (binding.Key == currentBindingAction) continue;
            if (binding.Value == keyCode) return true;
        }
        return false;
    }

    public KeyCode GetKeyForAction(string actionName)
    {
        return GlobalSettingsData.GetKeyForAction(actionName);
    }

    private void OnDestroy()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
        }
    }

    // Обработчик изменения разрешения
    private void OnResolutionChanged(int index)
    {
        if (isChangingResolution) return; // Предотвращаем повторные вызовы
        
        Debug.Log($"Resolution dropdown value changed to index: {index}");
        SetResolution(index);
    }
} 