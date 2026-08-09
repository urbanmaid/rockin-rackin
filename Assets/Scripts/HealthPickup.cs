using UnityEngine;

public sealed class HealthPickup : MonoBehaviour
{
    [SerializeField] private GameObject EffectParticle;
    [SerializeField] private float effectParticleLifetime = 1.5f;

    private RockinRackinPrototype controller;

    public void Configure(RockinRackinPrototype owner)
    {
        controller = owner;

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

    private void Update()
    {
        transform.Rotate(0f, 150f * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (controller == null)
        {
            return;
        }

        RollingBallAgent ball = other.GetComponent<RollingBallAgent>();
        if (ball == null || ball.IsEnemy)
        {
            return;
        }

        controller.CollectPickup(this);
    }

    private void OnDestroy()
    {
        if (controller != null)
        {
            controller.NotifyPickupDestroyed(this);
        }
    }
}
