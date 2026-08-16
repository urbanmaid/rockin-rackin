using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine;

public sealed class RockinRackinPrototype : MonoBehaviour
{
    [Header("Stage")]
    [SerializeField] private float fieldSize = 26f;
    [SerializeField] private bool useCircularStage;
    [SerializeField] private Vector2 circularStageCenter = Vector2.zero;
    [SerializeField] private float circularStageRadius = 13f;
    [SerializeField] private float baseTiltDegrees = 12f;
    [SerializeField] private float boostTiltDegrees = 23f;
    [SerializeField] private float tiltSmoothing = 7.5f;
    [SerializeField] private float suddenTiltLiftImpulse = 7f;
    [SerializeField] private float suddenTiltRadius = 5.5f;

    [Header("Player")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float passiveHealthDrain = 3.5f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float fallDamage = 32f;
    [SerializeField] private float invulnerableSeconds = 0.75f;
    [SerializeField] private bool enablePushAbility = true;
    [SerializeField] private float pushRadius = 5f;
    [SerializeField] private float pushImpulse = 13f;
    [SerializeField] private float pushCooldownSeconds = 5f;

    [Header("Camera")]
    [SerializeField] private bool FixCameraToPlayer = true;
    [SerializeField] private float cameraFieldOfView = 55f;
    [SerializeField] private float damageCameraShakeDuration = 0.22f;
    [SerializeField] private float damageCameraShakeMagnitude = 0.42f;
    [SerializeField] private float damageCameraShakeFrequency = 34f;
    [SerializeField] private float pushCameraFovIncrease = 6f;
    [SerializeField] private float pushCameraFovExpandDuration = 0.16f;
    [SerializeField] private float pushCameraFovRecoverDuration = 0.34f;

    [Header("Visual Effects")]
    [SerializeField] private TemporaryMaterialColorChanger boostMaterialColorChanger;

    [Header("Enemies")]
    [SerializeField] private int startingEnemies = 10;
    [SerializeField] private int maxEnemies = 24;
    [SerializeField] private float enemySpawnInterval = 4.5f;
    [SerializeField] private float enemyHealth = 35f;
    [SerializeField] private float enemyHomingForce = 3.2f;
    [SerializeField] private float landingDamageVelocity = 6.5f;

    [Header("Enemy Difficulty")]
    [SerializeField] private float enemyDifficultyStepSeconds = 20f;
    [SerializeField] private int enemySpawnBatchSize = 4;
    [SerializeField] private int enemySpawnBatchIncrease = 2;
    [SerializeField] private int maxEnemySpawnBatchSize = 8;
    [SerializeField] private float enemySpawnIntervalDecreasePerCycle = 0.2f;
    [SerializeField] private float minEnemySpawnInterval = 1.5f;
    [SerializeField] private float enemyHomingForceMultiplierPerMinute = 1.1f;
    [SerializeField] private float maxEnemiesIncreaseIntervalSeconds = 30f;
    [SerializeField] private int maxEnemiesIncreasePerStep = 2;
    [SerializeField] private int maxEnemiesAbsoluteLimit = 40;
    [SerializeField] private float enemySpawnPauseUpgradeSeconds = 8f;

    [Header("Pickups")]
    [SerializeField] private int startingPickups = 8;
    [SerializeField] private int maxPickups = 18;
    [SerializeField] private float healthSpawnInterval = 2.8f;
    [SerializeField] private float healthRestore = 16f;

    [Header("Score")]
    [SerializeField] private float survivalScoreIntervalSeconds = 0.1f;
    [SerializeField] private int survivalScorePerInterval = 1;
    [SerializeField] private int pushRingOutScore = 100;
    [SerializeField] private int tiltRingOutScore = 25;
    [SerializeField] private float pushRingOutAttributionSeconds = 2.5f;
    [SerializeField] private float pushRingOutMinimumPlanarSpeed = 5f;

    [Header("Templates")]
    [SerializeField] private GameObject stageTemplate;
    [SerializeField] private GameObject playerTemplate;
    [SerializeField] private GameObject enemyTemplate;
    [SerializeField] private GameObject healthPickupTemplate;

    [Header("Runtime Flow")]
    [SerializeField] private bool waitForStartUI = true;

    [Header("Graphic UI - Ingame")]
    [SerializeField] private GameObject[] ingameUIObject; // It should be concealed if game over
    [SerializeField] private bool useGraphicUI;
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private LevelCounterUI levelCounterUI;
    [SerializeField] private TimeScoreUI timeScoreUI;
    [SerializeField] private AuxScore auxScore;

    [Header("Graphic UI - Outgame")]
    [SerializeField] private UpgradeUI upgradeUI;
    [SerializeField] private UpgradeItemDictionary upgradeItemSprite;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string localizationTableName = "DefaultLocale";

    private readonly List<RollingBallAgent> enemies = new();
    private readonly List<HealthPickup> pickups = new();
    private readonly List<UpgradeOption> pendingUpgrades = new();

    private Transform stageRoot;
    private Transform healthItemRoot;
    private Collider stageCollider;
    private Rigidbody playerBody;
    private RollingBallAgent playerAgent;
    private Camera mainCamera;
    private InputAction tiltAction;
    private InputAction pushAction;
    private InputAction boostAction;
    private InputAction restartAction;

    private Vector2 targetTilt;
    private Vector2 smoothedTilt;
    private Vector2 previousSmoothedTilt;
    private float smoothedTiltDegrees;
    private float health;
    private float invulnerableTimer;
    private float pushCooldownTimer;
    private float enemySpawnTimer;
    private float enemySpawnPauseTimer;
    private float survivalTime;
    private float finalSurvivalTime;
    private float pickupSpawnTimer;
    private float luck;
    private float survivalScoreTimer;
    private float cameraShakeTimer;
    private float cameraShakeSeed;
    private float pushCameraFovTimer;
    private float pushCameraFovTotalDuration;
    private int totalScore;
    private int upgradePoints;
    private int level = 1;
    private int nextUpgradeScore = 6;
    private bool gameplayStarted;
    private bool upgradeOpen;
    private bool gameOver;
    private bool boostMaterialColorActive;

    public Rigidbody PlayerBody => playerBody;
    public float EnemyHomingForce => enemyHomingForce * Mathf.Pow(Mathf.Max(0.01f, enemyHomingForceMultiplierPerMinute), survivalTime / 60f);
    public float ContactDamage => contactDamage;
    public bool HasGameplayStarted => gameplayStarted;

    public void BeginGame()
    {
        if (gameOver || gameplayStarted)
        {
            return;
        }

        gameplayStarted = true;
        Time.timeScale = 1f;
        GameBgmPlayer.PlayGameplay();
        SetIngameUIVisible(true);
        SetInputEnabled(true);
    }

    private void Awake()
    {
        Physics.gravity = new Vector3(0f, -16f, 0f);
        gameplayStarted = !waitForStartUI;
        Time.timeScale = gameplayStarted ? 1f : 0f;
        health = maxHealth;
        smoothedTiltDegrees = baseTiltDegrees;
        BuildInputActions();
        BuildScene();
        SetIngameUIVisible(gameplayStarted);
        if (gameplayStarted)
        {
            GameBgmPlayer.PlayGameplay();
        }
        else
        {
            GameBgmPlayer.PlayMainMenu();
        }
    }

    private void OnEnable()
    {
        SetInputEnabled(true);
    }

    private void OnDisable()
    {
        SetBoostMaterialColorActive(false);
        SetInputEnabled(false);
    }

    private void OnDestroy()
    {
        tiltAction?.Dispose();
        pushAction?.Dispose();
        boostAction?.Dispose();
        restartAction?.Dispose();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SetActionEnabled(pushAction, enabled && enablePushAbility);
        if (!enablePushAbility)
        {
            pushCooldownTimer = 0f;
            pushCameraFovTimer = 0f;
        }
    }

    private void Update()
    {
        if (!gameplayStarted)
        {
            return;
        }

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
        if (!gameplayStarted || gameOver || upgradeOpen || stageRoot == null)
        {
            SetBoostMaterialColorActive(false);
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
        SetActionEnabled(pushAction, enabled && enablePushAbility);
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
        if (mainCamera == null || !TryGetCameraFocusPosition(out Vector3 focusPosition))
        {
            return;
        }

        Vector3 wantedPosition = focusPosition + new Vector3(0f, 13f, -11f);
        Vector3 cameraPosition = Vector3.Lerp(mainCamera.transform.position, wantedPosition, Time.unscaledDeltaTime * 7f);
        cameraPosition += GetCameraShakeOffset();
        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
        mainCamera.fieldOfView = GetCameraFieldOfView();

        UpdateGameplayUI();
    }

    private bool TryGetCameraFocusPosition(out Vector3 focusPosition)
    {
        if (FixCameraToPlayer)
        {
            if (playerBody == null)
            {
                focusPosition = Vector3.zero;
                return false;
            }

            focusPosition = playerBody.position;
            return true;
        }

        if (stageRoot == null)
        {
            focusPosition = Vector3.zero;
            return false;
        }

        focusPosition = stageRoot.position;
        return true;
    }

    private void OnGUI()
    {
        if (!useGraphicUI)
        {
            DrawFallbackStatusUI();
        }

        if (upgradeOpen && upgradeUI == null)
        {
            DrawUpgradePanel();
        }

        if (gameOver && gameOverUI == null)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 170f, Screen.height * 0.5f - 72f, 340f, 144f), "Game Over");
            GUI.Label(new Rect(Screen.width * 0.5f - 132f, Screen.height * 0.5f - 30f, 264f, 24f), $"Survived {TimeScoreUI.FormatSurvivalTime(finalSurvivalTime)}");
            GUI.Label(new Rect(Screen.width * 0.5f - 132f, Screen.height * 0.5f + 4f, 264f, 24f), "R 키를 눌러 다시 시작");
        }
    }

    private void UpdateGameplayUI()
    {
        if (!useGraphicUI)
        {
            return;
        }

        UpdateGraphicUI();
    }

    private void UpdateGraphicUI()
    {
        healthUI?.UpdateHealth(health, maxHealth, enablePushAbility, pushCooldownTimer, pushCooldownSeconds);
        levelCounterUI?.UpdateLevel(level, upgradePoints, nextUpgradeScore);
        timeScoreUI?.UpdateTimeScore(survivalTime, totalScore);
    }

    private void DrawFallbackStatusUI()
    {
        const int width = 310;
        GUI.Box(new Rect(12, 12, width, 150), string.Empty);
        GUI.Label(new Rect(26, 24, width - 28, 22), $"Health {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}");
        GUI.HorizontalScrollbar(new Rect(26, 48, width - 44, 18), 0f, health, 0f, maxHealth);
        GUI.Label(new Rect(26, 70, width - 28, 22), $"Score {totalScore}    Level {level}    Points {upgradePoints}/{nextUpgradeScore}");
        string pushText = !enablePushAbility ? "Push Disabled" : pushCooldownTimer <= 0f ? "Push Ready" : $"Push {pushCooldownTimer:0.0}s";
        GUI.Label(new Rect(26, 94, width - 28, 22), $"{pushText}    Luck +{luck:0%}");
        GUI.Label(new Rect(26, 118, width - 28, 22), $"Time {TimeScoreUI.FormatSurvivalTime(gameOver ? finalSurvivalTime : survivalTime)}");
    }

    private void SetIngameUIVisible(bool visible)
    {
        if (ingameUIObject == null)
        {
            return;
        }

        for (int i = 0; i < ingameUIObject.Length; i++)
        {
            if (ingameUIObject[i] != null)
            {
                ingameUIObject[i].SetActive(visible);
            }
        }
    }

    private Vector3 GetCameraShakeOffset()
    {
        if (cameraShakeTimer <= 0f || damageCameraShakeDuration <= 0f || damageCameraShakeMagnitude <= 0f)
        {
            cameraShakeTimer = 0f;
            return Vector3.zero;
        }

        float elapsed = damageCameraShakeDuration - cameraShakeTimer;
        float shakeProgress = Mathf.Clamp01(cameraShakeTimer / damageCameraShakeDuration);
        float strength = damageCameraShakeMagnitude * shakeProgress * shakeProgress;
        float frequency = Mathf.Max(0.01f, damageCameraShakeFrequency);
        float sampleTime = elapsed * frequency;
        float x = Mathf.PerlinNoise(cameraShakeSeed, sampleTime) * 2f - 1f;
        float y = Mathf.PerlinNoise(cameraShakeSeed + 47.31f, sampleTime) * 2f - 1f;

        cameraShakeTimer = Mathf.Max(0f, cameraShakeTimer - Time.unscaledDeltaTime);

        return (mainCamera.transform.right * x + mainCamera.transform.up * y) * strength;
    }

    private void StartDamageCameraShake()
    {
        cameraShakeTimer = Mathf.Max(cameraShakeTimer, damageCameraShakeDuration);
        cameraShakeSeed = UnityEngine.Random.value * 1000f;
    }

    private float GetCameraFieldOfView()
    {
        return cameraFieldOfView + GetPushCameraFovOffset();
    }

    private float GetPushCameraFovOffset()
    {
        if (pushCameraFovTimer <= 0f || pushCameraFovIncrease <= 0f || pushCameraFovTotalDuration <= 0f)
        {
            pushCameraFovTimer = 0f;
            return 0f;
        }

        float elapsed = pushCameraFovTotalDuration - pushCameraFovTimer;
        float expandDuration = Mathf.Max(0.0001f, pushCameraFovExpandDuration);
        float recoverDuration = Mathf.Max(0.0001f, pushCameraFovRecoverDuration);
        float offset;

        if (elapsed <= expandDuration)
        {
            float t = Mathf.Clamp01(elapsed / expandDuration);
            offset = pushCameraFovIncrease * EaseOutCubic(t);
        }
        else
        {
            float t = Mathf.Clamp01((elapsed - expandDuration) / recoverDuration);
            offset = pushCameraFovIncrease * (1f - EaseOutCubic(t));
        }

        pushCameraFovTimer = Mathf.Max(0f, pushCameraFovTimer - Time.unscaledDeltaTime);
        return offset;
    }

    private void StartPushCameraFovPulse()
    {
        pushCameraFovTotalDuration = Mathf.Max(0f, pushCameraFovExpandDuration) + Mathf.Max(0f, pushCameraFovRecoverDuration);
        pushCameraFovTimer = pushCameraFovTotalDuration;
    }

    private void SetBoostMaterialColorActive(bool active)
    {
        if (boostMaterialColorActive == active)
        {
            return;
        }

        boostMaterialColorActive = active;
        boostMaterialColorChanger?.SetChanged(active);
    }

    private static float EaseOutCubic(float t)
    {
        float inverse = 1f - Mathf.Clamp01(t);
        return 1f - inverse * inverse * inverse;
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
        mainCamera.fieldOfView = cameraFieldOfView;

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

        playerAgent = agent;
        agent.Configure(this, false, maxHealth);
    }

    private void SpawnEnemy()
    {
        if (enemies.Count >= GetCurrentMaxEnemies())
        {
            return;
        }

        Vector3 position = RandomPointOnStage(4f);
        if (IsTooCloseToPlayer(position, 4f))
        {
            position = MoveLocalPointAwayFromPlayer(position, 4f);
        }

        GameObject enemy = InstantiateRequired(enemyTemplate, stageRoot.TransformPoint(position) + stageRoot.up * 1.1f, Quaternion.identity, transform);
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
        if (useCircularStage)
        {
            float radius = Mathf.Max(0f, circularStageRadius - margin);
            Vector2 point = circularStageCenter + UnityEngine.Random.insideUnitCircle * radius;
            return new Vector3(point.x, 0f, point.y);
        }

        float extent = Mathf.Max(0f, fieldSize * 0.5f - margin);
        return new Vector3(UnityEngine.Random.Range(-extent, extent), 0f, UnityEngine.Random.Range(-extent, extent));
    }

    private bool IsTooCloseToPlayer(Vector3 stageLocalPosition, float minimumDistance)
    {
        if (playerBody == null || stageRoot == null)
        {
            return false;
        }

        Vector3 playerLocalPosition = stageRoot.InverseTransformPoint(playerBody.position);
        stageLocalPosition.y = 0f;
        playerLocalPosition.y = 0f;
        return Vector3.Distance(stageLocalPosition, playerLocalPosition) < minimumDistance;
    }

    private Vector3 MoveLocalPointAwayFromPlayer(Vector3 stageLocalPosition, float distance)
    {
        Vector3 playerLocalPosition = stageRoot.InverseTransformPoint(playerBody.position);
        Vector2 direction = new Vector2(stageLocalPosition.x - playerLocalPosition.x, stageLocalPosition.z - playerLocalPosition.z);
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = UnityEngine.Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }
        }

        direction.Normalize();
        Vector3 movedPosition = new Vector3(
            playerLocalPosition.x + direction.x * distance,
            stageLocalPosition.y,
            playerLocalPosition.z + direction.y * distance);

        return ClampLocalPointToStage(movedPosition, 1f);
    }

    private void ReadTiltInput()
    {
        targetTilt = Vector2.ClampMagnitude(tiltAction.ReadValue<Vector2>(), 1f);
    }

    private void UpdateTilt()
    {
        bool boost = boostAction.IsPressed();
        SetBoostMaterialColorActive(boost);
        float targetTiltDegrees = boost ? boostTiltDegrees : baseTiltDegrees;
        float smoothingFactor = 1f - Mathf.Exp(-tiltSmoothing * Time.fixedDeltaTime);
        previousSmoothedTilt = smoothedTilt;
        smoothedTilt = Vector2.Lerp(smoothedTilt, targetTilt, smoothingFactor);
        smoothedTiltDegrees = Mathf.Lerp(smoothedTiltDegrees, targetTiltDegrees, smoothingFactor);

        Quaternion targetRotation = Quaternion.Euler(smoothedTilt.y * smoothedTiltDegrees, 0f, -smoothedTilt.x * smoothedTiltDegrees);
        ApplyStageRotationAroundPlayer(targetRotation);

        float tiltSpeed = (smoothedTilt - previousSmoothedTilt).magnitude * smoothedTiltDegrees / Mathf.Max(Time.fixedDeltaTime, 0.0001f);
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
        invulnerableTimer = Mathf.Max(0f, invulnerableTimer - Time.deltaTime);
        if (!enablePushAbility)
        {
            pushCooldownTimer = 0f;
            return;
        }

        pushCooldownTimer = Mathf.Max(0f, pushCooldownTimer - Time.deltaTime);

        bool pushPressed = pushAction.WasPressedThisFrame();
        if (pushPressed && pushCooldownTimer <= 0f)
        {
            UsePush();
        }
    }

    private void UsePush()
    {
        if (!enablePushAbility)
        {
            return;
        }

        pushCooldownTimer = pushCooldownSeconds;
        StartPushCameraFovPulse();
        playerAgent?.PlayEffectParticle();

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
            float forceFactor = 1f - distance / pushRadius;
            if (forceFactor <= 0f)
            {
                continue;
            }

            enemy.MarkAffectedByPushForceField(pushRingOutAttributionSeconds);
            enemy.Body.AddForce(direction.normalized * pushImpulse * forceFactor, ForceMode.Impulse);
        }
    }

    private void UpdateHealthAndSpawns()
    {
        health -= passiveHealthDrain * Time.deltaTime;
        if (health <= 0f)
        {
            health = 0f;
            EndGame();
            return;
        }

        survivalTime += Time.deltaTime;
        UpdateSurvivalScore();

        if (enemySpawnPauseTimer > 0f)
        {
            enemySpawnPauseTimer = Mathf.Max(0f, enemySpawnPauseTimer - Time.deltaTime);
            enemySpawnTimer = 0f;
        }
        else
        {
            enemySpawnTimer += Time.deltaTime;
            GetEnemySpawnSettings(out float currentEnemySpawnInterval, out int currentEnemySpawnBatchSize);
            if (enemySpawnTimer >= currentEnemySpawnInterval)
            {
                enemySpawnTimer -= currentEnemySpawnInterval;
                SpawnEnemyBatch(currentEnemySpawnBatchSize);
            }
        }

        pickupSpawnTimer += Time.deltaTime;
        if (pickupSpawnTimer >= healthSpawnInterval)
        {
            pickupSpawnTimer = 0f;
            SpawnPickup();
        }
    }

    private void UpdateSurvivalScore()
    {
        if (survivalScoreIntervalSeconds <= 0f || survivalScorePerInterval == 0)
        {
            return;
        }

        survivalScoreTimer += Time.deltaTime;
        int intervalCount = Mathf.FloorToInt(survivalScoreTimer / survivalScoreIntervalSeconds);
        if (intervalCount <= 0)
        {
            return;
        }

        survivalScoreTimer -= intervalCount * survivalScoreIntervalSeconds;
        AddScore(intervalCount * survivalScorePerInterval);
    }

    private void AddScore(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        totalScore = Mathf.Max(0, totalScore + amount);
    }

    private void AddEnemyScore(int amount)
    {
        AddScore(amount);
        auxScore?.ShowScore(amount);
    }

    private void PauseEnemySpawns(float seconds)
    {
        enemySpawnPauseTimer = Mathf.Max(enemySpawnPauseTimer, seconds);
        enemySpawnTimer = 0f;
    }

    private void GetEnemySpawnSettings(out float currentInterval, out int currentBatchSize)
    {
        int baseBatchSize = Mathf.Max(1, enemySpawnBatchSize);
        int batchIncrease = Mathf.Max(1, enemySpawnBatchIncrease);
        int maxBatchSize = Mathf.Max(baseBatchSize, maxEnemySpawnBatchSize);
        int batchStepCount = Mathf.Max(1, ((maxBatchSize - baseBatchSize) / batchIncrease) + 1);
        int difficultyStep = enemyDifficultyStepSeconds > 0f
            ? Mathf.FloorToInt(survivalTime / enemyDifficultyStepSeconds)
            : 0;

        int batchStep = difficultyStep % batchStepCount;
        int intervalStep = difficultyStep / batchStepCount;
        currentBatchSize = Mathf.Min(maxBatchSize, baseBatchSize + batchStep * batchIncrease);
        currentInterval = Mathf.Max(minEnemySpawnInterval, enemySpawnInterval - intervalStep * enemySpawnIntervalDecreasePerCycle);
    }

    private void SpawnEnemyBatch(int batchSize)
    {
        int spawnCount = Mathf.Min(Mathf.Max(1, batchSize), GetCurrentMaxEnemies() - enemies.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    private int GetCurrentMaxEnemies()
    {
        int baseMaxEnemies = Mathf.Max(0, maxEnemies);
        if (maxEnemiesIncreaseIntervalSeconds <= 0f || maxEnemiesIncreasePerStep <= 0)
        {
            return baseMaxEnemies;
        }

        int stepCount = Mathf.FloorToInt(survivalTime / maxEnemiesIncreaseIntervalSeconds);
        int currentMaxEnemies = baseMaxEnemies + stepCount * maxEnemiesIncreasePerStep;
        if (maxEnemiesAbsoluteLimit > 0)
        {
            currentMaxEnemies = Mathf.Min(currentMaxEnemies, Mathf.Max(baseMaxEnemies, maxEnemiesAbsoluteLimit));
        }

        return currentMaxEnemies;
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
                AddEnemyScore(enemy.HasConfirmedPushRingOut(pushRingOutMinimumPlanarSpeed) ? pushRingOutScore : tiltRingOutScore);
                DestroyEnemy(enemy, true, true);
            }
        }
    }

    private bool IsOutsideField(Vector3 position)
    {
        float limit = fieldSize * 0.5f + 1.7f;
        if (useCircularStage)
        {
            Vector3 localPosition = stageRoot.InverseTransformPoint(position);
            Vector2 offset = new Vector2(localPosition.x - circularStageCenter.x, localPosition.z - circularStageCenter.y);
            float circularLimit = Mathf.Max(0f, circularStageRadius) + 1.7f;
            return offset.sqrMagnitude > circularLimit * circularLimit || position.y < stageRoot.transform.position.y - circularLimit;
        }

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

        if (amount <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - amount);
        invulnerableTimer = invulnerableSeconds;
        StartDamageCameraShake();
        GameSfxPlayer.PlayPlayerDamage();
        if (health <= 0f)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        if (gameOver)
        {
            return;
        }

        finalSurvivalTime = survivalTime;
        gameOver = true;
        RankManager.SaveRecord(totalScore, finalSurvivalTime);
        SetIngameUIVisible(false);
        GameBgmPlayer.PlayGameOver();
        gameOverUI?.Show(TimeScoreUI.FormatSurvivalTime(finalSurvivalTime), totalScore, RestartPrototype, ReturnToMainMenu);
        Time.timeScale = 0f;
        UpdateGameplayUI();
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
            DestroyEnemy(enemy, true, false);
        }
    }

    private void DestroyEnemy(RollingBallAgent enemy, bool allowDrop, bool playEffectParticle)
    {
        if (enemy == null)
        {
            return;
        }

        enemies.Remove(enemy);
        Vector3 dropPosition = enemy.transform.position;
        float destroyDelay = GameSfxPlayer.PlayEnemyDestroy(enemy);
        if (playEffectParticle)
        {
            enemy.PlayEffectParticle();
        }

        enemy.PrepareForDelayedDestroy();
        Destroy(enemy.gameObject, Mathf.Max(0.01f, destroyDelay));

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
        Vector3 pickupPosition = ClampLocalPointToStage(localPosition, fieldSize * 0.05f);
        pickupPosition.y = 0.45f;
        CreatePickup("Dropped Health Pickup", pickupPosition, Vector3.one * 0.62f);
    }

    private Vector3 ClampLocalPointToStage(Vector3 localPosition, float margin)
    {
        if (useCircularStage)
        {
            float radius = Mathf.Max(0f, circularStageRadius - margin);
            Vector2 center = circularStageCenter;
            Vector2 offset = new Vector2(localPosition.x - center.x, localPosition.z - center.y);
            if (offset.sqrMagnitude > radius * radius)
            {
                offset = offset.sqrMagnitude > 0.0001f ? offset.normalized * radius : Vector2.zero;
            }

            return new Vector3(center.x + offset.x, localPosition.y, center.y + offset.y);
        }

        float extent = Mathf.Max(0f, fieldSize * 0.5f - margin);
        return new Vector3(
            Mathf.Clamp(localPosition.x, -extent, extent),
            localPosition.y,
            Mathf.Clamp(localPosition.z, -extent, extent));
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
        upgradePoints++;
        GameSfxPlayer.PlayHealthPickup();
        pickup.PlayEffectParticle();
        Destroy(pickup.gameObject);

        if (upgradePoints >= nextUpgradeScore)
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

        float damage = -previousVelocity.y - landingDamageVelocity;
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
        GameSfxPlayer.PlayUpgradeAvailable();
        pendingUpgrades.Clear();

        List<UpgradeKind> pool = new()
        {
            UpgradeKind.ItemDensity,
            UpgradeKind.MaxHealth,
            UpgradeKind.BoostTilt,
            UpgradeKind.Luck
        };

        if (health <= 20f)
        {
            pool.Add(UpgradeKind.SpawnPause);
        }

        if (enablePushAbility)
        {
            pool.Add(UpgradeKind.PushCooldown);
            pool.Add(UpgradeKind.PushPower);
            pool.Add(UpgradeKind.PushRange);
        }

        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            pendingUpgrades.Add(CreateUpgrade(pool[index]));
            pool.RemoveAt(index);
        }

        ShowUpgradeUI();
    }

    private UpgradeOption CreateUpgrade(UpgradeKind kind)
    {
        bool highGrade = UnityEngine.Random.value < 0.16f + luck * 0.08f;
        float multiplier = highGrade ? 1.55f : 1f;
        string grade = highGrade ? "++" : "+";

        return kind switch
        {
            UpgradeKind.ItemDensity => new UpgradeOption(kind, "ingame.upgrade.category.density", "ingame.upgrade.category.density.desc", () =>
            {
                healthSpawnInterval = Mathf.Max(0.75f, healthSpawnInterval - 0.42f * multiplier);
                maxPickups += highGrade ? 4 : 2;
            }, " " + grade),
            UpgradeKind.MaxHealth => new UpgradeOption(kind, "ingame.upgrade.category.maxhealth", "ingame.upgrade.category.maxhealth.desc", () =>
            {
                float increase = 18f * multiplier;
                maxHealth += increase;
                health = Mathf.Min(maxHealth, health + increase);
            }, " " + grade),
            UpgradeKind.BoostTilt => new UpgradeOption(kind, "ingame.upgrade.category.boosttilt", "ingame.upgrade.category.boosttilt.desc", () =>
            {
                boostTiltDegrees += 3.2f * multiplier;
                suddenTiltLiftImpulse += 0.9f * multiplier;
            }, " " + grade),
            UpgradeKind.PushCooldown => new UpgradeOption(kind, "ingame.upgrade.category.pushcooldown", "ingame.upgrade.category.pushcooldown.desc", () =>
            {
                pushCooldownSeconds = Mathf.Max(1.2f, pushCooldownSeconds - 0.7f * multiplier);
            }, " " + grade),
            UpgradeKind.PushPower => new UpgradeOption(kind, "ingame.upgrade.category.pushpower", "ingame.upgrade.category.pushpower.desc", () =>
            {
                pushImpulse += 3.5f * multiplier;
            }, " " + grade),
            UpgradeKind.PushRange => new UpgradeOption(kind, "ingame.upgrade.category.pushrange", "ingame.upgrade.category.pushrange.desc", () =>
            {
                pushRadius += 0.85f * multiplier;
            }, " " + grade),
            UpgradeKind.SpawnPause => new UpgradeOption(kind, "ingame.upgrade.category.spawnpause", "ingame.upgrade.category.spawnpause.desc", () =>
            {
                PauseEnemySpawns(enemySpawnPauseUpgradeSeconds * multiplier);
            }, " " + grade, new object[] { enemySpawnPauseUpgradeSeconds * multiplier }),
            _ => new UpgradeOption(kind, "ingame.upgrade.category.luck", "ingame.upgrade.category.luck.desc", () =>
            {
                luck += 0.18f * multiplier;
            }, " " + grade)
        };
    }

    private void ShowUpgradeUI()
    {
        if (upgradeUI == null)
        {
            return;
        }

        UpgradeUI.UpgradeChoice[] choices = new UpgradeUI.UpgradeChoice[pendingUpgrades.Count];
        for (int i = 0; i < pendingUpgrades.Count; i++)
        {
            Sprite icon = upgradeItemSprite != null ? upgradeItemSprite.GetSprite(pendingUpgrades[i].Kind) : null;
            choices[i] = UpgradeUI.UpgradeChoice.Localized(
                pendingUpgrades[i].TitleKey,
                pendingUpgrades[i].DescriptionKey,
                icon,
                pendingUpgrades[i].TitleSuffix,
                pendingUpgrades[i].DescriptionArguments);
        }

        upgradeUI.ShowLocalized(level + 1, "ingame.upgrade.desc", choices, SelectUpgrade);
    }

    private void SelectUpgrade(int index)
    {
        if (!upgradeOpen || index < 0 || index >= pendingUpgrades.Count)
        {
            return;
        }

        pendingUpgrades[index].Apply();
        level++;
        nextUpgradeScore += 6 + level * 3;
        pendingUpgrades.Clear();
        upgradeOpen = false;
        upgradeUI?.Hide();
        Time.timeScale = 1f;
    }

    private void DrawUpgradePanel()
    {
        Rect panel = new Rect(Screen.width * 0.5f - 220f, Screen.height * 0.5f - 128f, 440f, 256f);
        GUI.Box(panel, GetLocalizedText("ingame.upgrade.title", level + 1));
        GUI.Label(new Rect(panel.x + 24f, panel.y + 34f, panel.width - 48f, 24f), GetLocalizedText("ingame.upgrade.desc"));

        for (int i = 0; i < pendingUpgrades.Count; i++)
        {
            UpgradeOption option = pendingUpgrades[i];
            Rect button = new Rect(panel.x + 24f, panel.y + 70f + i * 54f, panel.width - 48f, 42f);
            string title = GetLocalizedText(option.TitleKey) + option.TitleSuffix;
            string description = GetLocalizedText(option.DescriptionKey, option.DescriptionArguments);
            if (GUI.Button(button, $"{title}\n{description}"))
            {
                GameSfxPlayer.PlayUiClick();
                SelectUpgrade(i);
            }
        }
    }

    private string GetLocalizedText(string key, params object[] arguments)
    {
        LocalizedString localizedString = new(localizationTableName, key)
        {
            Arguments = arguments
        };

        return localizedString.GetLocalizedString();
    }

    private void RestartPrototype()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private readonly struct UpgradeOption
    {
        public UpgradeOption(UpgradeKind kind, string titleKey, string descriptionKey, Action apply, string titleSuffix = null, object[] descriptionArguments = null)
        {
            Kind = kind;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            TitleSuffix = titleSuffix;
            DescriptionArguments = descriptionArguments;
            Apply = apply;
        }

        public UpgradeKind Kind { get; }
        public string TitleKey { get; }
        public string DescriptionKey { get; }
        public string TitleSuffix { get; }
        public object[] DescriptionArguments { get; }
        public Action Apply { get; }
    }

    public enum UpgradeKind
    {
        ItemDensity,
        MaxHealth,
        BoostTilt,
        PushCooldown,
        PushPower,
        PushRange,
        SpawnPause,
        Luck
    }
}
