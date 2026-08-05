using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Image pushCooldownFillImage;
    [SerializeField] private TextMeshProUGUI healthLevelText;

    public void UpdateHealth(float health, float maxHealth, bool pushEnabled, float pushCooldownTimer, float pushCooldownSeconds)
    {
        SetRadialFill(healthFillImage, maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f);
        SetRadialFill(pushCooldownFillImage, GetPushCooldownPercent(pushEnabled, pushCooldownTimer, pushCooldownSeconds));
        SetText(healthLevelText, $"{Mathf.CeilToInt(health)}");
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
}
