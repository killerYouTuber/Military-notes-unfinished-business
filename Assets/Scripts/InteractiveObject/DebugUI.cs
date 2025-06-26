using UnityEngine;
using TMPro;

public class DebugUI : MonoBehaviour
{
    public static DebugUI Instance;

    [Header("Ёлементы UI")]
    public TMP_Text interactionPromptText;
    public GameObject promptPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static void ShowPrompt(string message)
    {
        if (Instance == null) return;

        Instance.interactionPromptText.text = message;
        Instance.promptPanel.SetActive(true);
    }

    public static void HidePrompt()
    {
        if (Instance == null) return;

        Instance.promptPanel.SetActive(false);
    }
}