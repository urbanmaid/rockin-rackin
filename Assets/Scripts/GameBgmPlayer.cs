using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public sealed class GameBgmPlayer : MonoBehaviour
{
    private enum BgmMode
    {
        None,
        MainMenu,
        Gameplay,
        GameOver
    }

    public static GameBgmPlayer Instance { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup outputAudioMixerGroup;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private float fadeSeconds = 0.35f;

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip[] gameplayMusicClips;
    [SerializeField] private AudioClip gameOverMusic;

    private static BgmMode requestedMode = BgmMode.MainMenu;

    private Coroutine fadeRoutine;
    private BgmMode currentMode = BgmMode.None;
    private int lastGameplayMusicIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheAudioSource();
        PlayRequestedMode();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static void PlayMainMenu()
    {
        requestedMode = BgmMode.MainMenu;
        Instance?.PlayRequestedMode();
    }

    public static void PlayGameplay()
    {
        requestedMode = BgmMode.Gameplay;
        Instance?.PlayRequestedMode();
    }

    public static void PlayGameOver()
    {
        requestedMode = BgmMode.GameOver;
        Instance?.PlayRequestedMode();
    }

    private void PlayRequestedMode()
    {
        switch (requestedMode)
        {
            case BgmMode.MainMenu:
                PlayMode(BgmMode.MainMenu, mainMenuMusic);
                break;
            case BgmMode.Gameplay:
                PlayMode(BgmMode.Gameplay, GetGameplayMusicClip());
                break;
            case BgmMode.GameOver:
                PlayMode(BgmMode.GameOver, gameOverMusic);
                break;
            default:
                StopMusic();
                break;
        }
    }

    private void PlayMode(BgmMode mode, AudioClip clip)
    {
        CacheAudioSource();

        if (clip == null)
        {
            StopMusic();
            currentMode = mode;
            return;
        }

        if (currentMode == mode && audioSource.clip == clip && audioSource.isPlaying)
        {
            audioSource.volume = volume;
            return;
        }

        currentMode = mode;
        StartFadeToClip(clip);
    }

    private AudioClip GetGameplayMusicClip()
    {
        if (gameplayMusicClips == null || gameplayMusicClips.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, gameplayMusicClips.Length);
        if (gameplayMusicClips.Length > 1 && index == lastGameplayMusicIndex)
        {
            index = (index + 1) % gameplayMusicClips.Length;
        }

        lastGameplayMusicIndex = index;
        return gameplayMusicClips[index];
    }

    private void StartFadeToClip(AudioClip clip)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeToClip(clip));
    }

    private IEnumerator FadeToClip(AudioClip clip)
    {
        float duration = Mathf.Max(0f, fadeSeconds);
        if (duration > 0f && audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = duration > 0f ? 0f : volume;
        audioSource.Play();

        if (duration > 0f)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(0f, volume, elapsed / duration);
                yield return null;
            }
        }

        audioSource.volume = volume;
        fadeRoutine = null;
    }

    private void StopMusic()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    private void CacheAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = outputAudioMixerGroup;
    }
}
