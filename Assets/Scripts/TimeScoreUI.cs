using TMPro;
using UnityEngine;

public sealed class TimeScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private TextMeshProUGUI totalScoreText;

    public void UpdateTimeScore(float survivalTime, int totalScore)
    {
        SetText(survivalTimeText, FormatSurvivalTime(survivalTime));
        SetText(totalScoreText, $"{totalScore:N0}");
    }

    public static string FormatSurvivalTime(float seconds)
    {
        float clampedSeconds = Mathf.Max(0f, seconds);
        int centiseconds = Mathf.FloorToInt(clampedSeconds * 100f);
        int minutes = centiseconds / 6000;
        int remainingSeconds = centiseconds / 100 % 60;
        int remainingCentiseconds = centiseconds % 100;
        return $"{minutes:00}:{remainingSeconds:00}.{remainingCentiseconds:00}";
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
