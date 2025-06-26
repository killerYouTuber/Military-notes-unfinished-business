using UnityEngine;

public class DamageBuffer : MonoBehaviour
{
    private float totalBufferedDamage = 0;
    private float bufferTime = 0.1f; // Время для накопления урона (3-5 кадров)
    private float timer = 0f;
    private PlayerHealth health;

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogError("DamageBuffer не может найти компонент PlayerHealth!", this);
            enabled = false;
        }
    }

    public void RegisterHit(int damage)
    {
        totalBufferedDamage += damage;
        timer = bufferTime; // Сбрасываем таймер при каждом попадании
    }

    void Update()
    {
        if (totalBufferedDamage > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                // Применяем весь накопленный урон одним вызовом
                health.TakeDamage((int)totalBufferedDamage);
                Debug.Log($"[DamageBuffer] Применен суммарный урон от дробовика: {totalBufferedDamage}");
                
                // Сбрасываем буфер
                totalBufferedDamage = 0;
            }
        }
    }
}
