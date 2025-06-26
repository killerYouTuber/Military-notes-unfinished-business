using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isInvincible = false;
    public float invincibilityTime = 1f;

    [Header("UI Settings")]
    public Color flashColor = new Color(1, 0, 0, 0.5f);
    // public float flashDuration = 0.2f; // УДАЛЕНО, чтобы не было дублирования

    [Header("Events")]
    public UnityEvent onDamageTaken;
    public UnityEvent onHeal;
    public UnityEvent onDeath;
    
    // Эти поля теперь приватные, скрипт найдет их сам
    private Slider healthSlider;
    // private Image damageFlash; // УДАЛЕНО

    [Header("Medkit Settings")]
    public int maxMedkits = 5;
    public int currentMedkits = 3;
    private TMP_Text medkitText;

    [Header("Damage Flash Settings")]
    public GameObject damageFlashObject; // Назначается вручную или ищется по имени
    public float flashDuration = 0.2f;

    [Header("Messages UI")]
    [Tooltip("Текстовое поле для вывода сообщений, например, 'Максимум аптечек'.")]
    public TMP_Text messagesText; // Поле для сообщений

    public Slider HealthSlider { get { return healthSlider; } }
    // public Image DamageFlash { get { return damageFlash; } } // УДАЛЕНО
    public TMP_Text MedkitText { get { return medkitText; } }

    // Свойство, которое показывает, полное ли количество аптечек
    public bool HasMaxMedkits => currentMedkits >= maxMedkits;

    private Coroutine messageCoroutine;

    private void Awake()
    {
        currentHealth = maxHealth;
        // Автоматически добавляем DamageBuffer, если его нет
        if (GetComponent<DamageBuffer>() == null)
        {
            gameObject.AddComponent<DamageBuffer>();
        }

        // --- УЛУЧШЕННЫЙ АВТОМАТИЧЕСКИЙ ПОИСК UI ---
        
        // Ищем корневой Canvas на сцене
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas != null)
        {
            // Ищем компоненты внутри Canvas. Это надежнее, чем поиск по имени.
            healthSlider = mainCanvas.GetComponentInChildren<Slider>(true); // true, чтобы найти даже неактивные
            // damageFlash = mainCanvas.GetComponentInChildren<Image>(true); // (если имя Image уникально для флеша)
            
            // Если у вас несколько Image на Canvas, лучше добавить уникальный тег или компонент-маркер.
            // Для DamageFlash можно сделать так:
            // if (damageFlash != null && damageFlash.name != "DamageFlash") // Простая проверка по имени для уточнения
            // {
            //      // Пытаемся найти по имени внутри Canvas
            //      Transform flashTransform = mainCanvas.transform.Find("DamageFlash");
            //      if(flashTransform != null)
            //      {
            //          damageFlash = flashTransform.GetComponent<Image>();
            //      }
            //      else
            //      {
            //         // Если все равно не нашли, возможно, есть несколько Image, и мы взяли не тот.
            //         Debug.LogWarning("PlayerHealth: Найден компонент Image, но его имя не 'DamageFlash'. Возможна ошибка в назначении.");
            //      }
            // }

            if (healthSlider == null)
            {
                Debug.LogError("PlayerHealth: Не найден компонент 'Slider' внутри Canvas на сцене!");
            }
            // if (damageFlash == null)
            // {
            //     Debug.LogError("PlayerHealth: Не найден компонент 'Image' для DamageFlash внутри Canvas на сцене!");
            // }
        }
        else
        {
            Debug.LogError("PlayerHealth: Не найден 'Canvas' на сцене! UI не будет работать.");
        }
        
        // Автоматически подписываем методы на события
        onDamageTaken.AddListener(UpdateHealthBar);
        // onDamageTaken.AddListener(PlayDamageFlash);
        onHeal.AddListener(UpdateHealthBar);

        // --- ПОИСК ТЕКСТА АПТЕЧЕК ---
        Canvas mainCanvasForMedkits = FindObjectOfType<Canvas>();
        if (mainCanvasForMedkits != null)
        {
            TMP_Text[] texts = mainCanvasForMedkits.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                if (t.text.Contains("Аптечки"))
                {
                    medkitText = t;
                    break;
                }
            }
            if (medkitText == null)
            {
                Debug.LogWarning("PlayerHealth: Не найден TMP_Text для аптечек на Canvas!");
            }
        }

        // Ищем текстовое поле для сообщений
        if (messagesText == null)
        {
            GameObject msgObj = GameObject.Find("Messegs");
            if (msgObj != null)
            {
                messagesText = msgObj.GetComponent<TMP_Text>();
                if (messagesText != null)
                {
                    messagesText.gameObject.SetActive(false); // Скрываем по умолчанию
                }
            }
            if (messagesText == null)
            {
                Debug.LogWarning("PlayerHealth: Не найден TMP_Text с именем 'Messegs' на сцене!");
            }
        }
    }

    void Start()
    {
        if (damageFlashObject == null)
        {
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                Transform flashTransform = mainCanvas.transform.Find("DamageFlash");
                if (flashTransform != null)
                {
                    damageFlashObject = flashTransform.gameObject;
                }
            }
        }
        if (damageFlashObject != null)
        {
            damageFlashObject.SetActive(false);
        }
        // Гарантируем подписку на событие урона
        onDamageTaken.RemoveListener(PlayDamageFlash);
        onDamageTaken.AddListener(PlayDamageFlash);
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
        currentMedkits = maxMedkits; // Всегда максимум аптечек при старте
        UpdateHealthBar(); 
        UpdateMedkitText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseMedkit();
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        onDamageTaken.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrame());
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHeal.Invoke();
    }

    public void UseMedkit()
    {
        if (currentMedkits > 0 && currentHealth < maxHealth)
        {
            currentHealth = maxHealth;
            currentMedkits--;
            UpdateHealthBar();
            UpdateMedkitText();
            onHeal.Invoke();
        }
    }

    public void AddMedkit(int amount)
    {
        currentMedkits = Mathf.Min(currentMedkits + amount, maxMedkits);
        UpdateMedkitText();
    }

    public void RestoreMedkitsToMax()
    {
        currentMedkits = maxMedkits;
        UpdateMedkitText();
    }

    public void UpdateMedkitText()
    {
        if (medkitText != null)
        {
            medkitText.text = $"Аптечки: {currentMedkits}";
        }
    }

    // Для сохранения/загрузки количества аптечек
    public int GetMedkitCount() => currentMedkits;
    public void SetMedkitCount(int count)
    {
        currentMedkits = Mathf.Clamp(count, 0, maxMedkits);
        UpdateMedkitText();
    }

    private void Die()
    {
        onDeath.Invoke();
        Debug.Log("Игрок умер!");

        if (GameManager.Instance != null && GameManager.Instance.gameOverManager != null)
        {
            GameManager.Instance.gameOverManager.ShowGameOverScreen();
        }
        else
        {
            Debug.LogError("GameOverManager не найден! Не могу показать экран смерти.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator InvincibilityFrame()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
    }

    public void PlayDamageFlash()
    {
        Debug.Log("PlayDamageFlash вызван!");
        if (damageFlashObject != null)
        {
            StartCoroutine(DamageFlashRoutine());
        }
        else
        {
            Debug.LogWarning("damageFlashObject не найден!");
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        Debug.Log("DamageFlashRoutine старт!");
        damageFlashObject.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        damageFlashObject.SetActive(false);
        Debug.Log("DamageFlashRoutine завершён!");
    }

    // Обновленный метод для отображения временных сообщений
    public void ShowMessage(string message, float duration)
    {
        if (messagesText != null)
        {
            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
            }
            messageCoroutine = StartCoroutine(ShowMessageRoutine(message, duration));
        }
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        messagesText.text = message;
        messagesText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messagesText.gameObject.SetActive(false);
    }

    // Метод, чтобы немедленно скрыть сообщение
    public void HideMessage()
    {
        if (messagesText != null)
        {
            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
                messageCoroutine = null;
            }
            messagesText.gameObject.SetActive(false);
        }
    }
}