using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEnemy : MonoBehaviour
{

    public float speed = 30f;
    public int damage = 10;
    public float gravity = 0.02f; 
    public bool isShotgunPellet = false; // Флаг для дробинок
    private Vector3 velocity;
    private Rigidbody rb;
    public LayerMask destroyOnLayers;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        velocity = transform.forward * speed;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = velocity;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // Уничтожаем пулю через 10 секунд, если она никуда не попала
        Destroy(gameObject, 10f);
    }

    void FixedUpdate()
    {
        // ��������� ����������
        velocity += Physics.gravity * gravity * Time.fixedDeltaTime;

        if (rb != null)
        {
            rb.velocity = velocity;
            if (velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime); // ������� �������� �� ����������� ��������
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[BulletEnemy] Пуля столкнулась с: {other.name}, слой: {LayerMask.LayerToName(other.gameObject.layer)}, позиция: {transform.position}");
        if (other.GetComponent<Bullet>() != null || other.GetComponent<BulletEnemy>() != null)
            return;

        // --- НОВАЯ ЛОГИКА ДЛЯ ДРОБОВИКА ---
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            DamageBuffer damageBuffer = other.GetComponent<DamageBuffer>();

            if (playerHealth != null)
            {
                if (isShotgunPellet && damageBuffer != null)
                {
                    // Регистрируем попадание в буфер, а не наносим урон напрямую
                    damageBuffer.RegisterHit(damage);
                    Debug.Log($"Дробинка зарегистрировала попадание: {damage} урона.");
                }
                else
                {
                    // Обычное оружие наносит урон напрямую
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Пуля врага нанесла урон игроку: {damage}");
                }
            }
            Destroy(gameObject);
            return;
        }
        // --- КОНЕЦ НОВОЙ ЛОГИКИ ---

        // Потом проверка по LayerMask
        if (((1 << other.gameObject.layer) & destroyOnLayers.value) != 0)
        {
            Debug.Log($"Пуля врага столкнулась с объектом слоя {LayerMask.LayerToName(other.gameObject.layer)} и будет уничтожена");
            Destroy(gameObject);
            return;
        }

        // Destroy(gameObject); // Отключено для отладки
    }

    // Установка параметров пули из EnemyShooting
    public void SetParams(float newSpeed, int newDamage)
    {
        speed = newSpeed;
        damage = newDamage;
        velocity = transform.forward * speed;
        if (rb != null)
        {
            rb.velocity = velocity;
        }
    }
}
