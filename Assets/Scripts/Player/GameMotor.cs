using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

public class GameMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    public float speed = 3f;
    private bool isGrounded;
    public float gravity = -9.8f;
    public float jumpHeight = 2.0f;
    [SerializeField]
    private float sprintMult = 2.5f;
    
    [Header("Настройки инерции и направления")]
    [Tooltip("Как быстро персонаж набирает скорость.")]
    public float acceleration = 10f;
    [Tooltip("Как быстро персонаж останавливается.")]
    public float deceleration = 20f;
    [Tooltip("Множитель скорости для движения назад.")]
    [Range(0.1f, 1f)]
    public float backwardSpeedMultiplier = 0.6f;
    [Tooltip("Множитель скорости для движения вбок (стрейф).")]
    [Range(0.1f, 1f)]
    public float strafeSpeedMultiplier = 0.8f;

    [SerializeField]
    private float maxSprintTime = 2f; // Максимальное значение Sprint Time
    [SerializeField]
    private float sprintRegenRate = 0.5f; // Скорость восстановления Sprint Time
    private float currentSprintTime;
    
    [Header("Улучшенные настройки приседания")]
    [Tooltip("Высота персонажа стоя")]
    public float standingHeight = 1.8f;
    [Tooltip("Высота персонажа вприсядку")]
    public float crouchingHeight = 1.3f;
    [Tooltip("Множитель скорости при приседании")]
    public float crouchSpeedMultiplier = 0.5f;
    [Tooltip("Множитель высоты прыжка в присяде")]
    public float crouchJumpMultiplier = 0.7f;
    [Tooltip("Скорость перехода в режим приседания")]
    public float crouchTransitionSpeed = 10f;
    [Tooltip("Слой, который считается потолком (препятствием)")]
    public LayerMask ceilingLayer;
    [Tooltip("Радиус проверки потолка")]
    public float ceilingCheckRadius = 0.4f;
    
    private bool isBoosting = false;
    private float currentSpeed;
    private bool isCrouch = false;
    private bool isStaminaRecharging = false;

    private bool wasGrounded; // Для отслеживания приземления
    private GameLook gameLook;

    private Vector3 lastPosition;
    private float currentHorizontalSpeed;
    public float CurrentHorizontalSpeed => currentHorizontalSpeed;

    // Публичное свойство для доступа к состоянию приседания
    public bool IsCrouching => isCrouch;
    public bool IsBoosting => isBoosting;
    public bool JustLanded { get; private set; } // Станет true на 1 кадр после приземления

    // Публичное свойство для доступа к текущей скорости игрока
    public float CurrentVelocity => controller != null ? controller.velocity.magnitude : 0f;

    private float debugSpeedLogTimer = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        gameLook = GetComponent<GameLook>();
        currentSpeed = speed;
        currentSprintTime = maxSprintTime; // Инициализация при старте
        
        // Устанавливаем начальную высоту и центр контроллера
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2.0f, 0);
        lastPosition = transform.position;
    }

    void Update()
    {
        // Сбрасываем флаг приземления в начале каждого кадра
        JustLanded = false;

        isGrounded = controller.isGrounded;

        // Проверяем, приземлились ли мы в этом кадре
        if (!wasGrounded && isGrounded)
        {
            JustLanded = true;
            Debug.Log("Player has landed!");
        }

        wasGrounded = isGrounded; // Обновляем состояние для следующего кадра
        
        debugSpeedLogTimer -= Time.deltaTime;
        if (debugSpeedLogTimer <= 0f)
        {
            Debug.Log($"[Player] Скорость: {CurrentVelocity:F2}");
            debugSpeedLogTimer = 1f;
        }
        if (isBoosting)
        {
            currentSprintTime -= Time.deltaTime;
            if (currentSprintTime <= 0)
            {
                StopBoost();
            }
        }
        else // Восстановление Sprint Time, когда игрок не бежит
        {
            if (currentSprintTime < maxSprintTime)
            {
                currentSprintTime += Time.deltaTime * sprintRegenRate;
                currentSprintTime = Mathf.Min(currentSprintTime, maxSprintTime);
            }
        }
        
        // Плавная обработка изменения высоты при приседании
        HandleCrouchTransition();

        // В конце Update вычисляем горизонтальную скорость
        float horizontalSpeed = Vector3.ProjectOnPlane((transform.position - lastPosition) / Time.deltaTime, Vector3.up).magnitude;
        lastPosition = transform.position;
        currentHorizontalSpeed = horizontalSpeed;
    }

    public void ProcessMove(Vector2 input)
    {
        // --- 1. Определение целевой базовой скорости ---
        float baseSpeed = isCrouch ? speed * crouchSpeedMultiplier : speed;
        if (isBoosting && !isCrouch && currentSprintTime > 0)
        {
            baseSpeed *= sprintMult;
        }

        // --- 2. Применение множителей направления ---
        float directionalMultiplier = 1.0f;
        if (input.y < 0) // Движение назад
        {
            directionalMultiplier = backwardSpeedMultiplier;
        }
        else if (Mathf.Abs(input.x) > 0.01f) // Движение вбок (имеет приоритет ниже, чем движение назад)
        {
            directionalMultiplier = strafeSpeedMultiplier;
        }
        
        float finalTargetSpeed = baseSpeed * directionalMultiplier;

        // --- 3. Плавное изменение текущей скорости (инерция) ---
        float effectiveTargetSpeed = (input.magnitude > 0) ? finalTargetSpeed : 0;
        float currentAcceleration = (input.magnitude > 0) ? acceleration : deceleration;

        currentSpeed = Mathf.MoveTowards(currentSpeed, effectiveTargetSpeed, currentAcceleration * Time.deltaTime);

        // --- 4. Применение движения ---
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;

        controller.Move(transform.TransformDirection(moveDirection) * currentSpeed * Time.deltaTime);
        
        // --- 5. Гравитация ---
        playerVelocity.y += gravity * Time.deltaTime;
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    public void ProcessJump()
    {
        if(isGrounded)
        {
            float currentJumpHeight = isCrouch ? jumpHeight * crouchJumpMultiplier : jumpHeight;
            playerVelocity.y = Mathf.Sqrt(currentJumpHeight * -1.0f * gravity);
        }
    }

    public void ProccessSprint()
    {
        // Запрещаем спринт если игрок присел или стамина не восстановилась полностью
        if (!isBoosting && isGrounded && !isCrouch && !isStaminaRecharging)
        {
            StartBoost();
        }
    }

    private void StartBoost()
    {
        isBoosting = true;
    }

    private void StopBoost()
    {
        isBoosting = false;
    }

    public void ResetBoost()
    {
        if (isBoosting)
        {
            StopBoost();
        }
    }

    /// <summary>
    /// Основной метод для переключения режима приседания.
    /// Вызывается из InputManager.
    /// </summary>
    public void ToggleCrouch()
    {      
        if (isCrouch)
        {
            // Пытаемся встать. Сначала проверка потолка.
            // Точка для проверки находится в верхней части стоячего коллайдера
            Vector3 checkPoint = transform.position + new Vector3(0, crouchingHeight + 0.1f, 0);
            if (Physics.CheckSphere(checkPoint, ceilingCheckRadius, ceilingLayer, QueryTriggerInteraction.Ignore))
            {
                // Над нами потолок, встать не можем.
                Debug.Log("Невозможно встать: над головой препятствие.");
                return; // Выходим, оставляя isCrouch = true
            }
        }
        
        // Если проверки не было или она прошла успешно, меняем состояние
        isCrouch = !isCrouch;

        // Отключаем спринт при приседании
        if (isCrouch && isBoosting)
        {
            StopBoost();
        }

        if (gameLook != null)
        {
            gameLook.SetCrouchCamera(isCrouch);
    }
    }

    /// <summary>
    /// Плавно изменяет высоту и центр CharacterController в Update.
    /// </summary>
    private void HandleCrouchTransition()
    {
        float targetHeight = isCrouch ? crouchingHeight : standingHeight;
        Vector3 targetCenter = new Vector3(0, targetHeight / 2.0f, 0);

        // Плавное изменение высоты и центра
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchTransitionSpeed);
    }

    // Метод для получения текущего значения выносливости
    public float GetCurrentSprintTime()
    {
        return currentSprintTime;
    }

    // Метод для установки значения выносливости (для загрузки игры)
    public void SetCurrentSprintTime(float value)
    {
        currentSprintTime = value;
    }

    // Метод для получения текущего состояния приседания
    public bool GetIsCrouching()
    {
        return isCrouch;
    }

    // Метод для установки состояния приседания (для загрузки игры)
    public void SetCrouchingState(bool value)
    {
        // Гарантируем, что controller инициализирован
        if (controller == null)
            controller = GetComponent<CharacterController>();

        isCrouch = value;

        // Мгновенно устанавливаем высоту и центр, без плавной анимации
        float targetHeight = isCrouch ? crouchingHeight : standingHeight;
        Vector3 targetCenter = new Vector3(0, targetHeight / 2.0f, 0);

        controller.height = targetHeight;
        controller.center = targetCenter;

        if (gameLook != null)
        {
            gameLook.SetCrouchCamera(isCrouch);
        }
    }
}

