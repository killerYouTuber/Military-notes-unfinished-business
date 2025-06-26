using UnityEngine;

/// <summary>
/// Генерирует звуковые события (шум) в зависимости от действий игрока.
/// Работает в связке с NoiseManager для оповещения врагов.
/// </summary>
[RequireComponent(typeof(GameMotor))]
public class PlayerNoiseEmitter : MonoBehaviour
{
    private GameMotor playerMotor;

    [Header("Настройки радиусов шума")]
    [Tooltip("Радиус шума при ходьбе")]
    public float walkNoiseRadius = 7f;
    [Tooltip("Радиус шума при беге")]
    public float runNoiseRadius = 15f;
    [Tooltip("Радиус шума от приземления после прыжка")]
    public float jumpLandingNoiseRadius = 10f;
    // В приседе шум не создается

    [Header("Настройки частоты шума")]
    [Tooltip("Интервал между 'шагами' (созданием шума) при ходьбе.")]
    public float walkStepInterval = 0.6f;
    [Tooltip("Интервал между 'шагами' (созданием шума) при беге.")]
    public float runStepInterval = 0.35f;
    
    private float noiseTimer;

    void Start()
    {
        playerMotor = GetComponent<GameMotor>();
    }

    void Update()
    {
        // Проверка шума от приземления (проверяется каждый кадр)
        if (playerMotor.JustLanded && !playerMotor.IsCrouching)
        {
            NoiseManager.EmitNoise(transform.position, jumpLandingNoiseRadius);
            Debug.Log($"[Noise] Создан шум от приземления с радиусом {jumpLandingNoiseRadius}");
        }

        // Проверяем, двигается ли игрок и не в приседе ли он
        if (playerMotor.CurrentHorizontalSpeed > 0.1f && !playerMotor.IsCrouching)
        {
            noiseTimer -= Time.deltaTime;

            if (noiseTimer <= 0)
            {
                // Если игрок бежит
                if (playerMotor.IsBoosting)
                {
                    NoiseManager.EmitNoise(transform.position, runNoiseRadius);
                    Debug.Log($"[Noise] Создан шум от БЕГА с радиусом {runNoiseRadius}");
                    noiseTimer = runStepInterval; // Сбрасываем таймер для бега
                }
                // Если игрок идет
                else
                {
                    NoiseManager.EmitNoise(transform.position, walkNoiseRadius);
                    Debug.Log($"[Noise] Создан шум от ХОДЬБЫ с радиусом {walkNoiseRadius}");
                    noiseTimer = walkStepInterval; // Сбрасываем таймер для ходьбы
                }
            }
        }
        else
        {
            // Если игрок не двигается, сбрасываем таймер, чтобы шум сработал сразу при начале движения
            noiseTimer = 0;
        }
    }
} 