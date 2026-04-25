using UnityEngine;

public class NpcAttack : MonoBehaviour
{
    [Header("Ability Configuration")]
    public offensivemelee currentAbility;
    public Transform attackAnchor;

    private Transform currentTarget;

    [Header("Cooldown")]
    public float globalAttackCooldown = 0.2f;
    private float nextAttackTime;

    private void Awake()
    {
        if (currentAbility != null)
            currentAbility = Instantiate(currentAbility);
    }

    private void Update()
    {
        if (currentTarget == null || currentAbility == null)
            return;

        TryAttack(currentTarget);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Resource"))
            currentTarget = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == currentTarget)
        {
            currentTarget = null;
            StopAttacking();
        }
    }

    public void TryAttack(Transform target)
    {
        if (Time.time < nextAttackTime)
            return;

        if (target == null)
            return;

        bool finished = currentAbility.Execute(transform, target, true);

        nextAttackTime = Time.time + globalAttackCooldown;

        if (finished)
            nextAttackTime = Time.time + currentAbility.fireRate;
    }

    public void StopAttacking()
    {
        currentAbility?.Execute(transform, null, false);
    }
}