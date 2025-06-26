using UnityEngine;
using System.Collections;

public class FragGrenade : MonoBehaviour
{
    [Header("Настройки взрыва")]
    public float explosionRadius = 5f;
    public float explosionForce = 500f;
    public float damage = 100f;
    public float fuseTime = 3f;
    public LayerMask damageLayerMask = -1;
    
    [Header("Эффекты")]
    public GameObject explosionEffect;
    public AudioClip explosionSound;
    public float explosionVolume = 1f;
    
    [Header("Физика")]
    public float bounceForce = 0.5f;
    public int maxBounces = 3;
    
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool hasExploded = false;
    private int bounceCount = 0;
    private float timer = 0f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Запускаем таймер взрыва
        StartCoroutine(ExplodeAfterDelay());
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Взрываемся по таймеру
        if (timer >= fuseTime && !hasExploded)
        {
            Explode();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        bounceCount++;
        
        // Взрываемся при слишком частых столкновениях
        if (bounceCount >= maxBounces && !hasExploded)
        {
            Explode();
        }
    }
    
    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(fuseTime);
        
        if (!hasExploded)
        {
            Explode();
        }
    }
    
    public void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        // Находим все объекты в радиусе взрыва
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider nearbyObject in colliders)
        {
            // Наносим урон
            ApplyDamage(nearbyObject);
            
            // Применяем силу взрыва
            ApplyExplosionForce(nearbyObject);
            
            // Сообщаем близким врагам об опасности (для укрытия)
            var ai = nearbyObject.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.OnGrenadeDanger(transform.position, explosionRadius);
            }
        }
        
        // Создаем эффект взрыва
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        
        // Проигрываем звук взрыва
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound, explosionVolume);
        }
        
        // Уничтожаем гранату
        Destroy(gameObject, 0.1f);
    }
    
    private void ApplyDamage(Collider target)
    {
        // Получаем расстояние до цели
        float distance = Vector3.Distance(transform.position, target.transform.position);
        
        // Рассчитываем урон в зависимости от расстояния
        float damageMultiplier = 1f - (distance / explosionRadius);
        damageMultiplier = Mathf.Clamp01(damageMultiplier);
        
        float finalDamage = damage * damageMultiplier;
        
        // Наносим урон игроку
        var playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage((int)finalDamage);
            Debug.Log($"Игрок получил {(int)finalDamage} урона от осколочной гранаты");
        }
        
        // Наносим урон врагам
        var enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(finalDamage);
            Debug.Log($"Враг получил {finalDamage} урона от осколочной гранаты");
        }
    }
    
    private void ApplyExplosionForce(Collider target)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 explosionDirection = (target.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, target.transform.position);
            
            // Сила взрыва уменьшается с расстоянием
            float forceMultiplier = 1f - (distance / explosionRadius);
            forceMultiplier = Mathf.Clamp01(forceMultiplier);
            
            targetRb.AddExplosionForce(explosionForce * forceMultiplier, transform.position, explosionRadius);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Отображаем радиус взрыва в редакторе
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
} 