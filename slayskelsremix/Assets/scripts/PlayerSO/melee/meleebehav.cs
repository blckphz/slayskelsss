using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class meleebehav : MonoBehaviour
{
    private float damage;
    private float bonusDamage;
    private List<IDamageable> hitEnemies = new List<IDamageable>();
    private Animator anim;
    private Vector3 prefabScale;
    private Coroutine deactivationRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        prefabScale = transform.localScale;
    }

    public void Setup(float dmg, float bDmg, int swingIndex)
    {
        damage = dmg;
        bonusDamage = bDmg;
        hitEnemies.Clear();

        if (deactivationRoutine != null) StopCoroutine(deactivationRoutine);

        bool isEven = (swingIndex % 2 == 0);
        transform.localScale = new Vector3(
            isEven ? -prefabScale.x : prefabScale.x,
            prefabScale.y,
            prefabScale.z
        );

        if (anim != null)
        {
            anim.SetInteger("SwingIndex", swingIndex);
            anim.SetTrigger("Attack");
            deactivationRoutine = StartCoroutine(DeactivateAfterAnimation());
        }
        else
        {
            deactivationRoutine = StartCoroutine(DeactivateAfterTime(0.3f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null && !hitEnemies.Contains(target))
        {
            float finalDamage = damage;

            // Apply bonus damage if the target is slowed/stunned
            if (target.IsSlowed)
            {
                finalDamage += bonusDamage;
                // Optional: Trigger a "Crit" or "Bonus" visual effect here
            }

            target.TakeDamage(finalDamage);
            hitEnemies.Add(target);
            CameraShaker.Shake(0.35f, 0.12f);
        }
    }

    private IEnumerator DeactivateAfterAnimation()
    {
        yield return new WaitForEndOfFrame();
        if (anim != null)
        {
            float duration = anim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(duration);
        }
        Deactivate();
    }

    private IEnumerator DeactivateAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        Deactivate();
    }

    void Deactivate()
    {
        transform.localScale = prefabScale;
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}