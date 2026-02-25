using UnityEngine;
using System.Collections;

public class offensivemelee : offensiveability
{
    [Header("Placement Settings")]
    public float spawnOffset = 1.5f;
    public float rotationOffset = 0f;

    [Header("Combo Settings")]
    public int swingsPerAttack = 2;
    public float delayBetweenSwings = 0.15f;

    public override void Execute(Transform caster, Transform targetAnchor)
    {
        if (caster == null) return;
        caster.GetComponent<MonoBehaviour>().StartCoroutine(MeleeSequence(caster, targetAnchor));
    }

    private IEnumerator MeleeSequence(Transform caster, Transform targetAnchor)
    {
        for (int i = 0; i < swingsPerAttack; i++)
        {
            if (caster == null) yield break;

            if (launchsound != null)
                audiomanager.Instance?.PlaySound(launchsound);

            PerformSingleSwing(caster, targetAnchor, i + 1);

            if (i < swingsPerAttack - 1)
                yield return new WaitForSeconds(delayBetweenSwings);
        }
    }

    private void PerformSingleSwing(Transform caster, Transform targetAnchor, int index)
    {
        if (prefab == null) { Debug.LogError("Melee Prefab is missing!"); return; }

        // 1. Calculate direction toward target
        Vector2 rawDir = (targetAnchor.position - caster.position).normalized;
        Vector2 snappedDir = Mathf.Abs(rawDir.x) > Mathf.Abs(rawDir.y)
            ? new Vector2(Mathf.Sign(rawDir.x), 0)
            : new Vector2(0, Mathf.Sign(rawDir.y));

        // 2. Calculate the intended WORLD position
        Vector3 desiredWorldPos = caster.position + (Vector3)(snappedDir * spawnOffset);

        // DEBUG: Draw a Green line in Scene view toward the intended spawn point
        Debug.DrawRay(caster.position, (Vector3)snappedDir * spawnOffset, Color.green, 1f);

        // 3. Calculate rotation
        float angle = (Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg) + rotationOffset;

        // 4. Get from pool
        GameObject woosh = ObjectPooler.Instance.GetPooledObject(prefab, desiredWorldPos, Quaternion.Euler(0, 0, angle));

        // 5. Parent it
        woosh.transform.SetParent(caster);

        // 6. FIX: Convert world position to local space to counter character flipping
        woosh.transform.localPosition = caster.InverseTransformPoint(desiredWorldPos);


        var behav = woosh.GetComponent<meleebehav>();
        if (behav != null)
            behav.Setup(damage, index);

        CameraShaker.Shake(0.4f, 0.12f);
    }
}