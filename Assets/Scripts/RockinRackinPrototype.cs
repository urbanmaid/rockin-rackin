using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public sealed class RockinRackinPrototype : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private float fieldSize = 26f;
    [SerializeField] private float baseTiltDegrees = 12f;
    [SerializeField] private float boostTiltDegrees = 23f;
    [SerializeField] private float tiltSmoothing = 7.5f;
    [SerializeField] private float suddenTiltLiftThreshold = 80f;
    [SerializeField] private float suddenTiltLiftImpulse = 7f;
    [SerializeField] private float suddenTiltRadius = 5.5f;

    [Header("Player")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float passiveHealthDrain = 3.5f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float fallDamage = 32f;
    [SerializeField] private float invulnerableSeconds = 0.75f;
    [SerializeField] private float pushRadius = 5f;
    [SerializeField] private float pushImpulse = 13f;
    [SerializeField] private float pushCooldownSeconds = 5f;

    [Header("Enemies")]
    [SerializeField] private int startingEnemies = 10;
    [SerializeField] private int maxEnemies = 24;
    [SerializeField] private float enemySpawnInterval = 4.5f;
    [SerializeField] private float enemyHealth = 35f;
    [SerializeField] private float enemyHomingForce = 3.2f;
    [SerializeField] private float landingDamageVelocity = 6.5f;
    [SerializeField] private float landingDamageMultiplier = 7f;

    [Header("Pickups")]
    [SerializeField] private int startingPickups = 8;
    [SerializeField] private int maxPickups = 18;
    [SerializeField] private float healthSpawnInterval = 2.8f;
    [SerializeField] private float healthRestore = 16f;

    [Header("Templates")]
    [SerializeField] private GameObject stageTemplate;
    [SerializeField] private GameObject playerTemplate;
    [SerializeField] private GameObject enemyTemplate;
    [SerializeField] private GameObject healthPickupTemplate;

    private readonly List<RollingBallAgent> enemies = new();
    private readonly List<HealthPickup> pickups = new();
    private readonly List<UpgradeOption> pendingUpgrades = new();

    private Transform stageRoot;
    private Transform healthItemRoot;
    private Collider stageCollider;
    private Rigidbody playerBody;
    private Camera mainCamera;
    private InputAction tiltAction;
    private InputAction pushAction;
    private InputAction boostAction;
    private InputAction restartAction;

    private Vector2 targetTilt;
    private Vector2 smoothedTilt;
    private Vector2 previousSmoothedTilt;
    private float health;
    private float invulnerableTimer;
    private float pushCooldownTimer;
    private float enemySpawnTimer;
    private float pickupSpawnTimer;
    private float luck;
    private int score;
    private int level = 1;
    private int nextUpgradeScore = 6;
    private bool upgradeOpen;
    private bool gameOver;

    public Rigidbody PlayerBody => playerBody;
    public float EnemyHomingForce => enemyHomingForce;
    public float ContactDamage => contactDamage;

    private void Awake()
    {
        Physics.gravity = new Vector3(0f, -16f, 0f);
        Time.timeScale = 1f;
        health = maxHealth;
        BuildInputActions();
        BuildScene();
    }

    private void OnEnable()
    {
        SetInputEnabled(true);
    }

    private void OnDisable()
    {
        SetInputEnabled(false);
    }

    private void OnDestroy()
    {
        tiltAction?.Dispose();
        pushAction?.Dispose();
        boostAction?.Dispose();
        restartAction?.Dispose();
    }

    private void Update()
    {
        if (gameOver)
        {
            if (restartAction.WasPressedThisFrame())
            {
                RestartPrototype();
            }

            return;
        }

        if (upgradeOpen)
        {
            return;
        }

        ReadTiltInput();
        UpdatePlayerActions();
        UpdateHealthAndSpawns();
        CheckOutOfBounds();
    }

    private void FixedUpdate()
    {
        if (gameOver || upgradeOpen || stageRoot == null)
        {
            return;
        }

        UpdateTilt();
    }

    private void BuildInputActions()
    {
        tiltAction = new InputAction("Tilt Stage", InputActionType.Value, expectedControlType: "Vector2");
        tiltAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        tiltAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        tiltAction.AddBinding("<Gamepad>/leftStick");

        pushAction = new InputAction("Push Nearby Enemies", InputActionType.Button);
        pushAction.AddBinding("<Keyboard>/space");
        pushAction.AddBinding("<Gamepad>/buttonWest");

        boostAction = new InputAction("Boost Tilt", InputActionType.Button);
        boostAction.AddBinding("<Keyboard>/leftShift");
        boostAction.AddBinding("<Keyboard>/rightShift");
        boostAction.AddBinding("<Gamepad>/buttonSouth");

        restartAction = new InputAction("Restart Prototype", InputActionType.Button);
        restartAction.AddBinding("<Keyboard>/r");

        SetInputEnabled(true);
    }

    private void SetInputEnabled(bool enabled)
    {
        SetActionEnabled(tiltAction, enabled);
        SetActionEnabled(pushAction, enabled);
        SetActionEnabled(boostAction, enabled);
        SetActionEnabled(restartAction, enabled);
    }

    private static void SetActionEnabled(InputAction action, bool enabled)
    {
        if (action == null)
        {
            return;
        }

        if (enabled && !action.enabled)
        {
            action.Enable();
        }
        else if (!enabled && action.enabled)
        {
            action.Disable();
        }
    }

    private void LateUpdate()
    {
        if (playerBody == null || mainCamera == null)
        {
            return;
        }

        Vector3 wantedPosition = playerBody.position + new Vector3(0f, 13f, -11f);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, wantedPosition, Time.unscaledDeltaTime * 7f);
        mainCamera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
    }

    private void OnGUI()
    {
        const int width = 310;
        GUI.Box(new Rect(12, 12, width, 126), string.Empty);
        GUI.Label(new Rect(26, 24, width - 28, 22), $"Health {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}");
        GUI.HorizontalScrollbar(new Rect(26, 48, width - 44, 18), 0f, health, 0f, maxHealth);
        GUI.Label(new Rect(26, 70, width - 28, 22), $"Score {score}    Level {level}    Next {nextUpgradeScore}");
        string pushText = pushCooldownTimer <= 0f ? "Push Ready" : $"Push {pushCooldownTimer:0.0}s";
        GUI.Label(new Rect(26, 94, width - 28, 22), $"{pushText}    Luck +{luck:0%}");

        if (upgradeOpen)
        {
            DrawUpgradePanel();
        }

        if (gameOver)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 170f, Screen.height * 0.5f - 58f, 340f, 116f), "Game Over");
            GUI.Label(new Rect(Screen.width * 0.5f - 132f, Screen.height * 0.5f - 20f, 264f, 24f), "R 키를 눌러 다시 시작");
        }
    }

    private void BuildScene()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 120f;
        mainCamera.fieldOfView = 55f;

        stageRoot = InstantiateRequired(stageTemplate, Vector3.zero, Quaternion.identity, transform).transform;
        stageRoot.name = "Tilting Stage Root";
        ConfigureStageInstance();

        SpawnPlayer(transform);

        for (int i = 0; i < startingEnemies; i++)
        {
            SpawnEnemy();
        }

        for (int i = 0; i < startingPickups; i++)
        {
            SpawnPickup();
        }
    }

    private GameObject InstantiateRequired(GameObject template, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (template == null)
        {
            Debug.LogError("Prototype template reference is missing.", this);
            enabled = false;
            return new GameObject("Missing Prototype Template");
        }

        GameObject instance = Instantiate(template, position, rotation, parent);
        instance.SetActive(true);
        return instance;
    }

    private void ConfigureStageInstance()
    {
        Transform stagePlane = stageRoot.Find("Primitive Plane Stage");
        if (stagePlane != null)
        {
            //stagePlane.localPosition = Vector3.zero;
            //stagePlane.localScale = Vector3.one * (fieldSize / 10f);
            //stageCollider = stagePlane.GetComponent<Collider>();
        }
        else
        {
            stageCollider = stageRoot.GetComponentInChildren<Collider>();
        }

        ConfigureEdgeMarker("North Edge", new Vector3(0f, -0.02f, fieldSize * 0.5f), new Vector3(fieldSize, 0.05f, 0.25f));
        ConfigureEdgeMarker("South Edge", new Vector3(0f, -0.02f, -fieldSize * 0.5f), new Vector3(fieldSize, 0.05f, 0.25f));
        ConfigureEdgeMarker("East Edge", new Vector3(fieldSize * 0.5f, -0.02f, 0f), new Vector3(0.25f, 0.05f, fieldSize));
        ConfigureEdgeMarker("West Edge", new Vector3(-fieldSize * 0.5f, -0.02f, 0f), new Vector3(0.25f, 0.05f, fieldSize));

        healthItemRoot = stageRoot.Find("Health Item");
        if (healthItemRoot == null)
        {
            Debug.LogWarning("Health Item parent was not found under the stage. Health pickups will use the stage root.", stageRoot);
            healthItemRoot = stageRoot;
        }
        else
        {
            healthItemRoot.localPosition = Vector3.zero;
            healthItemRoot.localRotation = Quaternion.identity;
            healthItemRoot.localScale = Vector3.one;
        }
    }

    private void ConfigureEdgeMarker(string markerName, Vector3 position, Vector3 scale)
    {
        Transform marker = stageRoot.Find(markerName);
        if (marker == null)
        {
            return;
        }

        marker.localPosition = position;
        marker.localScale = scale;
    }

    private void SpawnPlayer(Transform parent)
    {
        GameObject player = InstantiateRequired(playerTemplate, new Vector3(0f, 1.2f, 0f), Quaternion.identity, parent);
        player.name = "Player Sphere";
        player.transform.localScale = Vector3.one * 1.1f;

        playerBody = player.GetComponent<Rigidbody>();
        //ConfigureBallBody(playerBody, 1.15f, 0.15f);

        RollingBallAgent agent = player.GetComponent<RollingBallAgent>();
        if (playerBody == null || agent == null)
        {
            Debug.LogError("Player template must include Rigidbody and RollingBallAgent.", player);
            enabled = false;
            return;
        }

        agent.Configure(this, false, maxHealth);
    }

    private void SpawnEnemy()
    {
        if (enemies.Count >= maxEnemies)
        {
            return;
        }

        Vector3 position = RandomPointOnStage(4f);
        if (Vector3.Distance(position, playerBody.position) < 4f)
        {
            position += position.normalized * 4f;
        }

        GameObject enemy = InstantiateRequired(enemyTemplate, position + Vector3.up * 1.1f, Quaternion.identity, transform);
        enemy.name = "Enemy Sphere";
        enemy.transform.localScale = Vector3.one * 0.95f;

        RollingBallAgent agent = enemy.GetComponent<RollingBallAgent>();
        Rigidbody body = enemy.GetComponent<Rigidbody>();
        if (body == null || agent == null)
        {
            Debug.LogError("Enemy template must include Rigidbody and RollingBallAgent.", enemy);
            Destroy(enemy);
            return;
        }

        //ConfigureBallBody(body, 0.9f, 0.08f);
        agent.Configure(this, true, enemyHealth);
        enemies.Add(agent);
    }

    private void SpawnPickup()
    {
        if (pickups.Count >= maxPickups)
        {
            return;
        }

        CreatePickup("Health Pickup", RandomPointOnStage(2f) + Vector3.up * 0.45f, Vector3.one * 0.55f);
    }

    private static void ConfigureBallBody(Rigidbody body, float mass, float damping)
    {
        if (body == null)
        {
            return;
        }

        body.mass = mass;
        body.linearDamping = damping;
        body.angularDamping = 0.05f;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.isKinematic = false;
        body.useGravity = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    private Vector3 RandomPointOnStage(float margin)
    {
        float extent = fieldSize * 0.5f - margin;
        return new Vector3(UnityEngine.Random.Range(-extent, extent), 0f, UnityEngine.Random.Range(-extent, extent));
    }

    private void ReadTiltInput()
    {
        targetTilt = Vector2.ClampMagnitude(tiltAction.ReadValue<Vector2>(), 1f);
    }

    private void UpdateTilt()
    {
        bool boost = boostAction.IsPressed();
        float tiltDegrees = boost ? boostTiltDegrees : baseTiltDegrees;
        previousSmoothedTilt = smoothedTilt;
        smoothedTilt = Vector2.Lerp(smoothedTilt, targetTilt, 1f - Mathf.Exp(-tiltSmoothing * Time.fixedDeltaTime));

        Quaternion targetRotation = Quaternion.Euler(smoothedTilt.y * tiltDegrees, 0f, -smoothedTilt.x * tiltDegrees);
        ApplyStageRotationAroundPlayer(targetRotation);

        float tiltSpeed = (smoothedTilt - previousSmoothedTilt).magnitude * tiltDegrees / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
        /*
        if (tiltSpeed >= suddenTiltLiftThreshold)
        {
            LiftNearbyEnemies(Mathf.InverseLerp(suddenTiltLiftThreshold, suddenTiltLiftThreshold * 2f, tiltSpeed));
        }
        */
    }

    private void ApplyStageRotationAroundPlayer(Quaternion targetRotation)
    {
        if (playerBody == null)
        {
            stageRoot.rotation = targetRotation;
            return;
        }

        Vector3 pivotWorld = playerBody.position;
        Vector3 pivotLocal = stageRoot.InverseTransformPoint(pivotWorld);

        stageRoot.rotation = targetRotation;

        Vector3 pivotAfterRotation = stageRoot.TransformPoint(pivotLocal);
        stageRoot.position += pivotWorld - pivotAfterRotation;
    }

    private void LiftNearbyEnemies(float strength)
    {
        Vector3 playerPosition = playerBody.position;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            RollingBallAgent enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            Vector3 offset = enemy.transform.position - playerPosition;
            offset.y = 0f;
            float distance = offset.magnitude;
            if (distance > suddenTiltRadius)
            {
                continue;
            }

            float falloff = 1f - distance / suddenTiltRadius;
            Vector3 impulse = Vector3.up * suddenTiltLiftImpulse * (0.4f + strength) * falloff;
            enemy.Body.AddForce(impulse, ForceMode.Impulse);
        }
    }

    private void UpdatePlayerActions()
    {
        pushCooldownTimer = Mathf.Max(0f, pushCooldownTimer - Time.deltaTime);
        invulnerableTimer = Mathf.Max(0f, invulnerableTimer - Time.deltaTime);

        bool pushPressed = pushAction.WasPressedThisFrame();
        if (pushPressed && pushCooldownTimer <= 0f)
        {
            UsePush();
        }
    }

    private void UsePush()
    {
        pushCooldownTimer = pushCooldownSeconds;
        Vector3 playerPosition = playerBody.position;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            RollingBallAgent enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            Vector3 offset = enemy.transform.position - playerPosition;
            float distance = offset.magnitude;
            if (distance > pushRadius)
            {
                continue;
            }

            Vector3 direction = offset.normalized;
            direction.y = 0.35f;
            enemy.Body.AddForce(direction.normalized * pushImpulse * (1f - distance / pushRadius), ForceMode.Impulse);
        }
    }

    private void UpdateHealthAndSpawns()
    {
        health -= passiveHealthDrain * Time.deltaTime;
        if (health <= 0f)
        {
            health = 0f;
            gameOver = true;
            Time.timeScale = 0f;
            return;
        }

        enemySpawnTimer += Time.deltaTime;
        if (enemySpawnTimer >= enemySpawnInterval)
        {
            enemySpawnTimer = 0f;
            SpawnEnemy();
        }

        pickupSpawnTimer += Time.deltaTime;
        if (pickupSpawnTimer >= healthSpawnInterval)
        {
            pickupSpawnTimer = 0f;
            SpawnPickup();
        }
    }

    private void CheckOutOfBounds()
    {
        if (IsOutsideField(playerBody.position))
        {
            DamagePlayer(fallDamage);
            ResetBody(playerBody, stageRoot.transform.position + Vector3.up * 1.4f);
        }

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            RollingBallAgent enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            if (IsOutsideField(enemy.transform.position))
            {
                DestroyEnemy(enemy, true);
            }
        }
    }

    private bool IsOutsideField(Vector3 position)
    {
        float limit = fieldSize * 0.5f + 1.7f;
        return Mathf.Abs(position.x) > limit || Mathf.Abs(position.z) > limit || position.y < stageRoot.transform.position.y - limit;
    }

    private static void ResetBody(Rigidbody body, Vector3 position)
    {
        body.position = position;
        body.rotation = Quaternion.identity;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }

    public bool IsStageCollider(Collider other)
    {
        return other == stageCollider;
    }

    public void DamagePlayer(float amount)
    {
        if (gameOver || invulnerableTimer > 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - amount);
        invulnerableTimer = invulnerableSeconds;
        if (health <= 0f)
        {
            gameOver = true;
            Time.timeScale = 0f;
        }
    }

    public void DamageEnemy(RollingBallAgent enemy, float amount)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.Health -= amount;
        if (enemy.Health <= 0f)
        {
            DestroyEnemy(enemy, true);
        }
    }

    private void DestroyEnemy(RollingBallAgent enemy, bool allowDrop)
    {
        if (enemy == null)
        {
            return;
        }

        enemies.Remove(enemy);
        Vector3 dropPosition = enemy.transform.position;
        Destroy(enemy.gameObject);

        float dropChance = Mathf.Clamp01(0.62f + luck * 0.2f);
        if (allowDrop && UnityEngine.Random.value <= dropChance)
        {
            SpawnPickupNear(dropPosition);
        }
    }

    private void SpawnPickupNear(Vector3 position)
    {
        if (pickups.Count >= maxPickups)
        {
            return;
        }

        Vector3 localPosition = healthItemRoot.InverseTransformPoint(position);
        CreatePickup("Dropped Health Pickup", new Vector3(
            Mathf.Clamp(localPosition.x, -fieldSize * 0.45f, fieldSize * 0.45f),
            0.45f,
            Mathf.Clamp(localPosition.z, -fieldSize * 0.45f, fieldSize * 0.45f)),
            Vector3.one * 0.62f);
    }

    private void CreatePickup(string pickupName, Vector3 localPosition, Vector3 scale)
    {
        GameObject pickup = InstantiateRequired(healthPickupTemplate, Vector3.zero, Quaternion.identity, healthItemRoot);
        pickup.name = pickupName;
        pickup.transform.localPosition = localPosition;
        pickup.transform.localRotation = Quaternion.identity;
        pickup.transform.localScale = scale;

        Collider pickupCollider = pickup.GetComponent<Collider>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }

        Rigidbody body = pickup.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
        }

        HealthPickup pickupComponent = pickup.GetComponent<HealthPickup>();
        if (pickupComponent == null)
        {
            Debug.LogError("Health pickup template must include HealthPickup.", pickup);
            Destroy(pickup);
            return;
        }

        pickupComponent.Configure(this);
        pickups.Add(pickupComponent);
    }

    public void CollectPickup(HealthPickup pickup)
    {
        if (pickup == null || !pickups.Remove(pickup))
        {
            return;
        }

        health = Mathf.Min(maxHealth, health + healthRestore);
        score++;
        Destroy(pickup.gameObject);

        if (score >= nextUpgradeScore)
        {
            OpenUpgradeChoices();
        }
    }

    public void NotifyPickupDestroyed(HealthPickup pickup)
    {
        pickups.Remove(pickup);
    }

    public void HandleEnemyLanding(RollingBallAgent enemy, Vector3 previousVelocity)
    {
        if (enemy == null || previousVelocity.y > -landingDamageVelocity)
        {
            return;
        }

        float damage = (-previousVelocity.y - landingDamageVelocity) * landingDamageMultiplier;
        bool critical = UnityEngine.Random.value < luck * 0.12f;
        if (critical)
        {
            damage *= 1.8f;
        }

        DamageEnemy(enemy, damage);
    }

    private void OpenUpgradeChoices()
    {
        upgradeOpen = true;
        Time.timeScale = 0f;
        pendingUpgrades.Clear();

        List<UpgradeKind> pool = new()
        {
            UpgradeKind.ItemDensity,
            UpgradeKind.MaxHealth,
            UpgradeKind.BoostTilt,
            UpgradeKind.PushCooldown,
            UpgradeKind.Luck
        };

        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            pendingUpgrades.Add(CreateUpgrade(pool[index]));
            pool.RemoveAt(index);
        }
    }

    private UpgradeOption CreateUpgrade(UpgradeKind kind)
    {
        bool highGrade = UnityEngine.Random.value < 0.16f + luck * 0.08f;
        float multiplier = highGrade ? 1.55f : 1f;
        string grade = highGrade ? "++" : "+";

        return kind switch
        {
            UpgradeKind.ItemDensity => new UpgradeOption("체력 아이템 밀도 " + grade, "아이템 생성이 빨라지고 최대 개수가 증가", () =>
            {
                healthSpawnInterval = Mathf.Max(0.75f, healthSpawnInterval - 0.42f * multiplier);
                maxPickups += highGrade ? 4 : 2;
            }),
            UpgradeKind.MaxHealth => new UpgradeOption("최대 체력 " + grade, "최대 체력과 현재 체력을 함께 증가", () =>
            {
                float increase = 18f * multiplier;
                maxHealth += increase;
                health = Mathf.Min(maxHealth, health + increase);
            }),
            UpgradeKind.BoostTilt => new UpgradeOption("더 기울이기 " + grade, "쉬프트/A 버튼의 최대 각도와 띄우기 힘 증가", () =>
            {
                boostTiltDegrees += 3.2f * multiplier;
                suddenTiltLiftImpulse += 0.9f * multiplier;
            }),
            UpgradeKind.PushCooldown => new UpgradeOption("밀어내기 쿨타임 " + grade, "스페이스/X 버튼 재사용 시간 감소", () =>
            {
                pushCooldownSeconds = Mathf.Max(1.2f, pushCooldownSeconds - 0.7f * multiplier);
            }),
            _ => new UpgradeOption("운 " + grade, "드롭, 고등급 업그레이드, 치명타 확률 증가", () =>
            {
                luck += 0.18f * multiplier;
            })
        };
    }

    private void DrawUpgradePanel()
    {
        Rect panel = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 128f, 440f, 256f);
        GUI.Box(panel, $"Level {level + 1} Upgrade");
        GUI.Label(new Rect(panel.x + 24f, panel.y + 34f, panel.width - 48f, 24f), "업그레이드 1개를 선택하세요.");

        for (int i = 0; i < pendingUpgrades.Count; i++)
        {
            UpgradeOption option = pendingUpgrades[i];
            Rect button = new Rect(panel.x + 24f, panel.y + 70f + i * 54f, panel.width - 48f, 42f);
            if (GUI.Button(button, $"{option.Title}\n{option.Description}"))
            {
                option.Apply();
                level++;
                nextUpgradeScore += 6 + level * 3;
                pendingUpgrades.Clear();
                upgradeOpen = false;
                Time.timeScale = 1f;
            }
        }
    }

    private void RestartPrototype()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private readonly struct UpgradeOption
    {
        public UpgradeOption(string title, string description, Action apply)
        {
            Title = title;
            Description = description;
            Apply = apply;
        }

        public string Title { get; }
        public string Description { get; }
        public Action Apply { get; }
    }

    private enum UpgradeKind
    {
        ItemDensity,
        MaxHealth,
        BoostTilt,
        PushCooldown,
        Luck
    }
}
