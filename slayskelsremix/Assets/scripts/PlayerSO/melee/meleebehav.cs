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

    private bool chargeConsumedThisSwing = false;

    private offensivemelee.SwingOwner owner;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        prefabScale = transform.localScale;
    }

    // 🔥 NOW RECEIVES OWNER
    public void Setup(float dmg, float bDmg, int swingIndex, offensivemelee.SwingOwner swingOwner)
    {
        damage = dmg;
        bonusDamage = bDmg;
        owner = swingOwner;

        hitEnemies.Clear();
        chargeConsumedThisSwing = false;


        // ⚔️ CHARGE CONSUMPTION ONLY FOR PLAYER
        if (owner == offensivemelee.SwingOwner.Player)
        {
            if (chainController.isUnlocked)
            {
                if (chainController.hitCounter > 0)
                {
                    chainController.hitCounter--;


                    chargeConsumedThisSwing = true;
                }
                else
                {
                    Debug.Log("[Melee Swing] PLAYER has no charges.");
                }
            }
        }
        else
        {
            Debug.Log("[Melee Swing] NPC swing detected → NO charge consumed.");
        }

        if (deactivationRoutine != null)
            StopCoroutine(deactivationRoutine);

        bool isEven = (swingIndex % 2 == 0);

        transform.localScale = new Vector3(
            isEven ? -prefabScale.x : prefabScale.x,
            prefabScale.y,
            prefabScale.z
        );

        if (anim != null)
        {
            anim.SetInteger("SwingIndex", isEven ? 2 : 1);
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

            // ⚔️ BONUS DAMAGE LOGIC
            if (chainController.isUnlocked)
            {
                finalDamage += chainController.staticBonusDmg;
            }

            if (target.IsSlowed)
                finalDamage += bonusDamage;

            target.TakeDamage(finalDamage);
            hitEnemies.Add(target);

            Debug.Log(
                $"[Hit] OWNER: {owner} | Target: {collision.name} | Damage: {finalDamage}"
            );

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