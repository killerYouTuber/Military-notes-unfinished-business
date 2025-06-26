using UnityEngine.AI;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    // ================== Базовые настройки ==================
    [Header("Базовые настройки")]
    public Transform target;
    public NavMeshAgent agent;
    public Animator anim;
    public bool isAggressive = false;
    [Tooltip("Точка, куда враг будет смотреть (например, голова игрока)")]
    public Transform targetLookPoint;

    // ================== Бой ==================
    [Header("Бой")]
    public float attackRadius = 2f;
    public int attackDamage = 10;
    public float attackRate = 1f;
    protected float lastAttackTime;

    // ================== Зрение (основное) ==================
    [Header("Зрение (основное)")]
    [Tooltip("Дальность зрения на свету")]
    public float visionRange = 20f;
    [Tooltip("Горизонтальный угол зрения на свету")]
    [Range(0, 360)]
    public float visionAngle = 90f;
    [Tooltip("Время реакции при обнаружении на свету")]
    public float reactionTimeLight = 1.5f;

    // ================== Зрение (в темноте) ==================
    [Header("Зрение (в темноте)")]
    [Tooltip("Дальность зрения в темноте")]
    public float darkVisionRange = 8f;
    [Tooltip("Горизонтальный угол зрения в темноте")]
    [Range(0, 360)]
    public float darkVisionAngle = 120f;
    [Tooltip("Время реакции при обнаружении в темноте")]
    public float reactionTimeDark = 2.5f;

    // ================== Зрение (3D) ==================
    [Header("Зрение (3D)")]
    [Tooltip("Вертикальный угол зрения (только если используется eye)")]
    [Range(0,180)]
    public float verticalFOV = 60f;

    // ================== Логика обнаружения ==================
    [Header("Логика обнаружения")]
    [Tooltip("Время в секундах, которое игрок должен находиться в поле зрения, чтобы враг среагировал.")]
    public float reactionTime = 1.5f;
    [Tooltip("Слой препятствий для зрения")]
    public LayerMask obstacleMask;
    [Tooltip("Точка, из которой смотрит враг")]
    public Transform eye;

    // ================== Слух ==================
    [Header("Слух")]
    [Tooltip("Радиус реакции на смерть союзника")]
    public float allyDeathReactionRadius = 20f;
    // [Tooltip("Минимальная скорость игрока для слышимости (ходьба/бег)")] // УСТАРЕЛО
    // public float requiredMoveSpeed = 1f;
    // [Tooltip("Минимальная скорость для любого шума (игнорировать микродвижения)")] // УСТАРЕЛО
    // public float minMoveForNoise = 0.2f;
    [Tooltip("Время расследования после шума")]
    public float investigationTime = 5f;

    // ================== Агрессия при уроне ==================
    [Header("Агрессия при уроне")]
    [Tooltip("Радиус мгновенной агрессии")]
    public float aggroRadiusOnDamage = 10f;
    [Tooltip("Радиус расследования при уроне")]
    public float investigateRadiusOnDamage = 20f;

    // ================== Расследование ==================
    [Header("Расследование")]
    [Tooltip("Время пребывания у точки расследования")]
    public float investigationStayTime = 3f;
    [Tooltip("Время осматривания на точке расследования")]
    public float investigationLookDuration = 2f;
    private float stayTimer = 0f;
    private Vector3 initialPosition;

    private float detectionProgress = 0f;
    private PlayerLightDetector playerLightDetector;
    private float smokeBlindnessTimer = 0f;
    private GameMotor playerMotor; // Ссылка на компонент игрока

    // Переменные для новой логики состояний
    public enum State { Idle, Chase, Attack, Investigate, Patrol }
    private State currentState;
    private Vector3 lastHeardPosition;
    private float investigationTimer;

    // ================== Система укрытий ==================
    [Header("Система укрытий")]
    public float coverSearchRadius = 15f;
    public float peekTime = 1.5f;
    public float hideTime = 2f;
    private CoverPoint currentCover;
    private bool inCover = false;
    private bool isPeeking = false;
    private float coverTimer = 0f;
    private Vector3 coverOriginalPos;
    private Vector3 peekOffset = new Vector3(0.5f, 0, 0); // Смещение для выглядывания

    // ================== Выглядывание из низких укрытий ==================
    [Header("Выглядывание из низких укрытий")]
    public float crouchHeightOffset = -0.7f;
    public float peekHeightOffset = 0.0f;
    public float peekDurationLow = 1.2f;
    public float hideDurationLow = 1.5f;
    public float peekSpeedLow = 3f;
    private float peekTimerLow = 0f;
    private bool isPeekingLow = false;
    private Vector3 basePositionLow;

    // ================== Патрулирование ==================
    [Header("Патрулирование")]
    public List<PatrolPoint> patrolPoints;
    [Tooltip("На каком расстоянии от точки патруля считать, что она достигнута.")]
    public float patrolPointArrivalThreshold = 0.5f;
    private int patrolIndex = 0;
    private float patrolWaitTimer = 0f;
    private bool isPatrolling = true;
    private bool wasInvestigating = false;
    private bool doorWasClosed = false;
    private DoorInteractable lastDoor;

    // ================== Перебег между укрытиями ==================
    [Header("Перебег между укрытиями")]
    public float coverRunSpeed = 5f;

    // ================== AI: Настройки точности ==================
    [Header("AI: Настройки точности")]
    [Range(0,1)]
    public float missChance = 0.2f;

    // Для осматривания в Investigate
    private float investigationLookTimer = 0f;
    private float investigationLookAngle = 0f;
    private bool investigationLookActive = false;

    // ================== Реакция на звуки ==================
    [Header("Реакция на звуки")]
    [Tooltip("Множитель дистанции реакции на взрыв")]
    public float explosionHearingMultiplier = 2f;
    [Tooltip("Время расследования после взрыва")]
    public float explosionInvestigationTime = 10f;
    [Tooltip("Радиус мгновенной агрессии при взрыве")]
    public float explosionAggroRadius = 15f;

    // ================== Время реакции (прочее) ==================
    [Header("Время реакции (прочее)")]
    [Tooltip("Время реакции в состоянии расследования")]
    public float investigateReactionTime = 2.5f;

    private EnemyShooting enemyShooting;
    private float strafeTimer = 0f;
    private float strafeDuration = 1.2f;
    private int strafeDirection = 0; // -1 влево, 1 вправо, 0 — не двигается
    private float preferredShootDistance = 0.85f; // 85% от shootRange

    [Header("Оружие ближнего боя (нож)")]
    [Tooltip("WeaponParametrs для ножа. Назначьте сюда prefab ножа с isKnife = true.")]
    public WeaponParametrs knifeWeapon;

    [Header("Передвижение врага")]
    [Tooltip("Скорость в состоянии покоя (Idle)")]
    public float idleSpeed = 2.5f;
    [Tooltip("Скорость в состоянии расследования (Investigate)")]
    public float investigateSpeed = 4.0f;
    [Tooltip("Скорость в состоянии погони (Chase)")]
    public float chaseSpeed = 5.5f;
    [Tooltip("Скорость патрулирования")]
    public float patrolSpeed = 3.0f;

    [Header("Сохранение")]
    [Tooltip("Уникальный ID для сохранения. Генерируется автоматически.")]
    public string uniqueID;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

    void OnEnable()
    {
        NoiseManager.RegisterListener(this);
    }

    void OnDisable()
    {
        NoiseManager.UnregisterListener(this);
    }

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        agent.stoppingDistance = attackRadius;
        agent.speed = idleSpeed; // Начальная скорость — как в Idle
        enemyShooting = GetComponent<EnemyShooting>();

        TryAssignTarget();

        if (eye == null)
        {
            eye = transform;
        }
        initialPosition = transform.position;
        SwitchState(State.Idle); // Начальное состояние
    }

    protected virtual void Update()
    {
        // Если игра на паузе — не обновляем таймеры и не выполняем логику AI
        if (PauseManager.IsPaused)
            return;
        // Если target не назначен — пробуем ещё раз (например, если игрок появился позже)
        if (target == null)
        {
            TryAssignTarget();
            if (target == null) return;
        }

        // Реалистичный поворот головы
        Vector3 lookDir = GetLookDirection();
        if (lookDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
        }

        if (smokeBlindnessTimer > 0)
        {
            smokeBlindnessTimer -= Time.deltaTime;
        }

        HandleVision();
        HandleHearing();
        UpdateState();
        CheckForDoors();
    }

    private void TryAssignTarget()
    {
        if (target == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            target = PlayerManager.instance.player.transform;
            playerLightDetector = target.GetComponent<PlayerLightDetector>();
            playerMotor = target.GetComponent<GameMotor>();
            // --- Автоматический поиск точки взгляда ---
            if (targetLookPoint == null && target != null)
            {
                var lookPoint = target.Find("TargetLookPoint");
                if (lookPoint != null)
                    targetLookPoint = lookPoint;
            }
        }
    }

    protected virtual void HandleHearing()
    {
        // Вся логика слуха теперь основана на событиях
        // и обрабатывается в OnNoiseHeard, OnShotHeard и OnExplosionHeard.
        // Этот метод больше не нужен.
    }

    protected virtual void HandleVision()
    {
        if (PauseManager.IsPaused) return;
        // Убираем проверку isAggressive, чтобы враг продолжал видеть игрока
        // if (isAggressive) return;

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            detectionProgress += Time.deltaTime;
            float currentReactionTime = GetCurrentReactionTime();

            if (detectionProgress >= currentReactionTime)
            {
                OnPlayerSpotted();
            }
        }
        else
        {
            detectionProgress = Mathf.Max(0, detectionProgress - Time.deltaTime / 2);
        }
    }

    private float GetCurrentReactionTime()
    {
        if (currentState == State.Investigate)
        {
            return investigateReactionTime;
        }
        
        bool isPlayerInDark = playerLightDetector != null && !playerLightDetector.IsInLight;
        return isPlayerInDark ? reactionTimeDark : reactionTimeLight;
    }

    protected virtual void OnPlayerSpotted()
    {
        lastHeardPosition = target.position;
        isAggressive = true;
        
        // Если враг уже в состоянии атаки, не переключаемся в погоню
        if (currentState != State.Attack)
        {
            SwitchState(State.Chase);
        }
    }
    protected virtual bool CanSeePlayer()
    {
        if (smokeBlindnessTimer > 0) return false;
        
        bool isPlayerInDark = playerLightDetector != null && !playerLightDetector.IsInLight;
        float currentVisionRange = isPlayerInDark ? darkVisionRange : visionRange;
        float currentVisionAngle = isPlayerInDark ? darkVisionAngle : visionAngle;

        return IsTargetInCone(target, currentVisionRange, currentVisionAngle);
    }
    private bool IsTargetInCone(Transform targetToCheck, float range, float angle)
    {
        if (targetToCheck == null) return false;

        Vector3 startPoint = eye != null ? eye.position : transform.position;
        Vector3 targetPoint = targetToCheck.position; // Центр игрока

        float distanceToTarget = Vector3.Distance(startPoint, targetPoint);

        if (distanceToTarget > range)
        {
            return false;
        }

        Vector3 directionToTarget = (targetPoint - startPoint).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        if (angleToTarget > angle / 2)
        {
            return false; // За пределами горизонтального угла
        }

        // Проверка по вертикали (опционально)
        if (eye != null && verticalFOV < 180)
        {
            float verticalAngle = Vector3.Angle(transform.forward, new Vector3(directionToTarget.x, 0, directionToTarget.z));
            if (verticalAngle > verticalFOV / 2)
            {
                // return false; // За пределами вертикального угла, пока выключено
            }
        }

        if (Physics.Raycast(startPoint, directionToTarget, distanceToTarget, obstacleMask))
        {
            return false; // Есть препятствие
        }

        return true;
    }


    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chase:
                ChaseTarget();
                break;
            case State.Attack:
                AttackLogic();
                break;
            case State.Investigate:
                Investigate();
                break;
            case State.Patrol:
                Patrol();
                break;
        }
    }

    private void SwitchState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case State.Idle:
                isAggressive = false;
                agent.isStopped = true;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
                agent.speed = idleSpeed;
                break;
            case State.Chase:
                agent.isStopped = false;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", true);
                agent.speed = chaseSpeed;
                break;
            case State.Attack:
                agent.isStopped = true;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", false);
                break;
            case State.Investigate:
                agent.isStopped = false;
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
                agent.speed = investigateSpeed;
                agent.SetDestination(lastHeardPosition);
                investigationTimer = investigationTime;
                wasInvestigating = true;
                stayTimer = 0f;
                investigationLookActive = false;
                break;
            case State.Patrol:
                Patrol();
                break;
        }
    }


    protected virtual void ChaseTarget()
    {
        agent.autoBraking = true; // Убедимся, что агент будет тормозить для атаки
        if (target == null) return;

        agent.isStopped = false;
        agent.SetDestination(target.position);
        if (Vector3.Distance(transform.position, target.position) <= attackRadius)
        {
            SwitchState(State.Attack);
        }
    }

    protected virtual void Idle()
    {
        if (patrolPoints != null && patrolPoints.Count > 0 && isPatrolling)
        {
            Patrol();
        }
        else
        {
            // Логика закрытия двери после расследования
            if (lastDoor != null && doorWasClosed)
            {
                // Если мы далеко от двери, идем к ней
                if (Vector3.Distance(transform.position, lastDoor.transform.position) > 2f)
                {
                    agent.SetDestination(lastDoor.transform.position);
                }
                else // Если мы у двери, закрываем ее
                {
                    lastDoor.Interact();
                    doorWasClosed = false;
                    lastDoor = null;
                }
                return; // Не даем вернуться на исходную позицию, пока не закроем дверь
            }

            // Можно добавить логику для возвращения на исходную позицию
            if (wasInvestigating)
            {
                agent.SetDestination(initialPosition);
                if (Vector3.Distance(transform.position, initialPosition) < 1.0f)
                {
                    wasInvestigating = false;
                }
            }
        }
    }

    protected virtual void AttackLogic()
    {
        if (enemyShooting != null && target != null)
        {
            float shootRange = enemyShooting.shootRange;
            float dist = Vector3.Distance(transform.position, target.position);
            float minDist = shootRange * 0.8f;
            float maxDist = shootRange * 0.95f;

            // Если игрок слишком далеко — подходим к границе радиуса
            if (dist > maxDist)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position + (transform.position - target.position).normalized * maxDist);
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
                strafeDirection = 0;
                return;
            }
            // Если игрок слишком близко — отходим
            if (dist < minDist)
            {
                Vector3 away = (transform.position - target.position).normalized;
                Vector3 newPos = transform.position + away * (minDist - dist + 0.5f);
                agent.isStopped = false;
                agent.SetDestination(newPos);
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
                strafeDirection = 0;
                return;
            }
            // Игрок в оптимальном радиусе — стреляем и двигаемся зигзагом
            agent.isStopped = false;
            Vector3 moveDir = Vector3.zero;
            // Случайно выбираем направление для зигзага
            strafeTimer -= Time.deltaTime;
            if (strafeTimer <= 0)
            {
                strafeTimer = Random.Range(0.7f, strafeDuration);
                strafeDirection = Random.Range(-1, 2); // -1, 0, 1
            }
            // Двигаемся в сторону (зигзаг), но медленно
            if (strafeDirection != 0)
            {
                Vector3 right = Vector3.Cross(Vector3.up, (target.position - transform.position).normalized);
                moveDir += right * strafeDirection * 0.5f; // 0.5 — медленно
            }
            // Немного двигаемся к игроку
            moveDir += (target.position - transform.position).normalized * 0.2f;
            Vector3 dest = transform.position + moveDir;
            agent.SetDestination(dest);
            anim.SetBool("isWalking", true);
            anim.SetBool("isRunning", false);
            // Стреляем
            // enemyShooting.ShootAtPlayer(target.position); // УБРАНО: теперь стрельба только по таймеру в EnemyShooting
            LookTarget();
            return;
        }
        // Старое поведение для врагов без огнестрела
        if (Vector3.Distance(transform.position, target.position) > attackRadius)
        {
            SwitchState(State.Chase);
            return;
        }
        LookTarget();
        TryAttack();
    }

    protected virtual void Investigate()
    {
        agent.autoBraking = true; // Убедимся, что агент будет тормозить у точки расследования
        wasInvestigating = true;

        float dist = Vector3.Distance(transform.position, lastHeardPosition);
        // Если мы дошли до точки расследования
        if (agent.remainingDistance < agent.stoppingDistance)
        {
            stayTimer += Time.deltaTime;
            anim.SetBool("isWalking", false); // Остановились

            // Если только что прибыли, начинаем осматриваться
            if (!investigationLookActive)
            {
                investigationLookActive = true;
                investigationLookTimer = investigationLookDuration;
                investigationLookAngle = Random.Range(45, 90) * (Random.value > 0.5f ? 1 : -1);
            }

            // Процесс осматривания
            if (investigationLookActive && investigationLookTimer > 0)
            {
                 investigationLookTimer -= Time.deltaTime;
                 Quaternion lookRotation = Quaternion.LookRotation(agent.transform.forward);
                 Quaternion turnRotation = Quaternion.AngleAxis(investigationLookAngle * Mathf.Sin(Time.time * 2f), Vector3.up);
                 transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation * turnRotation, Time.deltaTime * 2f);
            }
            else
            {
                investigationLookActive = false; // Осматривание закончено
            }

            // Если провели достаточно времени на точке, возвращаемся в Idle
            if (stayTimer >= investigationStayTime)
            {
                SwitchState(State.Idle);
            }
        }
        else
        {
             anim.SetBool("isWalking", true); // Все еще идем
        }

        // Если время на расследование вышло, возвращаемся в Idle
        investigationTimer -= Time.deltaTime;
        if (investigationTimer <= 0)
        {
            SwitchState(State.Idle);
        }
    }
    
    protected virtual void TryAttack()
    {
        if (Time.time > lastAttackTime + (1f / attackRate))
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        //GetComponent<EnemyShooting>().Shoot(missChance);
        Debug.Log("Атака!");
    }


    protected void LookTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    private void OnDrawGizmos()
    {
        // Рисуем поле зрения
        bool isPlayerInDark = playerLightDetector != null && !playerLightDetector.IsInLight;
        float currentVisionRange = isPlayerInDark ? darkVisionRange : visionRange;
        float currentVisionAngle = isPlayerInDark ? darkVisionAngle : visionAngle;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentVisionRange);

        Vector3 fovLine1 = Quaternion.AngleAxis(currentVisionAngle / 2, transform.up) * transform.forward * currentVisionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-currentVisionAngle / 2, transform.up) * transform.forward * currentVisionRange;
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);

        // Показываем прогресс обнаружения
        if (detectionProgress > 0)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(1, 1, 0, 0.2f);
            UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, fovLine2, currentVisionAngle * (detectionProgress / GetCurrentReactionTime()), currentVisionRange);
#endif
        }

        // Рисуем радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);

        // Визуализация оптимальной точки радиуса стрельбы
        if (enemyShooting != null && target != null)
        {
            float shootRange = enemyShooting.shootRange;
            float preferredDist = shootRange * preferredShootDistance;
            Vector3 dirToPlayer = (target.position - transform.position).normalized;
            Vector3 optimalPoint = transform.position + dirToPlayer * preferredDist;
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(optimalPoint, 0.3f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, optimalPoint);
        }
    }


    public void ApplySmokeBlindness(float duration)
    {
        smokeBlindnessTimer = duration;
    }

    public void Alert(Vector3 position)
    {
        if (currentState != State.Chase && currentState != State.Attack)
        {
            lastHeardPosition = position;
            SwitchState(State.Investigate);
        }
    }

    public bool IsIdle()
    {
        return currentState == State.Idle || currentState == State.Investigate;
    }

    public void OnDamagedByPlayer(Vector3 damageSourcePosition)
    {
        // Если уже агрессивны, ничего не делаем
        if (isAggressive) return;

        // Если урон нанесен вблизи, становимся агрессивными
        if (Vector3.Distance(transform.position, damageSourcePosition) <= aggroRadiusOnDamage)
        {
            lastHeardPosition = damageSourcePosition;
            isAggressive = true;
            SwitchState(State.Chase);
        }
        // Иначе, если урон нанесен на расстоянии, идем расследовать
        else if (Vector3.Distance(transform.position, damageSourcePosition) <= investigateRadiusOnDamage)
        {
            lastHeardPosition = damageSourcePosition;
            SwitchState(State.Investigate);
        }
    }

    void TryFindAndMoveToCover()
    {
        if (inCover) return;

        var coverPoints = FindObjectsOfType<CoverPoint>()
            .Where(p => Vector3.Distance(p.transform.position, transform.position) < coverSearchRadius && !p.isOccupied)
            .OrderBy(p => Vector3.Distance(p.transform.position, transform.position))
            .ToList();

        if (coverPoints.Any())
        {
            currentCover = coverPoints.First();
            currentCover.isOccupied = true;
            inCover = true;
            agent.SetDestination(currentCover.transform.position);
            coverTimer = hideTime; // Начинаем с укрытия
            isPeeking = false;

            if (currentCover.coverType == CoverType.Low)
            {
                 basePositionLow = currentCover.transform.position;
                 SetCrouchPosition();
            }
        }
    }

    void HandleCoverLogic()
    {
        if (!inCover || currentCover == null)
        {
            SwitchState(State.Chase);
            return;
        }

        coverTimer -= Time.deltaTime;

        if (currentCover.coverType == CoverType.Low)
        {
            // Логика для низких укрытий
            if (isPeekingLow)
            {
                // Выглядываем
                float targetY = basePositionLow.y + peekHeightOffset;
                Vector3 peekPos = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * peekSpeedLow), transform.position.z);
                transform.position = peekPos;

                if (coverTimer <= 0)
                {
                    isPeekingLow = false;
                    coverTimer = hideDurationLow; // Время спрятаться
                }
            }
            else
            {
                // Прячемся
                float targetY = basePositionLow.y + crouchHeightOffset;
                Vector3 hidePos = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * peekSpeedLow), transform.position.z);
                transform.position = hidePos;

                if (coverTimer <= 0)
                {
                    isPeekingLow = true;
                    coverTimer = peekDurationLow; // Время выглядывать
                }
            }
        }
        else
        {
            // Логика для высоких укрытий
             if (isPeeking)
            {
                // Выглядываем
                transform.position = Vector3.Lerp(transform.position, coverOriginalPos + peekOffset, Time.deltaTime * 5f);
                if (coverTimer <= 0)
                {
                    isPeeking = false;
                    coverTimer = hideTime;
                }
            }
            else
            {
                // Прячемся
                transform.position = Vector3.Lerp(transform.position, coverOriginalPos, Time.deltaTime * 5f);
                if (coverTimer <= 0)
                {
                    isPeeking = true;
                    coverTimer = peekTime;
                    coverOriginalPos = transform.position; // Запоминаем позицию перед выглядыванием
                }
            }
        }
    }

    void SetCrouchPosition()
    {
        // Мгновенно "приседаем" при достижении низкого укрытия
        transform.position = new Vector3(transform.position.x, basePositionLow.y + crouchHeightOffset, transform.position.z);
        peekTimerLow = peekDurationLow; // Готовимся выглядывать
        isPeekingLow = false; // Начинаем в укрытии
    }
    void ReleaseCover()
    {
        currentCover = null;
        inCover = false;
        isPeeking = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            isPatrolling = false;
            return;
        }

        agent.speed = patrolSpeed;
        isPatrolling = true;
        agent.autoBraking = false; // Позволяем агенту "пересекать" точку, а не тормозить перед ней

        // Если мы в данный момент ждем на точке
        if (patrolWaitTimer > 0)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (patrolWaitTimer <= 0)
            {
                // Ожидание закончилось, отправляемся к следующей точке
                agent.isStopped = false;
                SetNextPatrolDestination();
            }
        }
        // Если мы движемся
        else
        {
            // Проверяем, не "пересекли" ли мы уже точку назначения
            if (!agent.pathPending && agent.remainingDistance <= patrolPointArrivalThreshold)
            {
                // Мы достигли точки. Проверяем, нужно ли ждать.
                float currentWaitTime = patrolPoints[patrolIndex].waitTime;
                if (currentWaitTime > 0)
                {
                    // Начинаем ожидание и останавливаем агента вручную
                    patrolWaitTimer = currentWaitTime;
                    agent.isStopped = true;
                }
                else
                {
                    // Ждать не нужно, сразу идем к следующей точке
                    SetNextPatrolDestination();
                }
            }
        }

        // Этот блок для самого первого запуска патруля
        if (!agent.hasPath && patrolWaitTimer <= 0)
        {
            SetNextPatrolDestination();
        }
    }

    private void SetNextPatrolDestination()
    {
        if (patrolPoints.Count == 0) return;

        patrolIndex = (patrolIndex + 1) % patrolPoints.Count;
        
        Vector3 targetPoint = patrolPoints[patrolIndex].point.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPoint, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"[EnemyAI] Точка патруля {patrolPoints[patrolIndex].point.name} недостижима! Пропускаю.", gameObject);
            // Пропускаем точку, если она недостижима, чтобы не застрять
            patrolWaitTimer = 0.1f; 
        }
    }

    public void OnGrenadeDanger(Vector3 pos, float radius)
    {
        FindAndMoveToSafeCover(pos, radius);
    }

    public void OnSmokeEnter(Vector3 pos, float radius)
    {
        // Логика реакции на дым
    }

    void FindAndMoveToSafeCover(Vector3 dangerPos, float dangerRadius)
    {
        // Ищем укрытия, которые не находятся в радиусе опасности
        var safeCovers = FindObjectsOfType<CoverPoint>()
            .Where(p => Vector3.Distance(p.transform.position, dangerPos) > dangerRadius && !p.isOccupied)
            .OrderBy(p => Vector3.Distance(p.transform.position, transform.position))
            .ToList();

        if (safeCovers.Any())
        {
            ReleaseCover(); // Освобождаем текущее укрытие
            currentCover = safeCovers.First();
            currentCover.isOccupied = true;
            inCover = true;
            agent.SetDestination(currentCover.transform.position);
            Debug.Log($"{name} бежит в укрытие от гранаты!");
        }
        else
        {
            // Если укрытий нет, просто бежим от гранаты
            Vector3 runDirection = (transform.position - dangerPos).normalized;
            Vector3 runDestination = transform.position + runDirection * 10f; // Бежим на 10м
            agent.SetDestination(runDestination);
            Debug.Log($"{name} просто убегает от гранаты!");
        }
    }
    public bool IsVulnerableToStealthKill()
    {
        return !isAggressive; // Можно убить, если не в агрессивном состоянии
    }

    private Vector3 GetLookDirection()
    {
        switch (currentState)
        {
            case State.Chase:
            case State.Attack:
                if (target != null)
                {
                    return (target.position - transform.position).normalized;
                }
                return transform.forward;
            
            case State.Investigate:
            case State.Idle:
                if (agent.velocity.sqrMagnitude > 0.1f)
                {
                    return agent.velocity.normalized;
                }
                return transform.forward;
            
            default:
                return transform.forward;
        }
    }

    public void OnShotHeard(Vector3 shotPosition, WeaponParametrs weapon)
    {
        // Не реагируем на звуки, если уже знаем где игрок или мертвы
        if (currentState == State.Chase || currentState == State.Attack) return;
        
        // Используем общую систему шума
        NoiseManager.EmitNoise(shotPosition, weapon.shotSoundRadius);
    }

    /// <summary>
    /// Публичный метод, вызываемый NoiseManager-ом, когда враг слышит шум.
    /// </summary>
    /// <param name="noisePosition">Позиция источника шума.</param>
    public void OnNoiseHeard(Vector3 noisePosition)
    {
        // Не реагируем на новый шум, если уже знаем где игрок или заняты чем-то важным
        if (currentState == State.Chase || currentState == State.Attack) return;

        // Проверяем, есть ли прямое "звуковое" сообщение (нет стен)
        float distanceToNoise = Vector3.Distance(transform.position, noisePosition);
        Vector3 startPoint = eye != null ? eye.position : transform.position;
        Vector3 direction = noisePosition - startPoint;

        if (!Physics.Raycast(startPoint, direction.normalized, distanceToNoise, obstacleMask))
        {
            Debug.Log($"{name} услышал шум в {noisePosition} и идет на расследование.");
            lastHeardPosition = noisePosition;
            SwitchState(State.Investigate);
        }
        else
        {
            Debug.Log($"{name} услышал шум, но он был за стеной.");
        }
    }

    public void OnExplosionHeard(Vector3 explosionPosition, float explosionRadius)
    {
        // Взрывы - это особенные, очень громкие события.
        // Оставим для них отдельную логику, но можно было бы тоже перевести на NoiseManager.
        // Не реагируем на взрыв, если уже знаем где игрок
        if (currentState == State.Chase || currentState == State.Attack) return;

        float actualExplosionRadius = explosionRadius * explosionHearingMultiplier;
        float distanceToExplosion = Vector3.Distance(transform.position, explosionPosition);

        // Мгновенная агрессия, если взрыв совсем рядом
        if (distanceToExplosion <= explosionAggroRadius)
        {
            Debug.Log($"{name} в ярости из-за близкого взрыва!");
            lastHeardPosition = explosionPosition;
            isAggressive = true;
            SwitchState(State.Chase);
            return;
        }

        // Расследование, если взрыв слышно, но он не так близко
        if (distanceToExplosion <= actualExplosionRadius)
        {
             // Проверяем наличие стен
            Vector3 startPoint = eye != null ? eye.position : transform.position;
            Vector3 direction = explosionPosition - startPoint;
            if (!Physics.Raycast(startPoint, direction.normalized, distanceToExplosion, obstacleMask))
            {
                Debug.Log($"{name} услышал взрыв и идет на расследование.");
                lastHeardPosition = explosionPosition;
                investigationTime = explosionInvestigationTime; // Увеличиваем время расследования для взрывов
                SwitchState(State.Investigate);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 3D-конус обычного зрения
        DrawVisionCone3D(new Color(1, 1, 0, 0.2f), visionRange, visionAngle, verticalFOV, 18, 16);
        // 3D-конус зрения в темноте
        DrawVisionCone3D(new Color(0, 0, 1, 0.15f), darkVisionRange, darkVisionAngle, verticalFOV, 18, 16);
        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        // Линия расследования
        if (currentState == State.Investigate)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, lastHeardPosition);
        }

        // ВИЗУАЛИЗАЦИЯ ПАТРУЛЬНЫХ ТОЧЕК
        if (patrolPoints != null && patrolPoints.Count > 0)
        {
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                var pt = patrolPoints[i];
                if (pt != null && pt.point != null)
                {
                    // Линия от врага к точке патруля
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, pt.point.position);

                    // Сфера в позиции точки патруля
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(pt.point.position, 0.4f);

                    // Сфера в позиции ближайшей точки на NavMesh
                    UnityEngine.AI.NavMeshHit hit;
                    if (UnityEngine.AI.NavMesh.SamplePosition(pt.point.position, out hit, 2.0f, UnityEngine.AI.NavMesh.AllAreas))
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawWireSphere(hit.position, 0.25f);
                    }
                }
            }
        }
    }

    private void DrawVisionCone3D(Color color, float range, float horizontalAngle, float verticalAngle, int segments = 18, int rings = 16)
    {
        Gizmos.color = color;
        Vector3 origin = eye != null ? eye.position : transform.position;
        for (int i = 1; i <= rings; i++)
        {
            float vFrac = i / (float)rings;
            float vAngle = Mathf.Lerp(-verticalAngle / 2, verticalAngle / 2, vFrac);
            Quaternion vRot = Quaternion.AngleAxis(vAngle, transform.right);
            Vector3 prevPoint = Vector3.zero;
            for (int j = 0; j <= segments; j++)
            {
                float hFrac = j / (float)segments;
                float hAngle = Mathf.Lerp(-horizontalAngle / 2, horizontalAngle / 2, hFrac);
                Quaternion hRot = Quaternion.AngleAxis(hAngle, transform.up);
                Vector3 dir = hRot * vRot * transform.forward;
                Vector3 point = origin + dir * range;
                if (j > 0)
                    Gizmos.DrawLine(prevPoint, point);
                if (i > 1)
                {
                    // Соединяем с предыдущим кольцом
                    float prevVAngle = Mathf.Lerp(-verticalAngle / 2, verticalAngle / 2, (i - 1) / (float)rings);
                    Quaternion prevVRot = Quaternion.AngleAxis(prevVAngle, transform.right);
                    Vector3 prevDir = hRot * prevVRot * transform.forward;
                    Vector3 prevRingPoint = origin + prevDir * range;
                    Gizmos.DrawLine(prevRingPoint, point);
                }
                prevPoint = point;
                if (i == rings)
                    Gizmos.DrawLine(origin, point);
            }
        }
        // Центральная вертикальная линия вверх
        Vector3 upDir = Quaternion.AngleAxis(verticalAngle / 2, transform.right) * transform.forward;
        Gizmos.DrawLine(origin, origin + upDir * range);
        // Центральная вертикальная линия вниз
        Vector3 downDir = Quaternion.AngleAxis(-verticalAngle / 2, transform.right) * transform.forward;
        Gizmos.DrawLine(origin, origin + downDir * range);
    }

    // ===================================================================
    // НОВЫЙ МЕТОД ДЛЯ ВЗАИМОДЕЙСТВИЯ С ДВЕРЯМИ
    // ===================================================================
    private void CheckForDoors()
    {
        // Проверяем только если агент движется
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            // Пускаем луч перед собой, чтобы найти дверь
            if (Physics.Raycast(eye.position, transform.forward, out RaycastHit hit, 1f))
            {
                if (hit.collider.TryGetComponent<DoorInteractable>(out var door))
                {
                    // Если дверь закрыта, взаимодействуем с ней
                    if (!door.IsOpen)
                    {
                        door.Interact();
                        lastDoor = door; // Запоминаем дверь
                        doorWasClosed = true; // Запоминаем, что она была закрыта
                    }
                }
            }
        }
    }

    public void OnOutOfAmmo()
    {
        if (enemyShooting != null && knifeWeapon != null && enemyShooting.weapon != knifeWeapon)
        {
            enemyShooting.weapon = knifeWeapon;
            Debug.Log($"{name} переключился на нож — патроны закончились!");
        }
    }

    public State GetCurrentState()
    {
        return currentState;
    }

    public void ApplySaveData(EnemyData data)
    {
        transform.position = data.position;
        transform.rotation = data.rotation;

        var health = GetComponent<EnemyHealth>();
        if (health != null) health.health = data.health;

        isAggressive = data.isAggressive;

        var shooting = GetComponent<EnemyShooting>();
        if (shooting != null && shooting.weapon != null)
        {
            shooting.weapon.ammoInClip = data.ammoInClip;
            shooting.weapon.ammoTotal = data.totalAmmo;
        }

        // Принудительно переключаем состояние AI
        SwitchState((State)data.aiState);
        
        // Если враг был неагрессивным и патрулировал, 
        // нужно заново запустить логику патрулирования
        if (!isAggressive && (State)data.aiState == State.Patrol)
        {
            isPatrolling = true;
        }
    }
}


[System.Serializable]
public class PatrolPoint
{
    public Transform point;
    public float waitTime = 2f;
}