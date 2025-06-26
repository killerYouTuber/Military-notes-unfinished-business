using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class GlobalSettingsData
{
    public static event System.Action OnKeyBindingsChanged;

    private static Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>()
    {
        // Движение
        {"Forward", KeyCode.W},
        {"Backward", KeyCode.S},
        {"Left", KeyCode.A},
        {"Right", KeyCode.D},
        {"Jump", KeyCode.Space},
        {"Crouch", KeyCode.C},
        {"Sprint", KeyCode.LeftShift},
        {"Interact", KeyCode.E},
        
        // Боевые действия
        {"Fire", KeyCode.Mouse0},        // ЛКМ
        {"Aim", KeyCode.Mouse1},         // ПКМ
        {"Reload", KeyCode.R},
        {"FireMode", KeyCode.B},
        {"Melee", KeyCode.V},
        {"Grenade", KeyCode.G},
        {"Medkit", KeyCode.H}
    };
    
    public static Dictionary<string, string> actionDescriptions = new Dictionary<string, string>()
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

    // Настройки дисплея
    public static int resolutionWidth = 1920; // Значение по умолчанию
    public static int resolutionHeight = 1080; // Значение по умолчанию
    public static bool isFullscreen = true; // Значение по умолчанию

    public static void LoadSettings()
    {
        LoadKeyBindings();
        LoadDisplaySettings();
    }

    public static void SaveSettings()
    {
        SaveKeyBindings();
        SaveDisplaySettings();
        PlayerPrefs.Save();
    }

    public static void LoadKeyBindings()
    {
        foreach (var binding in keyBindings.Keys.ToArray())
        {
            if (PlayerPrefs.HasKey($"KeyBinding_{binding}"))
            {
                keyBindings[binding] = (KeyCode)PlayerPrefs.GetInt($"KeyBinding_{binding}");
                Debug.Log($"[GlobalSettingsData] Загружена привязка для {binding}: {keyBindings[binding]}");
            }
        }
    }

    public static void SaveKeyBindings()
    {
        foreach (var binding in keyBindings)
        {
            PlayerPrefs.SetInt($"KeyBinding_{binding.Key}", (int)binding.Value);
            Debug.Log($"[GlobalSettingsData] Сохранена привязка для {binding.Key}: {binding.Value}");
        }
        PlayerPrefs.Save();
        Debug.Log("[GlobalSettingsData] Все привязки клавиш сохранены!");
    }

    public static void LoadDisplaySettings()
    {
        if (PlayerPrefs.HasKey("ResolutionWidth") && PlayerPrefs.HasKey("ResolutionHeight"))
        {
            resolutionWidth = PlayerPrefs.GetInt("ResolutionWidth");
            resolutionHeight = PlayerPrefs.GetInt("ResolutionHeight");
            Debug.Log($"[GlobalSettingsData] Загружено разрешение: {resolutionWidth}x{resolutionHeight}");
        }
        else
        {
            // Если нет сохраненных настроек, используем текущее разрешение экрана как дефолтное
            resolutionWidth = Screen.currentResolution.width;
            resolutionHeight = Screen.currentResolution.height;
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            isFullscreen = PlayerPrefs.GetInt("Fullscreen") == 1 ? true : false;
            Debug.Log($"[GlobalSettingsData] Загружена полноэкранная настройка: {isFullscreen}");
        }
        else
        {
            // Если нет сохраненных настроек, используем текущий полноэкранный режим как дефолтный
            isFullscreen = Screen.fullScreen;
        }
    }

    public static void SaveDisplaySettings()
    {
        PlayerPrefs.SetInt("ResolutionWidth", resolutionWidth);
        PlayerPrefs.SetInt("ResolutionHeight", resolutionHeight);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Debug.Log("[GlobalSettingsData] Настройки дисплея сохранены!");
    }

    public static bool HasSavedKeyBindings()
    {
        return PlayerPrefs.HasKey("KeyBinding_Forward");
    }

    public static void SaveDefaultKeyBindings()
    {
        foreach (var binding in keyBindings)
        {
            PlayerPrefs.SetInt($"KeyBinding_{binding.Key}", (int)binding.Value);
        }
        PlayerPrefs.Save();
        Debug.Log("[GlobalSettingsData] Сохранены привязки клавиш по умолчанию!");
    }

    public static KeyCode GetKeyForAction(string actionName)
    {
        if (keyBindings.ContainsKey(actionName))
        {
            return keyBindings[actionName];
        }
        Debug.LogWarning($"[GlobalSettingsData] Действие '{actionName}' не найдено в привязках клавиш.");
        return KeyCode.None;
    }

    public static void SetKeyForAction(string actionName, KeyCode keyCode)
    {
        if (keyBindings.ContainsKey(actionName))
        {
            keyBindings[actionName] = keyCode;
            Debug.Log($"[GlobalSettingsData] Установлена клавиша {keyCode} для действия {actionName}. Сохраняем...");
            SaveKeyBindings();
            OnKeyBindingsChanged?.Invoke(); // Вызываем событие после сохранения
        }
        else
        {
            Debug.LogWarning($"[GlobalSettingsData] Попытка установить клавишу для неизвестного действия: {actionName}");
        }
    }

    public static Dictionary<string, KeyCode> GetAllKeyBindings()
    {
        return new Dictionary<string, KeyCode>(keyBindings);
    }

    // Новый метод для получения копии словаря привязок клавиш
    public static IReadOnlyDictionary<string, KeyCode> GetKeyBindings()
    {
        return new Dictionary<string, KeyCode>(keyBindings);
    }
} 