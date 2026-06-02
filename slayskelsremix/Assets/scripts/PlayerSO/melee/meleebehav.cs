using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class meleebehav : MonoBehaviour
{
    private int damage;
    private int bonusDamage;

    private List<IDamageable> hitEnemies = new List<IDamageable>();
    private Animator anim;
    private Vector3 prefabScale;
    private Coroutine deactivationRoutine;

    private offensivemelee.SwingOwner owner;
    private ToolType associatedTool;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        prefabScale = transform.localScale;
    }

    public void Setup(
        int dmg,
        int bDmg,
        int swingIndex,
        offensivemelee.SwingOwner swingOwner,
        Transform parentTransform,
        Vector3 localOffset,
        ToolType toolType)
    {
        damage = dmg;
        bonusDamage = bDmg;
        owner = swingOwner;
        associatedTool = toolType;

        hitEnemies.Clear();

        if (parentTransform != null)
        {
            transform.SetParent(parentTransform);
            transform.localPosition = localOffset;
        }

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
            int finalDamage = damage;

            if (chainController.isUnlocked)
                finalDamage += chainController.staticBonusDmg;

            if (target.IsSlowed)
                finalDamage += bonusDamage;

            ItemData currentToolItem = null;

            if (owner == offensivemelee.SwingOwner.Player &&
                PlayerHotbarManager.Instance != null)
            {
                currentToolItem = PlayerHotbarManager.Instance.GetSelectedItem();
            }

            target.TakeDamage(finalDamage, associatedTool, currentToolItem);

            hitEnemies.Add(target);
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

        gameObject.SetActive(false);
    }

    private IEnumerator DeactivateAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}