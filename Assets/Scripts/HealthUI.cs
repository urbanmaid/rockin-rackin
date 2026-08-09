using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image pushCooldownFillImage;
    [SerializeField] private TextMeshProUGUI healthLevelText;

    [Header("Low Health Warning")]
    [SerializeField] private bool enableLowHealthWarning = true;
    [SerializeField] private float lowHealthWarningThreshold = 20f;
    [SerializeField] private Color lowHealthWarningColor = Color.red;
    [SerializeField] private float lowHealthWarningBlinkInterval = 0.18f;

    private Color initialHealthFillColor = Color.white;
    private Color initialHealthLevelTextColor = Color.white;
    private bool hasInitialColors;

    private void Awake()
    {
        CacheInitialColors();
    }

    private void OnDisable()
    {
        RestoreInitialColors();
    }

    public void UpdateHealth(float health, float maxHealth, bool pushEnabled, float pushCooldownTimer, float pushCooldownSeconds)
    {
        CacheInitialColors();
        SetRadialFill(healthFillImage, maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f);
        SetRadialFill(pushCooldownFillImage, GetPushCooldownPercent(pushEnabled, pushCooldownTimer, pushCooldownSeconds));
        SetText(healthLevelText, $"{Mathf.CeilToInt(health)}");
        UpdateLowHealthWarning(health);
    }

    private static float GetPushCooldownPercent(bool pushEnabled, float pushCooldownTimer, float pushCooldownSeconds)
    {
        if (!pushEnabled)
        {
            return 0f;
        }

        if (pushCooldownSeconds <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01(1f - pushCooldownTimer / pushCooldownSeconds);
    }

    private static void SetRadialFill(Image image, float fillAmount)
    {
        if (image == null)
        {
            return;
        }

        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Radial360;
        image.fillAmount = Mathf.Clamp01(fillAmount);
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void UpdateLowHealthWarning(float health)
    {
        if (!enableLowHealthWarning || health > lowHealthWarningThreshold)
        {
            RestoreInitialColors();
            return;
        }

        float interval = Mathf.Max(0.01f, lowHealthWarningBlinkInterval);
        bool showWarningColor = Mathf.FloorToInt(Time.unscaledTime / interval) % 2 == 0;
        Color currentColor = showWarningColor ? lowHealthWarningColor : initialHealthFillColor;

        SetColor(healthFillImage, currentColor);
        SetColor(healthLevelText, showWarningColor ? lowHealthWarningColor : initialHealthLevelTextColor);
    }

    private void CacheInitialColors()
    {
        if (hasInitialColors)
        {
            return;
        }

        if (healthFillImage != null)
        {
            initialHealthFillColor = healthFillImage.color;
        }

        if (healthLevelText != null)
        {
            initialHealthLevelTextColor = healthLevelText.color;
        }

        hasInitialColors = true;
    }

    private void RestoreInitialColors()
    {
        if (!hasInitialColors)
        {
            return;
        }

        SetColor(healthFillImage, initialHealthFillColor);
        SetColor(healthLevelText, initialHealthLevelTextColor);
    }

    private static void SetColor(Graphic graphic, Color color)
    {
        if (graphic != null)
        {
            graphic.color = color;
        }
    }
}
