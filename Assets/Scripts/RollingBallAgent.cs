using UnityEngine;
using UnityEngine.Audio;

public sealed class RollingBallAgent : MonoBehaviour
{
    [SerializeField] private GameObject EffectParticle;
    [SerializeField] private float effectParticleLifetime = 1.5f;
    [SerializeField] private AudioSource sfxAudioSource;

    private RockinRackinPrototype controller;
    private bool isEnemy;
    private Vector3 previousVelocity;
    private float pushRingOutScoreExpiresAt;
    private bool wasAffectedByPushForceField;
    private bool pendingDestroy;

    public Rigidbody Body { get; private set; }
    private Vector3 toPlayer = Vector3.zero;
    public float Health { get; set; }
    public bool IsEnemy => isEnemy;

    public void Configure(RockinRackinPrototype owner, bool enemy, float health)
    {
        controller = owner;
        isEnemy = enemy;
        Health = health;
        Body = GetComponent<Rigidbody>();
        pushRingOutScoreExpiresAt = float.NegativeInfinity;
        wasAffectedByPushForceField = false;
        pendingDestroy = false;

        if (EffectParticle != null)
        {
            EffectParticle.SetActive(false);
        }
    }

    public void PlayEffectParticle()
    {
        if (EffectParticle == null)
        {
            return;
        }

        GameObject effect = Instantiate(EffectParticle, transform.position, transform.rotation);
        effect.transform.localScale = EffectParticle.transform.lossyScale;
        effect.SetActive(true);

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Play(true);
        }

        Destroy(effect, Mathf.Max(0.01f, effectParticleLifetime));
    }

    public float PlaySfx(AudioClip clip, float volume, AudioMixerGroup outputAudioMixerGroup)
    {
        if (clip == null)
        {
            return 0f;
        }

        CacheSfxAudioSource();
        sfxAudioSource.outputAudioMixerGroup = outputAudioMixerGroup;
        sfxAudioSource.PlayOneShot(clip, volume);
        return clip.length;
    }

    public void PrepareForDelayedDestroy()
    {
        pendingDestroy = true;

        if (Body != null)
        {
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.isKinematic = true;
            Body.detectCollisions = false;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }
    }

    public void MarkAffectedByPushForceField(float duration)
    {
        wasAffectedByPushForceField = true;
        pushRingOutScoreExpiresAt = Mathf.Max(pushRingOutScoreExpiresAt, Time.time + Mathf.Max(0f, duration));
    }

    public bool HasConfirmedPushRingOut(float minimumPlanarSpeed)
    {
        if (!wasAffectedByPushForceField || Time.time > pushRingOutScoreExpiresAt || Body == null)
        {
            return false;
        }

        Vector3 planarVelocity = Body.linearVelocity;
        planarVelocity.y = 0f;
        return planarVelocity.sqrMagnitude >= minimumPlanarSpeed * minimumPlanarSpeed;
    }

    private void FixedUpdate()
    {
        if (pendingDestroy)
        {
            return;
        }

        if (Body == null)
        {
            return;
        }

        previousVelocity = Body.linearVelocity;

        if (!isEnemy || controller == null || controller.PlayerBody == null)
        {
            return;
        }

        toPlayer = controller.PlayerBody.position - Body.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.2f)
        {
            Body.AddForce(toPlayer.normalized * controller.EnemyHomingForce, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (controller == null || pendingDestroy)
        {
            return;
        }

        if (isEnemy && controller.IsStageCollider(collision.collider))
        {
            controller.HandleEnemyLanding(this, previousVelocity);
        }

        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (pendingDestroy)
        {
            return;
        }

        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider other)
    {
        if (controller == null || isEnemy || pendingDestroy)
        {
            return;
        }

        RollingBallAgent otherBall = other.GetComponent<RollingBallAgent>();
        if (otherBall != null && otherBall.isEnemy)
        {
            controller.DamagePlayer(controller.ContactDamage);
        }
    }

    private void CacheSfxAudioSource()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }

        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.spatialBlend = 1f;
    }
}
