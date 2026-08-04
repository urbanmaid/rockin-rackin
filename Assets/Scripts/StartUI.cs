using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [Header("Navigation Buttons")]
    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonPlayTips;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonExit;

    [Header("Start related UI objects")]
    [SerializeField] private GameObject playTipsUI;
    [SerializeField] private GameObject settingsUI;

    [Header("Ranks")]
    [SerializeField] private RankUI rankUI;

    void Start()
    {
        rankUI?.GenerateRankBoard();
    }

    // TODO Methods:
    // Start game
    // Show play tips and controls
    // Show settings UI
    // Exit game
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
