using UnityEngine;
using System.Collections.Generic;
using System;

public class meleebehav : MonoBehaviour
{
    private int damage;
    private int bonusDamage;

    public bool DidHitSomething { get; private set; }

    private List<IDamageable> hitEnemies = new List<IDamageable>();

    private Animator anim;
    private Vector3 prefabScale;

    private offensivemelee.SwingOwner owner;
    private ToolType associatedTool;

    private bool isActive;

    public Action<bool> OnSwingFinished;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        prefabScale = transform.localScale;
    }

    private void OnDisable()
    {
        DidHitSomething = false;
        hitEnemies.Clear();
        OnSwingFinished = null;
        isActive = false;
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

        // ✅ REAL FIX: this now always comes from weapon data
        associatedTool = toolType;

        DidHitSomething = false;
        hitEnemies.Clear();
        isActive = true;

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
            anim.ResetTrigger("Attack");
            anim.SetInteger("SwingIndex", isEven ? 2 : 1);
            anim.SetTrigger("Attack");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive) return;

        IDamageable target = collision.GetComponent<IDamageable>();

        if (target == null)
            return;

        if (hitEnemies.Contains(target))
            return;

        DidHitSomething = true;

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

        // ✅ THIS NOW WORKS (NO GetToolType BUG)
        target.TakeDamage(finalDamage, associatedTool, currentToolItem);

        hitEnemies.Add(target);
    }

    public void AnimationFinished()
    {
        FinishSwing();
    }

    private void FinishSwing()
    {
        if (!isActive) return;

        isActive = false;

        OnSwingFinished?.Invoke(DidHitSomething);

        gameObject.SetActive(false);
    }
}