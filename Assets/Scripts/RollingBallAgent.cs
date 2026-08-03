using UnityEngine;

public sealed class RollingBallAgent : MonoBehaviour
{
    private RockinRackinPrototype controller;
    private bool isEnemy;
    private Vector3 previousVelocity;
    private float pushRingOutScoreExpiresAt;
    private bool wasAffectedByPushForceField;

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
        if (controller == null)
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
        TryDamagePlayer(collision.collider);
    }

    private void TryDamagePlayer(Collider other)
    {
        if (controller == null || isEnemy)
        {
            return;
        }

        RollingBallAgent otherBall = other.GetComponent<RollingBallAgent>();
        if (otherBall != null && otherBall.isEnemy)
        {
            controller.DamagePlayer(controller.ContactDamage);
        }
    }
}
