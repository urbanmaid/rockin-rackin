using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over Descriptions")]

    [SerializeField] private TextMeshProUGUI textEstimatedTime;
    [SerializeField] private TextMeshProUGUI textScore;

    [Header("Navigation Buttons")]
    [SerializeField] private Button buttonRetry;
    [SerializeField] private Button buttonMainMenu;

    private Action onRetryRequested;
    private Action onMainMenuRequested;
    private bool controlsCached;

    private void Awake()
    {
        CacheControls();
        Hide();
    }

    private void OnDestroy()
    {
        if (buttonRetry != null)
        {
            buttonRetry.onClick.RemoveListener(Retry);
        }

        if (buttonMainMenu != null)
        {
            buttonMainMenu.onClick.RemoveListener(GoToMainMenu);
        }
    }

    public void Show(string estimatedTime, Action retryCallback, Action mainMenuCallback)
    {
        CacheControls();
        onRetryRequested = retryCallback;
        onMainMenuRequested = mainMenuCallback;
        gameObject.SetActive(true);

        SetText(textEstimatedTime, estimatedTime);
    }

    public void Hide()
    {
        onRetryRequested = null;
        onMainMenuRequested = null;
        gameObject.SetActive(false);
    }

    private void Retry()
    {
        onRetryRequested?.Invoke();
    }

    private void GoToMainMenu()
    {
        onMainMenuRequested?.Invoke();
    }

    private void CacheControls()
    {
        if (controlsCached)
        {
            return;
        }

        controlsCached = true;

        if (buttonRetry != null)
        {
            buttonRetry.onClick.AddListener(Retry);
        }

        if (buttonMainMenu != null)
        {
            buttonMainMenu.onClick.AddListener(GoToMainMenu);
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
