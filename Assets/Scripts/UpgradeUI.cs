using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI upgrageUITitle, upgrageUIDesc;

    [Header("Upgrade Buttons")]
    [SerializeField] private Button buttonMod1;
    [SerializeField] private TextMeshProUGUI buttonMod1Title, buttonMod1Desc;
    [SerializeField] private Image buttonSprite1;
    [Space]
    [SerializeField] private Button buttonMod2;
    [SerializeField] private TextMeshProUGUI buttonMod2Title, buttonMod2Desc;
    [SerializeField] private Image buttonSprite2;
    [Space]
    [SerializeField] private Button  buttonMod3;
    [SerializeField] private TextMeshProUGUI buttonMod3Title, buttonMod3Desc;
    [SerializeField] private Image buttonSprite3;

    private Button[] buttons;
    private TextMeshProUGUI[] buttonTitles;
    private TextMeshProUGUI[] buttonDescriptions;
    private Image[] buttonSprites;
    private Action<int> onSelected;

    private void Awake()
    {
        CacheControls();
        Hide();
    }

    public void Show(int nextLevel, string description, UpgradeChoice[] choices, Action<int> selectedCallback)
    {
        CacheControls();
        onSelected = selectedCallback;
        gameObject.SetActive(true);

        SetText(upgrageUITitle, $"Level {nextLevel} Upgrade");
        SetText(upgrageUIDesc, description);

        for (int i = 0; i < buttons.Length; i++)
        {
            bool hasChoice = choices != null && i < choices.Length;
            buttons[i].gameObject.SetActive(hasChoice);
            buttons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
            {
                SetSprite(buttonSprites[i], null);
                continue;
            }

            int choiceIndex = i;
            SetText(buttonTitles[i], choices[i].Title);
            SetText(buttonDescriptions[i], choices[i].Description);
            SetSprite(buttonSprites[i], choices[i].Icon);
            buttons[i].onClick.AddListener(() => Select(choiceIndex));
        }

        SelectDefaultButton();
    }

    public void Hide()
    {
        CacheControls();
        onSelected = null;
        gameObject.SetActive(false);
    }

    private void Select(int choiceIndex)
    {
        onSelected?.Invoke(choiceIndex);
    }

    private void CacheControls()
    {
        buttons ??= new[] { buttonMod1, buttonMod2, buttonMod3 };
        buttonTitles ??= new[] { buttonMod1Title, buttonMod2Title, buttonMod3Title };
        buttonDescriptions ??= new[] { buttonMod1Desc, buttonMod2Desc, buttonMod3Desc };
        buttonSprites ??= new[] { buttonSprite1, buttonSprite2, buttonSprite3 };
    }

    private void SelectDefaultButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null || !buttons[i].gameObject.activeInHierarchy || !buttons[i].interactable)
            {
                continue;
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttons[i].gameObject);
            buttons[i].Select();
            return;
        }
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private static void SetSprite(Image image, Sprite sprite)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.enabled = sprite != null;
        image.preserveAspect = true;
    }

    public readonly struct UpgradeChoice
    {
        public UpgradeChoice(string title, string description)
            : this(title, description, null)
        {
        }

        public UpgradeChoice(string title, string description, Sprite icon)
        {
            Title = title;
            Description = description;
            Icon = icon;
        }

        public string Title { get; }
        public string Description { get; }
        public Sprite Icon { get; }
    }
}
