using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public float health = 100f;
    private EnemyAI enemyAI;
    public Transform eye;
    public event System.Action OnDeath;

    private void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float damage)
    {
        if (health <= 0) return; // Чтобы метод не вызывался для уже мертвого врага

        health -= damage;
        Debug.Log($"{name} получил {damage} урона, осталось {health} здоровья.");

        if (enemyAI != null)
        {
            enemyAI.OnDamagedByPlayer(Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} погиб!");
        // 1. Оповещаем союзников
        AlertNearbyAllies();

                  // --- Бросаем оружие на землю ---
          var enemyShooting = GetComponent<EnemyShooting>();
          if (enemyShooting != null && enemyShooting.droppedWeaponPrefab != null)
          {
              // Создаем оружие немного в стороне и выше от врага
              Vector3 spawnPosition = transform.position + Vector3.up * 0.5f + transform.right * 0.5f;
              GameObject dropped = Instantiate(enemyShooting.droppedWeaponPrefab, spawnPosition, Random.rotation);
              dropped.SetActive(true);

              // Добавляем физику
              Rigidbody rb = dropped.GetComponent<Rigidbody>();
              if (rb == null)
              {
                  rb = dropped.AddComponent<Rigidbody>();
              }

              // Применяем случайную силу броска
              Vector3 throwForce = new Vector3(
                  Random.Range(-2f, 2f),
                  3f,
                  Random.Range(-2f, 2f)
              );
              rb.AddForce(throwForce, ForceMode.Impulse);

              Debug.Log($"{name} выбросил оружие: {dropped.name}");
          }

        // Вызов события смерти
        OnDeath?.Invoke();

        // 2. Отключаем компонент, чтобы избежать повторных вызовов,
        // и запускаем анимацию смерти или другие эффекты.
        // Вместо немедленного Destroy(gameObject), чтобы успела пройти анимация.
        GetComponent<Collider>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        this.enabled = false;
        
        if(enemyAI != null)
            enemyAI.enabled = false;

        // Запускаем анимацию смерти
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        
        // Уничтожаем объект после проигрывания анимации
        float destroyDelay = anim != null ? anim.GetCurrentAnimatorStateInfo(0).length : 3f;
        Destroy(gameObject, destroyDelay);
    }

    private void AlertNearbyAllies()
    {
        if (enemyAI == null) return;
        Debug.Log($"{name} оповещает союзников о своей смерти!");
        Collider[] allies = Physics.OverlapSphere(transform.position, enemyAI.allyDeathReactionRadius);
        foreach (var allyCollider in allies)
        {
            // Проверяем, что это другой враг
            if (allyCollider.gameObject == this.gameObject) continue;

            if (allyCollider.TryGetComponent<EnemyAI>(out var allyAI))
            {
                // Проверяем, есть ли препятствие между врагами
                Vector3 startPoint = eye != null ? eye.position : transform.position;
                Vector3 targetPoint = allyAI.eye != null ? allyAI.eye.position : allyAI.transform.position;
                Vector3 direction = targetPoint - startPoint;
                float distance = direction.magnitude;

                if (!Physics.Raycast(startPoint, direction.normalized, distance, enemyAI.obstacleMask))
                {
                    Debug.Log($"{name} успешно оповестил {allyAI.name} о своей смерти!");
                    // Отправляем оповещение, только если нет препятствий
                    allyAI.Alert(transform.position);
                }
                else
                {
                    Debug.Log($"{name} не смог оповестить {allyAI.name} — есть препятствие!");
                }
            }
        }
    }

    public void KillSilent()
    {
        if (health <= 0) return;
        health = 0;
        Debug.Log($"{name} был мгновенно убит ударом ножа в состоянии Idle!");
        Die();
    }
}