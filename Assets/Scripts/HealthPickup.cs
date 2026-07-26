using UnityEngine;

public sealed class HealthPickup : MonoBehaviour
{
    private RockinRackinPrototype controller;

    public void Configure(RockinRackinPrototype owner)
    {
        controller = owner;
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
