using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public sealed class LocaleDropdownUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown localeDropdown;
    [SerializeField] private bool useNativeLocaleName = true;
    [SerializeField] private bool saveSelectedLocale = true;
    [SerializeField] private string playerPrefsKey = "SelectedLocale";

    private readonly List<Locale> locales = new();
    private bool isChangingDropdown;

    private void Awake()
    {
        CacheDropdown();
    }

    private void OnEnable()
    {
        CacheDropdown();
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        StartCoroutine(InitializeDropdown());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;

        if (localeDropdown != null)
        {
            localeDropdown.onValueChanged.RemoveListener(ChangeLocale);
        }
    }

    public void ChangeLocale(int optionIndex)
    {
        if (isChangingDropdown || optionIndex < 0 || optionIndex >= locales.Count)
        {
            return;
        }

        Locale selectedLocale = locales[optionIndex];
        if (LocalizationSettings.SelectedLocale == selectedLocale)
        {
            return;
        }

        LocalizationSettings.SelectedLocale = selectedLocale;

        if (saveSelectedLocale && !string.IsNullOrWhiteSpace(playerPrefsKey))
        {
            PlayerPrefs.SetString(playerPrefsKey, selectedLocale.Identifier.Code);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator InitializeDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        RefreshLocaleOptions();
        ApplySavedLocale();
        SyncDropdownValue(LocalizationSettings.SelectedLocale);
        BindDropdown();
    }

    private void RefreshLocaleOptions()
    {
        locales.Clear();

        if (localeDropdown == null || LocalizationSettings.AvailableLocales == null)
        {
            return;
        }

        localeDropdown.ClearOptions();
        List<string> options = new();
        IList<Locale> availableLocales = LocalizationSettings.AvailableLocales.Locales;
        for (int i = 0; i < availableLocales.Count; i++)
        {
            Locale locale = availableLocales[i];
            if (locale == null)
            {
                continue;
            }

            locales.Add(locale);
            options.Add(GetLocaleDisplayName(locale));
        }

        localeDropdown.AddOptions(options);
    }

    private void ApplySavedLocale()
    {
        if (!saveSelectedLocale || string.IsNullOrWhiteSpace(playerPrefsKey) || !PlayerPrefs.HasKey(playerPrefsKey))
        {
            return;
        }

        string savedLocaleCode = PlayerPrefs.GetString(playerPrefsKey);
        for (int i = 0; i < locales.Count; i++)
        {
            if (locales[i].Identifier.Code == savedLocaleCode)
            {
                LocalizationSettings.SelectedLocale = locales[i];
                return;
            }
        }
    }

    private void BindDropdown()
    {
        if (localeDropdown == null)
        {
            return;
        }

        localeDropdown.onValueChanged.RemoveListener(ChangeLocale);
        localeDropdown.onValueChanged.AddListener(ChangeLocale);
    }

    private void OnSelectedLocaleChanged(Locale locale)
    {
        SyncDropdownValue(locale);
    }

    private void SyncDropdownValue(Locale locale)
    {
        if (localeDropdown == null || locale == null)
        {
            return;
        }

        int index = locales.IndexOf(locale);
        if (index < 0)
        {
            return;
        }

        isChangingDropdown = true;
        localeDropdown.SetValueWithoutNotify(index);
        localeDropdown.RefreshShownValue();
        isChangingDropdown = false;
    }

    private string GetLocaleDisplayName(Locale locale)
    {
        if (locale == null)
        {
            return string.Empty;
        }

        if (useNativeLocaleName && locale.Identifier.CultureInfo != null)
        {
            return locale.Identifier.CultureInfo.NativeName;
        }

        return locale.LocaleName;
    }

    private void CacheDropdown()
    {
        if (localeDropdown == null)
        {
            localeDropdown = GetComponentInChildren<TMP_Dropdown>(true);
        }
    }
}
