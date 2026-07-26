using UnityEngine;

public sealed class RollingBallAgent : MonoBehaviour
{
    private RockinRackinPrototype controller;
    private bool isEnemy;
    private Vector3 previousVelocity;

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

        if (!isEnemy || toPlayer.sqrMagnitude > 0.2f)
        {
            Body.AddForce(toPlayer.normalized * controller.EnemyHomingForce, ForceMode.Acceleration);
        }
    }

    private void FixedUpdate()
    {
        if (isEnemy || toPlayer.sqrMagnitude > 0.2f)
        {
            Body.AddForce(toPlayer.normalized * controller.EnemyHomingForce, ForceMode.Acceleration);
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
