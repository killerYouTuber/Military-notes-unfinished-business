using UnityEngine;
using UnityEngine.Events;  
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Настройки здоровья")]
    public int maxHealth = 100;          // Максимальное здоровье
    public int currentHealth;            // Текущее здоровье
    public bool isInvincible = false;    // Режим неуязвимости (например, после удара)
    public float invincibilityTime = 1f; // Длительность неуязвимости после удара

    [Header("События")]
    public UnityEvent onDamageTaken;     // Событие при получении урона
    public UnityEvent onHeal;            // Событие при лечении
    public UnityEvent onDeath;           // Событие при смерти

    private float lastDamageTime;

    private void Start()
    {
        currentHealth = maxHealth; // Инициализация здоровья
    }

    // Метод для нанесения урона
    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;
        onDamageTaken.Invoke(); // Запуск события

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFrame());
        }
    }

    // Метод для лечения
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHeal.Invoke(); // Запуск события
    }

    // Смерть объекта
    private void Die()
    {
        onDeath.Invoke(); // Запуск события
        Destroy(gameObject); // Уничтожение объекта (или можно отключить)
    }

    // Неуязвимость после удара
    private IEnumerator InvincibilityFrame()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }
}