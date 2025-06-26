using UnityEngine;

public class WeaponParametrs : MonoBehaviour
{
    [Header("Параметры оружия")]
    public GameObject bulletPrefab;
    [Tooltip("Префаб пули для врагов (будет использоваться вместо обычного bulletPrefab, когда оружие в руках у врага)")]
    public GameObject enemyBulletPrefab;
    public float bulletSpeed = 50f;
    public float damage = 10f;
    public float shootCoolDown = 0.1f;
    public float range = 500f;
    public float spread = 0.05f;

    [Header("Звуковые параметры")]
    [Tooltip("Будут ли враги реагировать на выстрелы из этого оружия")]
    public bool alertsEnemies = true;
    [Tooltip("Радиус слышимости выстрела")]
    public float shotSoundRadius = 30f;
    [Tooltip("Множитель для радиуса слышимости при использовании глушителя")]
    [Range(0f, 1f)]
    public float silencerMultiplier = 0.3f;
    [Tooltip("Установлен ли глушитель")]
    public bool hasSilencer = false;

    [Header("Боеприпасы")]
    public int ammoInClip = 30;
    public int clipSize = 30;
    public int ammoTotal = 90;
    public int initialTotalAmmo = 90; // Начальное количество патронов

    [Header("Эффекты")]
    public GameObject[] muzzleFlash;
    public AudioClip[] shootClip;
    public AudioClip reloadClip;

    [Header("Тип оружия")]
    public bool isKnife = false;
    
    [Header("Shotgun Settings")]
    public bool isShotgun = false;
    [Tooltip("Number of pellets fired in a single shot.")]
    public int pelletCount = 1;

    [Header("Время")]
    public float reloadTime = 1.5f;
    [HideInInspector] public float lastTimeShoot;
    public Transform shootPoint; // Точка выстрела пули

    [Header("Параметры стрельбы")]
    public int burstCount = 3;   // Количество выстрелов в очереди

    [Header("Префаб оружия (используется для идентификации типа)")]
    public WeaponParametrs weaponPrefab;

    [Header("Масштаб оружия в руках")]
    public Vector3 handScale = Vector3.one;

    [Header("Aiming Settings")]
    public Vector3 aimPosition;
    public Vector3 aimRotation;
    public float aimFOV = 40f;
    public float aimSpeed = 10f;
    public float aimSpread = 0.01f;

    void Start()
    {
        // Удалено: инициализация патронов теперь не происходит автоматически при создании экземпляра
        // ammoInClip = clipSize;
        // ammoTotal = initialTotalAmmo;
        // Debug.Log("[WeaponParametrs] Патроны инициализированы в Start(): " + ammoInClip + "/" + ammoTotal);
    }

    public void ResetAmmo()
    {
        ammoInClip = clipSize;
        ammoTotal = initialTotalAmmo;
        Debug.Log("[WeaponParametrs] Патроны сброшены: " + ammoInClip + "/" + ammoTotal);
    }

    [System.Serializable]
    public class WeaponParametersData
    {
        public GameObject bulletPrefab;
        public float bulletSpeed;
        public float damage;
        public float shootCoolDown;
        public float range;
        public float spread;
        public int ammoInClip;
        public int clipSize;
        public int ammoTotal;
        public int initialTotalAmmo;
        public bool isKnife;
        public float reloadTime;
        public int burstCount;
        public Vector3 handScale;
        public bool isShotgun;
        public int pelletCount;
        public Vector3 aimPosition;
        public Vector3 aimRotation;
        public float aimFOV;
        public float aimSpeed;
        public float aimSpread;

        public WeaponParametersData(WeaponParametrs source)
        {
            bulletPrefab = source.bulletPrefab;
            bulletSpeed = source.bulletSpeed;
            damage = source.damage;
            shootCoolDown = source.shootCoolDown;
            range = source.range;
            spread = source.spread;
            ammoInClip = source.ammoInClip;
            clipSize = source.clipSize;
            ammoTotal = source.ammoTotal;
            initialTotalAmmo = source.initialTotalAmmo;
            isKnife = source.isKnife;
            reloadTime = source.reloadTime;
            burstCount = source.burstCount;
            handScale = source.handScale;
            isShotgun = source.isShotgun;
            pelletCount = source.pelletCount;
            aimPosition = source.aimPosition;
            aimRotation = source.aimRotation;
            aimFOV = source.aimFOV;
            aimSpeed = source.aimSpeed;
            aimSpread = source.aimSpread;
        }
    }
}