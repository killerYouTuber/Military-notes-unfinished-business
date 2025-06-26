using UnityEngine;
using System.Collections.Generic;

public class DamageZone : MonoBehaviour
{
    [Tooltip("Урон, наносимый за один раз")]
    public int damageAmount = 10;

    [Tooltip("Как часто наносится урон (в секундах)")]
    public float damageInterval = 1f;

    // Словарь для отслеживания таймеров для каждого игрока в зоне
    private Dictionary<PlayerHealth, float> playerTimers = new Dictionary<PlayerHealth, float>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerTimers.ContainsKey(playerHealth))
            {
                // Наносим урон сразу при входе
                playerHealth.TakeDamage(damageAmount);
                // Добавляем игрока в словарь и записываем время нанесения урона
                playerTimers.Add(playerHealth, Time.time);
                Debug.Log($"Игрок вошел в зону урона. Нанесено {damageAmount} урона.");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && playerTimers.ContainsKey(playerHealth))
            {
                // Проверяем, прошло ли достаточно времени для повторного урона
                if (Time.time - playerTimers[playerHealth] >= damageInterval)
                {
                    playerHealth.TakeDamage(damageAmount);
                    // Обновляем время последнего урона
                    playerTimers[playerHealth] = Time.time; 
                    Debug.Log($"Игрок все еще в зоне. Нанесено {damageAmount} урона.");
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            // Удаляем игрока из отслеживания, когда он покидает зону
            if (playerHealth != null && playerTimers.ContainsKey(playerHealth))
            {
                playerTimers.Remove(playerHealth);
                Debug.Log("Игрок покинул зону урона.");
            }
        }
    }
} 