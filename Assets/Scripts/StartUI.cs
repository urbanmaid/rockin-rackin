using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private RockinRackinPrototype prototype;
    [SerializeField] private bool createFallbackStartButton = true;

    [Header("Navigation Buttons")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonPlayTips;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonExit;

    [Header("Start related UI objects")]
    [SerializeField] private GameObject mainUI;
    [SerializeField] private GameObject playTipsUI;
    [SerializeField] private GameObject settingsUI;

    [Header("Ranks")]
    [SerializeField] private RankUI rankUI;

    private void Awake()
    {
        CacheReferences();
        BindButtons();
        ShowMainUI();
    }

    private void Start()
    {
        gameObject.SetActive(true);
        rankUI?.GenerateRankBoard();
        SelectDefaultButton();
    }

    private void OnEnable()
    {
        SelectDefaultButton();
    }

    private void OnDestroy()
    {
        if (buttonStart != null)
        {
            buttonStart.onClick.RemoveListener(StartGame);
        }

        if (buttonPlayTips != null)
        {
            buttonPlayTips.onClick.RemoveListener(TogglePlayTips);
        }

        if (buttonSettings != null)
        {
            buttonSettings.onClick.RemoveListener(ToggleSettings);
        }

        if (buttonExit != null)
        {
            buttonExit.onClick.RemoveListener(ExitGame);
        }
    }

    public void StartGame()
    {
        CacheReferences();
        prototype?.BeginGame();
        gameObject.SetActive(false);
    }

    public void TogglePlayTips()
    {
        if (playTipsUI != null && playTipsUI.activeSelf)
        {
            ShowMainUI();
            return;
        }

        ShowOnly(playTipsUI);
    }

    public void ToggleSettings()
    {
        if (settingsUI != null && settingsUI.activeSelf)
        {
            ShowMainUI();
            return;
        }

        ShowOnly(settingsUI);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void CacheReferences()
    {
        if (prototype == null)
        {
            prototype = FindAnyObjectByType<RockinRackinPrototype>();
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        buttonStart ??= FindButton(buttons, "start", "play", "begin");
        buttonPlayTips ??= FindButton(buttons, "tip", "control", "help");
        buttonSettings ??= FindButton(buttons, "setting", "option");
        buttonExit ??= FindButton(buttons, "exit", "quit");

        if (buttonSettings == buttonPlayTips)
        {
            buttonSettings = FindButtonExcept(buttons, buttonPlayTips, "setting", "option");
        }

        if (buttonStart == null && buttons.Length > 0)
        {
            buttonStart = buttons[0];
        }

        if (buttonStart == null && createFallbackStartButton)
        {
            buttonStart = CreateFallbackStartButton();
        }
    }

    private void BindButtons()
    {
        if (buttonStart != null)
        {
            buttonStart.onClick.RemoveListener(StartGame);
            buttonStart.onClick.AddListener(StartGame);
        }

        if (buttonPlayTips != null)
        {
            buttonPlayTips.onClick.RemoveListener(TogglePlayTips);
            buttonPlayTips.onClick.AddListener(TogglePlayTips);
        }

        if (buttonSettings != null)
        {
            buttonSettings.onClick.RemoveListener(ToggleSettings);
            buttonSettings.onClick.AddListener(ToggleSettings);
        }

        if (buttonExit != null)
        {
            buttonExit.onClick.RemoveListener(ExitGame);
            buttonExit.onClick.AddListener(ExitGame);
        }
    }

    private static Button FindButton(Button[] buttons, params string[] nameHints)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            string buttonName = buttons[i].name.ToLowerInvariant();
            for (int hintIndex = 0; hintIndex < nameHints.Length; hintIndex++)
            {
                if (buttonName.Contains(nameHints[hintIndex]))
                {
                    return buttons[i];
                }
            }
        }

        return null;
    }

    private static Button FindButtonExcept(Button[] buttons, Button excludedButton, params string[] nameHints)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == excludedButton)
            {
                continue;
            }

            string buttonName = buttons[i].name.ToLowerInvariant();
            for (int hintIndex = 0; hintIndex < nameHints.Length; hintIndex++)
            {
                if (buttonName.Contains(nameHints[hintIndex]))
                {
                    return buttons[i];
                }
            }
        }

        return null;
    }

    private Button CreateFallbackStartButton()
    {
        GameObject buttonObject = new GameObject("Start Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.layer = gameObject.layer;
        buttonObject.transform.SetParent(transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = new Vector2(320f, 86f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.96f, 0.96f, 0.96f, 0.95f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.layer = buttonObject.layer;
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "Start";
        label.fontSize = 34f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.black;
        label.raycastTarget = false;

        return button;
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null || buttonStart == null || !buttonStart.gameObject.activeInHierarchy || !buttonStart.interactable)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(buttonStart.gameObject);
        buttonStart.Select();
    }

    private void ShowMainUI()
    {
        ShowOnly(mainUI);
    }

    private void ShowOnly(GameObject visiblePanel)
    {
        SetPanelVisible(mainUI, visiblePanel == mainUI || visiblePanel == null);
        SetPanelVisible(playTipsUI, visiblePanel == playTipsUI);
        SetPanelVisible(settingsUI, visiblePanel == settingsUI);
        SelectDefaultButton();
    }

    private static void SetPanelVisible(GameObject panel, bool visible)
    {
        panel?.SetActive(visible);
    }
}

[Serializable]
public class RankElement
{
    public int score;
    public float estimatedTime;
    public int dateRecorded; // UNIX time
}

public static class RankManager
{
    private const string RecordFileName = "rank_records.json";
    private const int MaxRecordCount = 10;

    public static string RecordFilePath => Path.Combine(Application.persistentDataPath, RecordFileName);

    public static List<RankElement> LoadRecords()
    {
        return new List<RankElement>(LoadRecordList().records);
    }

    public static void SaveRecord(int score, float estimatedTime)
    {
        SaveRecord(new RankElement
        {
            score = Mathf.Max(0, score),
            estimatedTime = Mathf.Max(0f, estimatedTime),
            dateRecorded = GetCurrentUnixTimeSeconds()
        });
    }

    public static void SaveRecord(RankElement record)
    {
        if (record == null)
        {
            return;
        }

        RankRecordList recordList = LoadRecordList();
        recordList.records.Add(record);
        SortAndTrim(recordList.records);
        SaveRecordList(recordList);
    }

    public static void ClearRecords()
    {
        if (File.Exists(RecordFilePath))
        {
            File.Delete(RecordFilePath);
        }
    }

    private static RankRecordList LoadRecordList()
    {
        if (!File.Exists(RecordFilePath))
        {
            return new RankRecordList();
        }

        try
        {
            string json = File.ReadAllText(RecordFilePath);
            RankRecordList recordList = JsonUtility.FromJson<RankRecordList>(json);
            recordList ??= new RankRecordList();
            recordList.records ??= new List<RankElement>();
            SortAndTrim(recordList.records);
            return recordList;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to load rank records from {RecordFilePath}: {exception.Message}");
            return new RankRecordList();
        }
    }

    private static void SaveRecordList(RankRecordList recordList)
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(RecordFilePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonUtility.ToJson(recordList, true);
            File.WriteAllText(RecordFilePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to save rank records to {RecordFilePath}: {exception.Message}");
        }
    }

    private static void SortAndTrim(List<RankElement> records)
    {
        records.Sort(CompareRecords);

        while (records.Count > MaxRecordCount)
        {
            records.RemoveAt(records.Count - 1);
        }
    }

    private static int CompareRecords(RankElement left, RankElement right)
    {
        int scoreComparison = right.score.CompareTo(left.score);
        if (scoreComparison != 0)
        {
            return scoreComparison;
        }

        int timeComparison = right.estimatedTime.CompareTo(left.estimatedTime);
        if (timeComparison != 0)
        {
            return timeComparison;
        }

        return right.dateRecorded.CompareTo(left.dateRecorded);
    }

    private static int GetCurrentUnixTimeSeconds()
    {
        long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return unixTime > int.MaxValue ? int.MaxValue : (int)unixTime;
    }

    [Serializable]
    private sealed class RankRecordList
    {
        public List<RankElement> records = new();
    }
}
