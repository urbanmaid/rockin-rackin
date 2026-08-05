using TMPro;
using UnityEngine;

public sealed class LevelCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI levelProgressText;

    public void UpdateLevel(int level, int currentPoints, int nextLevelPoints)
    {
        SetText(levelText, $"Level {level}");
        SetText(levelProgressText, $"{currentPoints} / {nextLevelPoints}");
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
