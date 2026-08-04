using System;
using UnityEngine;
using TMPro;

public class RankUIElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI estimateTimeText;
    [SerializeField] private TextMeshProUGUI dateText;

    public void SetRecordText(RankElement r)
    {
        if(scoreText != null && estimateTimeText != null && dateText != null)
        {
            scoreText.text = r.score.ToString("N0");
            estimateTimeText.text = FormatEstimatedTime(r.estimatedTime);
            dateText.text = FormatRecordedDate(r.dateRecorded);
        }
    }

    private static string FormatEstimatedTime(float seconds)
    {
        float clampedSeconds = Mathf.Max(0f, seconds);
        int centiseconds = Mathf.FloorToInt(clampedSeconds * 100f);
        int minutes = centiseconds / 6000;
        int remainingSeconds = centiseconds / 100 % 60;
        int remainingCentiseconds = centiseconds % 100;
        return $"{minutes:00}:{remainingSeconds:00}.{remainingCentiseconds:00}";
    }

    private static string FormatRecordedDate(int unixTimeSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }
}
