using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputBridge : MonoBehaviour
{
    private SettingsManager settingsManager;
    private Vector2 lastMousePosition;

    void Start()
    {
        settingsManager = FindObjectOfType<SettingsManager>();
        if (settingsManager == null)
        {
            Debug.LogError("SettingsManager not found in scene!");
            return;
        }

        // Переключаемся на мобильный ввод, так как он поддерживает установку значений
        CrossPlatformInputManager.SwitchActiveInputMethod(CrossPlatformInputManager.ActiveInputMethod.Touch);
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        if (settingsManager == null) return;

        // Горизонтальное движение
        if (Input.GetKey(settingsManager.GetKeyForAction("Right")))
            CrossPlatformInputManager.SetAxisPositive("Horizontal");
        else if (Input.GetKey(settingsManager.GetKeyForAction("Left")))
            CrossPlatformInputManager.SetAxisNegative("Horizontal");
        else
            CrossPlatformInputManager.SetAxisZero("Horizontal");

        // Вертикальное движение
        if (Input.GetKey(settingsManager.GetKeyForAction("Forward")))
            CrossPlatformInputManager.SetAxisPositive("Vertical");
        else if (Input.GetKey(settingsManager.GetKeyForAction("Backward")))
            CrossPlatformInputManager.SetAxisNegative("Vertical");
        else
            CrossPlatformInputManager.SetAxisZero("Vertical");

        // Прыжок
        if (Input.GetKeyDown(settingsManager.GetKeyForAction("Jump")))
        {
            CrossPlatformInputManager.SetButtonDown("Jump");
        }
        if (Input.GetKeyUp(settingsManager.GetKeyForAction("Jump")))
        {
            CrossPlatformInputManager.SetButtonUp("Jump");
        }

        // Приседание
        if (Input.GetKey(settingsManager.GetKeyForAction("Crouch")))
        {
            CrossPlatformInputManager.SetButtonDown("Crouch");
        }
        else
        {
            CrossPlatformInputManager.SetButtonUp("Crouch");
        }

        // Обработка мыши для вращения камеры
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        CrossPlatformInputManager.SetAxis("Mouse X", mouseDelta.x);
        CrossPlatformInputManager.SetAxis("Mouse Y", mouseDelta.y);
    }
}