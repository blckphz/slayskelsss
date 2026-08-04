using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(IAstarAI))]
[RequireComponent(typeof(Animator))]
public class enemyWalkAI : MonoBehaviour
{
    private IAstarAI ai;
    private Animator anim;
    private Transform tr;

    [Header("Wandering")]
    public float wanderRadius = 3f;
    public float wanderInterval = 3f;

    private float nextWanderTime;
    private Vector3 spawnPosition;

    private Vector2 lastDirection = Vector2.down;

    private static readonly int XHash = Animator.StringToHash("x");
    private static readonly int YHash = Animator.StringToHash("y");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Start()
    {
        tr = transform;
        ai = GetComponent<IAstarAI>();
        anim = GetComponent<Animator>();

        spawnPosition = tr.position;
    }

    public void MoveTo(Vector3 position)
    {
        if (ai == null) return;

        ai.isStopped = false;
        ai.destination = position;
    }

    public void StopMovement()
    {
        if (ai != null)
            ai.isStopped = true;
    }

    public void Wander()
    {
        if (ai == null) return;

        if (Time.time < nextWanderTime) return;

        nextWanderTime = Time.time + wanderInterval;

        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        ai.isStopped = false;
        ai.destination = spawnPosition + new Vector3(randomPoint.x, randomPoint.y, 0f);
    }

    public void UpdateMovementAnimation()
    {
        if (ai == null || anim == null) return;

        Vector3 velocity = ai.velocity;
        float speed = velocity.magnitude;

        anim.SetFloat(SpeedHash, speed);

        // Update face direction only while actively moving to prevent zeroing out
        if (speed > 0.05f)
        {
            lastDirection = new Vector2(velocity.x, velocity.y).normalized;
        }

        anim.SetFloat(XHash, lastDirection.x);
        anim.SetFloat(YHash, lastDirection.y);
    }
}