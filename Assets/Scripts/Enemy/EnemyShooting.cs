using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyShooting : MonoBehaviour
{
    [Header("Оружие врага")]
    public WeaponParametrs weapon; // Назначь нужный префаб оружия в инспекторе
    [Tooltip("Префаб оружия, который выпадет при смерти врага (должен быть WeaponInteractable)")]
    public GameObject droppedWeaponPrefab; // Префаб оружия для выпадения
    public float enemyWeaponDamage = 10f; // Урон врага (настраивается отдельно)

    [Header("AI & стрельба")]
    public float shootRange = 15f;
    public float shootCooldown = 2f;
    private EnemyAI enemyAI;
    private float lastShootTime;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    void Start()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (weapon == null)
        {
            Debug.LogError("EnemyShooting: Не назначено оружие (WeaponParametrs)!");
        }
    }

    void Update()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                int need = weapon.clipSize - weapon.ammoInClip;
                int take = Mathf.Min(need, weapon.ammoTotal);
                weapon.ammoInClip += take;
                weapon.ammoTotal -= take;
                isReloading = false;
                Debug.Log($"{name}: Перезарядка завершена! ammoInClip={weapon.ammoInClip}, ammoTotal={weapon.ammoTotal}");
            }
            return;
        }
        if (enemyAI.target == null)
        {
            Debug.Log($"{name}: Нет цели для стрельбы (target == null)");
            return;
        }
        if (weapon == null)
        {
            Debug.Log($"{name}: Нет оружия для стрельбы (weapon == null)");
            return;
        }
        if (!enemyAI.isAggressive)
        {
            Debug.Log($"{name}: Не агрессивен (isAggressive = false)");
            return;
        }
        // Используем targetLookPoint, если задан, иначе target.position + Vector3.up * 1.5f
        Vector3 targetPos = (enemyAI.targetLookPoint != null) ? enemyAI.targetLookPoint.position : enemyAI.target.position + Vector3.up * 1.5f;
        Vector3 rayOrigin = (enemyAI.eye != null) ? enemyAI.eye.position : transform.position;
        Vector3 directionToTarget = (targetPos - rayOrigin).normalized;
        float distance = Vector3.Distance(targetPos, rayOrigin);
        bool inShootRange = distance > enemyAI.attackRadius && distance <= shootRange;
        bool hasLineOfSight = false;
        if (inShootRange)
        {
            // Визуализируем луч в редакторе
            Debug.DrawRay(rayOrigin, directionToTarget * distance, Color.red, 0.1f);
            hasLineOfSight = Physics.Raycast(rayOrigin, directionToTarget, distance, enemyAI.obstacleMask);
        }
        if (Time.frameCount % 60 == 0) // Логируем каждую секунду
        {
            Debug.Log($"{name} - Проверка стрельбы: distance={distance:F1}, attackRadius={enemyAI.attackRadius:F1}, shootRange={shootRange:F1}, inShootRange={inShootRange}, hasLineOfSight={hasLineOfSight}, cooldownReady={Time.time >= lastShootTime + shootCooldown}, timeSinceLastShot={Time.time - lastShootTime:F1}");
        }
        if (inShootRange && hasLineOfSight && Time.time >= lastShootTime + shootCooldown)
        {
            Debug.Log($"{name} пытается выстрелить по игроку!");
            ShootAtPlayer(targetPos);
            lastShootTime = Time.time;
        }
        else if (inShootRange && !hasLineOfSight)
        {
            Debug.Log($"{name}: В радиусе стрельбы, но нет прямой видимости цели");
        }
        else if (!inShootRange)
        {
            Debug.Log($"{name}: Вне радиуса стрельбы (distance={distance:F1}, shootRange={shootRange:F1})");
        }
        else if (Time.time < lastShootTime + shootCooldown)
        {
            // Debug.Log($"{name}: Кулдаун стрельбы еще активен (осталось {lastShootTime + shootCooldown - Time.time:F1}с)");
        }
    }

    public static Vector3 CalculateLeadDirection(Vector3 pos, Vector3 targetPos, Vector3 targetVelocity, float bulletSpeed)
    {
        Vector3 toTarget = targetPos - pos;
        float a = Vector3.Dot(targetVelocity, targetVelocity) - bulletSpeed * bulletSpeed;
        float b = 2 * Vector3.Dot(targetVelocity, toTarget);
        float c = Vector3.Dot(toTarget, toTarget);

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
            return (targetPos - pos).normalized; // Не можем рассчитать упреждение — стреляем напрямую

        float t = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        if (t < 0) t = 0;
        Vector3 aimPoint = targetPos + targetVelocity * t;
        return (aimPoint - pos).normalized;
    }

    public void ShootAtPlayer(Vector3 targetPosition)
    {
        if (weapon == null || weapon.isKnife) return;
        // Проверка патронов
        if (weapon.ammoInClip <= 0)
        {
            if (weapon.ammoTotal > 0 && !isReloading)
            {
                isReloading = true;
                reloadTimer = weapon.reloadTime;
                Debug.Log($"{name}: Начал перезарядку! ammoInClip={weapon.ammoInClip}, ammoTotal={weapon.ammoTotal}");
            }
            else if (weapon.ammoTotal == 0)
            {
                Debug.Log($"{name}: Патроны закончились! ammoInClip={weapon.ammoInClip}, ammoTotal={weapon.ammoTotal}");
                if (enemyAI != null)
                    enemyAI.OnOutOfAmmo();
            }
            return;
        }
        weapon.ammoInClip--;
        Debug.Log($"{name} стреляет по позиции игрока: {targetPosition}. Осталось патронов в магазине: {weapon.ammoInClip}, всего: {weapon.ammoTotal}");
        Transform shootPoint = weapon.shootPoint != null ? weapon.shootPoint : transform;

        // --- Исправление: целимся в TargetLookPoint (голова/грудь), если есть ---
        Vector3 aimTarget = (enemyAI.targetLookPoint != null)
            ? enemyAI.targetLookPoint.position
            : enemyAI.target.position + Vector3.up * 1.5f;

        Rigidbody playerRb = enemyAI.target.GetComponent<Rigidbody>();
        Vector3 targetVelocity = playerRb != null ? playerRb.velocity : Vector3.zero;
        Vector3 leadDirection = CalculateLeadDirection(
            shootPoint.position,
            aimTarget,
            targetVelocity,
            weapon.bulletSpeed
        );
        float spread = weapon.spread;

        // Получаем шанс промаха из EnemyAI
        float missChance = enemyAI != null ? enemyAI.missChance : 0f;
        bool miss = Random.value < missChance;
        float missAngle = miss ? Random.Range(10f, 30f) : 0f; // Если промах, добавляем большой разброс
        float missSign = Random.value < 0.5f ? -1f : 1f;

        // Дробовик
        if (weapon.isShotgun)
        {
            int pelletCount = Mathf.Max(1, weapon.pelletCount);
            float damagePerPellet = enemyWeaponDamage / pelletCount;
            for (int i = 0; i < pelletCount; i++)
            {
                Vector3 pelletDir = Quaternion.Euler(
                    Random.Range(-spread, spread) + (miss ? missAngle * missSign : 0f),
                    Random.Range(-spread, spread) + (miss ? missAngle * missSign : 0f),
                    0) * leadDirection;
                FireProjectile(shootPoint, pelletDir, damagePerPellet);
            }
        }
        else
        {
            Vector3 dir = Quaternion.Euler(
                Random.Range(-spread, spread) + (miss ? missAngle * missSign : 0f),
                Random.Range(-spread, spread) + (miss ? missAngle * missSign : 0f),
                0) * leadDirection;
            FireProjectile(shootPoint, dir, enemyWeaponDamage);
        }

        // Визуальные эффекты
        if (weapon.muzzleFlash != null && weapon.muzzleFlash.Length > 0)
        {
            GameObject flash = Instantiate(
                weapon.muzzleFlash[Random.Range(0, weapon.muzzleFlash.Length)],
                shootPoint.position,
                Quaternion.identity
            );
        }
        // Звук
        if (weapon.shootClip != null && weapon.shootClip.Length > 0)
        {
            AudioSource.PlayClipAtPoint(weapon.shootClip[Random.Range(0, weapon.shootClip.Length)], shootPoint.position);
        }
        // Анимация
        if (enemyAI.anim != null)
        {
            enemyAI.anim.SetTrigger("Shoot");
        }
    }

    void FireProjectile(Transform shootPoint, Vector3 direction, float damage)
    {
        GameObject bulletPrefabToUse = weapon.enemyBulletPrefab != null ? weapon.enemyBulletPrefab : weapon.bulletPrefab;

        if (bulletPrefabToUse != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefabToUse,
                shootPoint.position,
                Quaternion.LookRotation(direction)
            );
            Debug.Log($"{name}: Создана пуля {bullet.name} в {shootPoint.position} с направлением {direction}");

            // Пробуем получить компонент Bullet или BulletEnemy
            var bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = damage;
                bulletScript.speed = weapon.bulletSpeed;
            }
            var bulletEnemyScript = bullet.GetComponent<BulletEnemy>();
            if (bulletEnemyScript != null)
            {
                bulletEnemyScript.SetParams(weapon.bulletSpeed, (int)damage);
                bulletEnemyScript.isShotgunPellet = weapon.isShotgun;
            }
        }
        else
        {
            Debug.LogWarning($"{name}: Нет префаба пули для Instantiate!");
            // Если префаба нет, используем raycast как запасной вариант
            RaycastHit hit;
            if (Physics.Raycast(shootPoint.position, direction, out hit, weapon.range))
            {
                PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage((int)damage);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}