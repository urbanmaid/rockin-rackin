using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    private const string UpgradeTitleKey = "ingame.upgrade.title";

    [SerializeField] private TextMeshProUGUI upgrageUITitle, upgrageUIDesc;
    [SerializeField] private string localizationTableName = "DefaultLocale";

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
    private int currentNextLevel;
    private string currentDescription;
    private string currentDescriptionKey;
    private UpgradeChoice[] currentChoices;
    private bool useLocalizedContent;

    private void Awake()
    {
        CacheControls();
        Hide();
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    public void Show(int nextLevel, string description, UpgradeChoice[] choices, Action<int> selectedCallback)
    {
        CacheControls();
        onSelected = selectedCallback;
        currentNextLevel = nextLevel;
        currentDescription = description;
        currentDescriptionKey = null;
        currentChoices = choices;
        useLocalizedContent = false;
        gameObject.SetActive(true);

        RefreshTexts();
        SelectDefaultButton();
    }

    public void ShowLocalized(int nextLevel, string descriptionKey, UpgradeChoice[] choices, Action<int> selectedCallback)
    {
        CacheControls();
        onSelected = selectedCallback;
        currentNextLevel = nextLevel;
        currentDescription = null;
        currentDescriptionKey = descriptionKey;
        currentChoices = choices;
        useLocalizedContent = true;
        gameObject.SetActive(true);

        RefreshTexts();
        SelectDefaultButton();
    }

    private void RefreshTexts()
    {
        SetText(upgrageUITitle, useLocalizedContent ? GetLocalizedText(UpgradeTitleKey, currentNextLevel) : $"Level {currentNextLevel} Upgrade");
        SetText(upgrageUIDesc, useLocalizedContent ? GetLocalizedText(currentDescriptionKey) : currentDescription);

        for (int i = 0; i < buttons.Length; i++)
        {
            bool hasChoice = currentChoices != null && i < currentChoices.Length;
            buttons[i].gameObject.SetActive(hasChoice);
            buttons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
            {
                SetSprite(buttonSprites[i], null);
                continue;
            }

            int choiceIndex = i;
            SetText(buttonTitles[i], currentChoices[i].GetTitle(localizationTableName));
            SetText(buttonDescriptions[i], currentChoices[i].GetDescription(localizationTableName));
            SetSprite(buttonSprites[i], currentChoices[i].Icon);
            buttons[i].onClick.AddListener(() => Select(choiceIndex));
        }
    }

    public void Hide()
    {
        CacheControls();
        onSelected = null;
        currentChoices = null;
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

    private void OnSelectedLocaleChanged(Locale locale)
    {
        if (gameObject.activeInHierarchy && useLocalizedContent)
        {
            RefreshTexts();
        }
    }

    private string GetLocalizedText(string key, params object[] arguments)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        LocalizedString localizedString = new(localizationTableName, key)
        {
            Arguments = arguments
        };

        return localizedString.GetLocalizedString();
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
            TitleKey = null;
            DescriptionKey = null;
            TitleSuffix = null;
            TitleArguments = null;
            DescriptionArguments = null;
        }

        public string Title { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public string TitleKey { get; }
        public string DescriptionKey { get; }
        public string TitleSuffix { get; }
        public object[] TitleArguments { get; }
        public object[] DescriptionArguments { get; }

        public static UpgradeChoice Localized(string titleKey, string descriptionKey, Sprite icon, string titleSuffix = null, object[] descriptionArguments = null)
        {
            return new UpgradeChoice(titleKey, descriptionKey, icon, titleSuffix, null, descriptionArguments);
        }

        public string GetTitle(string tableName)
        {
            if (string.IsNullOrWhiteSpace(TitleKey))
            {
                return Title;
            }

            return GetLocalizedText(tableName, TitleKey, TitleArguments) + TitleSuffix;
        }

        public string GetDescription(string tableName)
        {
            if (string.IsNullOrWhiteSpace(DescriptionKey))
            {
                return Description;
            }

            return GetLocalizedText(tableName, DescriptionKey, DescriptionArguments);
        }

        private UpgradeChoice(string titleKey, string descriptionKey, Sprite icon, string titleSuffix, object[] titleArguments, object[] descriptionArguments)
        {
            Title = null;
            Description = null;
            Icon = icon;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            TitleSuffix = titleSuffix;
            TitleArguments = titleArguments;
            DescriptionArguments = descriptionArguments;
        }

        private static string GetLocalizedText(string tableName, string key, object[] arguments)
        {
            LocalizedString localizedString = new(tableName, key)
            {
                Arguments = arguments
            };

            return localizedString.GetLocalizedString();
        }
    }
}
