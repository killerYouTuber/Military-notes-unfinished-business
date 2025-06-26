using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI missionText;
    
    [Header("Mission Settings")]
    [Tooltip("Массив текстов миссий. Каждый следующий текст будет показываться после выполнения предыдущей задачи")]
    public string[] missionTexts;
    public string nextSceneName = "briv2";

    private int currentMissionIndex = 0;
    private bool isEscapePointReached = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        missionText = null;
        StartCoroutine(FindAndAssignMissionTextCoroutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        missionText = null;
        StartCoroutine(FindAndAssignMissionTextCoroutine());
    }

    private IEnumerator FindAndAssignMissionTextCoroutine()
    {
        yield return null;

        missionText = null;

        if (missionText != null)
        {
            Debug.Log("[MissionManager] MissionText already assigned in Inspector. Skipping automatic search.");
            UpdateMissionDisplay();
            yield break;
        }

        Debug.Log("[MissionManager] Starting coroutine to find and assign MissionText (no space)...");

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null)
        {
            Debug.Log("[MissionManager] Canvas found. Searching for 'MissionText' (no space) child.");
            Transform missionTextTransform = mainCanvas.transform.Find("MissionText");
            if (missionTextTransform != null)
            {
                missionText = missionTextTransform.GetComponent<TextMeshProUGUI>();
                if (missionText != null)
                {
                    Debug.Log("[MissionManager] MissionText TextMeshProUGUI component found and assigned successfully.");
                    UpdateMissionDisplay();
                }
                else
                {
                    Debug.LogWarning("[MissionManager] MissionText GameObject found, but TextMeshProUGUI component is missing.");
                }
            }
            else
            {
                Debug.LogWarning("[MissionManager] No child object named 'MissionText' (no space) found under Canvas.");
            }
        }
        else
        {
            Debug.LogWarning("[MissionManager] No Canvas found in the current scene.");
        }

        if (missionText == null)
        {
            Debug.LogWarning("[MissionManager] Final check: MissionText UI element (no space) not found as a child of Canvas, and not assigned in Inspector. Cannot update display.");
        }
    }

    private void UpdateMissionDisplay()
    {
        if (missionText != null && missionTexts != null && missionTexts.Length > 0)
        {
            UpdateMissionText(missionTexts[currentMissionIndex]);
        }
        else
        {
            Debug.LogWarning("[MissionManager] Mission Text UI element is null or missionTexts array is empty. Cannot update display.");
        }
    }

    public void OnDocumentsCollected()
    {
        AdvanceMission();
    }

    public void OnEscapePointReached()
    {
        if (currentMissionIndex >= missionTexts.Length - 1 && !isEscapePointReached)
        {
            isEscapePointReached = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void AdvanceMission()
    {
        currentMissionIndex++;
        if (currentMissionIndex < missionTexts.Length)
        {
            UpdateMissionText(missionTexts[currentMissionIndex]);
        }
    }

    public void ResetMission()
    {
        currentMissionIndex = 0;
        UpdateMissionDisplay();
    }

    private void UpdateMissionText(string text)
    {
        if (missionText != null)
        {
            missionText.text = text;
        }
    }
} 