using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameSfxPlayer : MonoBehaviour
{
    public static GameSfxPlayer Instance { get; private set; }

    [Header("Output")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup outputAudioMixerGroup;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("UI")]
    [SerializeField] private AudioClip uiClickClip;

    [Header("Gameplay")]
    [SerializeField] private AudioClip[] healthPickupClips;
    [SerializeField] private AudioClip[] playerDamageClips;
    [SerializeField] private AudioClip[] enemyDestroyClips;
    [SerializeField] private AudioClip upgradeAvailableClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheAudioSource();
        BindAllButtonClickSounds();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        BindAllButtonClickSounds();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void PlayUiClick()
    {
        Instance?.PlayOneShot(Instance.uiClickClip);
    }

    public static void PlayHealthPickup()
    {
        Instance?.PlayRandom(Instance.healthPickupClips);
    }

    public static void PlayPlayerDamage()
    {
        Instance?.PlayRandom(Instance.playerDamageClips);
    }

    public static float PlayEnemyDestroy(RollingBallAgent enemy)
    {
        if (Instance == null || enemy == null)
        {
            return 0f;
        }

        AudioClip clip = Instance.GetRandomClip(Instance.enemyDestroyClips);
        return enemy.PlaySfx(clip, Instance.volume, Instance.outputAudioMixerGroup);
    }

    public static void PlayUpgradeAvailable()
    {
        Instance?.PlayOneShot(Instance.upgradeAvailableClip);
    }

    public void BindAllButtonClickSounds()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.RemoveListener(PlayUiClick);
            buttons[i].onClick.AddListener(PlayUiClick);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindAllButtonClickSounds();
    }

    private void PlayRandom(AudioClip[] clips)
    {
        PlayOneShot(GetRandomClip(clips));
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        CacheAudioSource();
        audioSource.PlayOneShot(clip, volume);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
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
