using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResetRecordButton : MonoBehaviour
{
    [SerializeField] private RankUI rankUI;

    private Button button;
    private Toggle toggle;

    private void Awake()
    {
        CacheElement();
    }

    private void OnEnable()
    {
        CacheElement();
        button.onClick.RemoveListener(ResetRecords);
        button.onClick.AddListener(ResetRecords);
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(ResetRecords);
        }
    }

    public void ResetRecords()
    {
        if(toggle.enabled)
        {
            RankManager.ResetRecords();
            rankUI?.GenerateRankBoard();
        }
    }

    private void CacheElement()
    {
        if (button == null)
        {
            button = GetComponentInChildren<Button>();
        }

        if (toggle == null)
        {
            toggle = GetComponentInChildren<Toggle>();
        }
    }
}
