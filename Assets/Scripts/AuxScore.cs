using TMPro;
using UnityEngine;

public sealed class AuxScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float visibleSeconds = 0.75f;
    [SerializeField] private float fadeSeconds = 0.25f;

    private float remainingSeconds;
    private int displayedScore;
    private Color baseColor = Color.white;
    private bool baseColorCached;

    private void Awake()
    {
        CacheText();
        HideImmediate();
    }

    private void Update()
    {
        if (remainingSeconds <= 0f)
        {
            return;
        }

        remainingSeconds = Mathf.Max(0f, remainingSeconds - Time.unscaledDeltaTime);
        float alpha = 1f;
        if (fadeSeconds > 0f && remainingSeconds < fadeSeconds)
        {
            alpha = remainingSeconds / fadeSeconds;
        }

        SetAlpha(alpha);
        if (remainingSeconds <= 0f)
        {
            HideImmediate();
        }
    }

    public void ShowScore(int score)
    {
        if (score <= 0)
        {
            return;
        }

        CacheText();
        bool shouldAccumulate = gameObject.activeSelf && remainingSeconds > 0f;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        displayedScore = shouldAccumulate ? displayedScore + score : score;
        if (scoreText != null)
        {
            scoreText.text = $"+{displayedScore:N0}";
        }

        remainingSeconds = Mathf.Max(0f, visibleSeconds) + Mathf.Max(0f, fadeSeconds);
        SetAlpha(1f);
    }

    public void HideImmediate()
    {
        remainingSeconds = 0f;
        displayedScore = 0;
        if (scoreText != null)
        {
            scoreText.text = string.Empty;
            SetAlpha(0f);
        }

        gameObject.SetActive(false);
    }

    private void CacheText()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }

        if (scoreText == null || baseColorCached)
        {
            return;
        }

        baseColor = scoreText.color;
        baseColorCached = true;
    }

    private void SetAlpha(float alpha)
    {
        if (scoreText == null)
        {
            return;
        }

        Color color = baseColorCached ? baseColor : scoreText.color;
        color.a = Mathf.Clamp01(alpha);
        scoreText.color = color;
    }
}
