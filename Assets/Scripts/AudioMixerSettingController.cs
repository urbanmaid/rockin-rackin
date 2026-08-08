using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public sealed class AudioMixerSettingController : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup targetMixerGroup;
    [SerializeField] private AudioMixer targetMixer;
    [SerializeField] private string exposedVolumeParameter = "Volume";

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Value")]
    [SerializeField, Range(0, 100)] private int defaultValue = 100;
    [SerializeField] private bool saveToPlayerPrefs;
    [SerializeField] private string playerPrefsKey = "AudioVolume";

    [Header("Decibel Range")]
    [SerializeField] private float minimumDecibels = -80f;
    [SerializeField] private float maximumDecibels = 0f;

    public int CurrentValue { get; private set; }

    private AudioMixer Mixer => targetMixer != null ? targetMixer : targetMixerGroup != null ? targetMixerGroup.audioMixer : null;

    private void Awake()
    {
        CacheReferences();
        SetupSlider();
        SetValue(GetInitialValue(), false);
    }

    private void OnEnable()
    {
        CacheReferences();

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        SetValue(CurrentValue, false);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    public void SetValue(float value)
    {
        SetValue(Mathf.RoundToInt(value), true);
    }

    public void SetValue(int value)
    {
        SetValue(value, true);
    }

    private void OnSliderValueChanged(float value)
    {
        SetValue(value);
    }

    private void SetValue(int value, bool persist)
    {
        CurrentValue = Mathf.Clamp(value, 0, 100);

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(CurrentValue);
        }

        ApplyMixerVolume(CurrentValue);
        UpdateValueText(CurrentValue);

        if (persist && saveToPlayerPrefs && !string.IsNullOrWhiteSpace(playerPrefsKey))
        {
            PlayerPrefs.SetInt(playerPrefsKey, CurrentValue);
            PlayerPrefs.Save();
        }
    }

    private int GetInitialValue()
    {
        if (saveToPlayerPrefs && !string.IsNullOrWhiteSpace(playerPrefsKey) && PlayerPrefs.HasKey(playerPrefsKey))
        {
            return PlayerPrefs.GetInt(playerPrefsKey);
        }

        return defaultValue;
    }

    private void ApplyMixerVolume(int value)
    {
        AudioMixer mixer = Mixer;
        if (mixer == null || string.IsNullOrWhiteSpace(exposedVolumeParameter))
        {
            return;
        }

        mixer.SetFloat(exposedVolumeParameter, ConvertValueToDecibels(value));
    }

    private float ConvertValueToDecibels(int value)
    {
        if (value <= 0)
        {
            return minimumDecibels;
        }

        float normalizedValue = Mathf.Clamp01(value / 100f);
        float decibels = Mathf.Log10(normalizedValue) * 20f;
        return Mathf.Clamp(decibels, minimumDecibels, maximumDecibels);
    }

    private void UpdateValueText(int value)
    {
        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
    }

    private void SetupSlider()
    {
        if (volumeSlider == null)
        {
            return;
        }

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 100f;
        volumeSlider.wholeNumbers = true;
    }

    private void CacheReferences()
    {
        if (volumeSlider == null)
        {
            volumeSlider = GetComponentInChildren<Slider>(true);
        }
    }
}
