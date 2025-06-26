using UnityEngine;

public class SmokeGrenade : MonoBehaviour
{
    [Header("Smoke Settings")]
    public float smokeRadius = 10f;
    public float smokeDuration = 8f;
    public LayerMask enemyLayer = 1; // Layer с врагами
    
    [Header("Visual Effects")]
    public GameObject smokeEffect;
    public ParticleSystem smokeParticles;
    
    private bool isActivated = false;
    private float activationTimer = 0f;
    
    void Start()
    {
        // Автоматически активируем дым через 3 секунды после броска
        Invoke(nameof(ActivateSmoke), 3f);
    }
    
    void Update()
    {
        if (isActivated)
        {
            activationTimer += Time.deltaTime;
            
            if (activationTimer >= smokeDuration)
            {
                DeactivateSmoke();
            }
        }
    }
    
    void ActivateSmoke()
    {
        isActivated = true;
        activationTimer = 0f;
        
        // Включаем визуальные эффекты
        if (smokeEffect != null)
            smokeEffect.SetActive(true);
            
        if (smokeParticles != null)
            smokeParticles.Play();
        
        // Находим всех врагов в радиусе и уменьшаем их зрение
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, smokeRadius, enemyLayer);
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            if (enemyCollider.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                enemyAI.ApplySmokeBlindness(smokeDuration);
                enemyAI.OnSmokeEnter(transform.position, smokeRadius);
            }

        }
        
        Debug.Log($"Дымовая граната активирована! Затронуто врагов: {enemiesInRange.Length}");
    }
    
    void DeactivateSmoke()
    {
        isActivated = false;
        
        // Выключаем визуальные эффекты
        if (smokeEffect != null)
            smokeEffect.SetActive(false);
            
        if (smokeParticles != null)
            smokeParticles.Stop();
        
        Debug.Log("Дымовая граната деактивирована, эффект рассеивается.");
        
        // Уничтожаем объект гранаты
        Destroy(gameObject, 2f);
    }
    
    void OnDrawGizmosSelected()
    {
        // Показываем радиус действия дыма в редакторе
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, smokeRadius);
    }
} 