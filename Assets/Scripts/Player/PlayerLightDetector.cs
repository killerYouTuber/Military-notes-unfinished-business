using UnityEngine;

/// <summary>
/// Автоматически определяет, находится ли игрок на свету или в тени.
/// </summary>
public class PlayerLightDetector : MonoBehaviour
{
    [Tooltip("Находится ли игрок в данный момент на свету? (Только для чтения)")]
    public bool IsInLight { get; private set; } = true;

    [Header("Настройки авто-определения")]
    [Tooltip("Слой с объектами, которые отбрасывают тени (стены, ящики и т.д.). Должен совпадать с ObstacleMask у врагов.")]
    public LayerMask obstacleMask;
    
    [Tooltip("Главный источник света в сцене (обычно Directional Light). Если не указан, будет найден автоматически.")]
    public Light sunSource;

    [Tooltip("Как часто (в секундах) проверять освещенность. Меньшее значение = точнее, но более ресурсоемко.")]
    public float checkInterval = 0.2f;

    [Tooltip("Пороговое значение для окружающего света. Если интенсивность окружающего света выше этого значения, считается, что игрок 'на свету' даже в тени от солнца.")]
    [Range(0, 8)]
    public float ambientLightThreshold = 0.5f;

    [Header("Дополнительные источники света")]
    [Tooltip("Включить автоматический поиск и проверку всех источников света (лампы, прожекторы и т.д.)")]
    public bool checkAllLights = true;
    
    [Tooltip("Максимальная дистанция для проверки источников света")]
    public float maxLightDistance = 50f;
    
    [Tooltip("Минимальная интенсивность источника света для его учёта")]
    [Range(0, 8)]
    public float minLightIntensity = 0.1f;

    private float checkTimer;
    private Light[] allLights;
    private Vector3 lastCheckedPosition;
    public float positionCheckDistance = 0.1f;

    void Start()
    {
        // Попытка найти главный источник света (солнце), если он не назначен вручную
        if (sunSource == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sunSource = light;
                    Debug.Log("PlayerLightDetector: найден главный источник света - " + sunSource.name);
                    break;
                }
            }
        }

        // Находим все источники света для проверки
        if (checkAllLights)
        {
            FindAllLights();
        }

        lastCheckedPosition = transform.position;
    }

    private void FindAllLights()
    {
        allLights = FindObjectsOfType<Light>();
        Debug.Log($"PlayerLightDetector: найдено {allLights.Length} источников света");
        
        // Выводим информацию о найденных источниках
        for (int i = 0; i < allLights.Length; i++)
        {
            Light light = allLights[i];
            if (light.intensity >= minLightIntensity)
            {
                Debug.Log($"Источник света {i + 1}: {light.name} (тип: {light.type}, интенсивность: {light.intensity:F2})");
            }
        }
    }

    void Update()
    {
        checkTimer -= Time.deltaTime;
        float movedDistance = (transform.position - lastCheckedPosition).sqrMagnitude;
        if (movedDistance > positionCheckDistance * positionCheckDistance)
        {
            lastCheckedPosition = transform.position;
            checkTimer = checkInterval; // сбрасываем таймер
            CheckLightStatus();
        }
        else if (checkTimer <= 0)
        {
            checkTimer = checkInterval;
            CheckLightStatus();
        }
    }

    private void CheckLightStatus()
    {
        float ambient = RenderSettings.ambientIntensity;
        // 1. Проверка на общий уровень освещенности сцены
        if (ambient >= ambientLightThreshold)
        {
            IsInLight = false;
            // 2. Если ambient выше порога — проверяем прямой свет от Directional Light
            if (sunSource != null)
            {
                Ray ray = new Ray(transform.position, -sunSource.transform.forward);
                if (!Physics.Raycast(ray, 1000f, obstacleMask))
                {
                    IsInLight = true;
                    Debug.Log($"[PlayerLightDetector] Освещённость: {ambient:F2} (порог: {ambientLightThreshold}) — СВЕТ (ambient + солнце)");
                    return;
                }
            }
        }
        // 3. Проверка на свет от других источников (лампы, прожекторы и т.д.)
        if (checkAllLights && allLights != null)
        {
            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light == sunSource || !light.enabled || light.intensity < minLightIntensity)
                    continue;
                float distanceToLight = Vector3.Distance(transform.position, light.transform.position);
                if (distanceToLight <= maxLightDistance)
                {
                    Vector3 directionToLight = (light.transform.position - transform.position).normalized;
                    if (!Physics.Raycast(transform.position, directionToLight, distanceToLight, obstacleMask))
                    {
                        if (light.type == LightType.Spot)
                        {
                            float angle = Vector3.Angle(-light.transform.forward, directionToLight);
                            if (angle <= light.spotAngle / 2f)
                            {
                                IsInLight = true;
                                Debug.Log($"[PlayerLightDetector] Освещённость: {ambient:F2} (порог: {ambientLightThreshold}) — СВЕТ (ambient + прожектор: {light.name})");
                                return;
                            }
                        }
                        else if (light.type == LightType.Point || light.type == LightType.Area)
                        {
                            IsInLight = true;
                            Debug.Log($"[PlayerLightDetector] Освещённость: {ambient:F2} (порог: {ambientLightThreshold}) — СВЕТ (ambient + лампа: {light.name})");
                            return;
                        }
                    }
                }
            }
        }
        // Если ни одно из условий не выполнено — считаем, что игрок в тени
        IsInLight = false;
        Debug.Log($"[PlayerLightDetector] Освещённость: {ambient:F2} (порог: {ambientLightThreshold}) — ТЕНЬ (нет достаточного ambient и нет прямого света)");
    }

    // Для отладки
    private void OnDrawGizmosSelected()
    {
        if (sunSource != null)
        {
            Gizmos.color = IsInLight ? Color.yellow : Color.blue;
            Vector3 startPoint = transform.position;
            Vector3 direction = -sunSource.transform.forward * 5;
            Gizmos.DrawRay(startPoint, direction);
            Gizmos.DrawSphere(startPoint + direction, 0.2f);
        }

        // Отображаем лучи к другим источникам света
        if (checkAllLights && allLights != null)
        {
            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];
                if (light == sunSource || !light.enabled || light.intensity < minLightIntensity)
                    continue;

                float distanceToLight = Vector3.Distance(transform.position, light.transform.position);
                if (distanceToLight <= maxLightDistance)
                {
                    Gizmos.color = IsInLight ? Color.yellow : Color.blue;
                    Gizmos.DrawLine(transform.position, light.transform.position);
                    Gizmos.DrawSphere(light.transform.position, 0.3f);
                }
            }
        }
    }
} 