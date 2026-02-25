using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class meleebehav : MonoBehaviour
{
    private float damage;
    private List<IDamageable> hitEnemies = new List<IDamageable>();
    private Animator anim;
    private Vector3 prefabScale;

    private Coroutine deactivationRoutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        // Save the original scale from the prefab/start
        prefabScale = transform.localScale;
    }

    public void Setup(float dmg, int swingIndex)
    {
        damage = dmg;
        hitEnemies.Clear();

        // Stop any previous deactivation routines if re-enabled quickly
        if (deactivationRoutine != null) StopCoroutine(deactivationRoutine);

        // Flip every second swing for visual variety
        // Note: Because we use worldPositionStays in the other script, 
        // this localScale change only affects the visual "flip" of the sprite.
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

    private IEnumerator DeactivateAfterAnimation()
    {
        // Wait for animator to transition to the new state
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null && !hitEnemies.Contains(target))
        {
            target.TakeDamage(damage);
            hitEnemies.Add(target);
            CameraShaker.Shake(0.35f, 0.12f);
        }
    }

    void Deactivate()
    {
        // Reset scale and parent before returning to pool
        transform.localScale = prefabScale;
        transform.SetParent(null);
        gameObject.SetActive(false);
    }
}