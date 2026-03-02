using UnityEngine;

[CreateAssetMenu(fileName = "turret", menuName = "Abilities/Turret")]
public class turretSO : defensiveAbilities
{
    public float lifetime = 10f;
    public float turretdmg;
    public float shootfreq;
    public int maxturretcount = 3; // Set your limit here

    public override void Execute(Transform caster, Transform targetAnchor)
    {
        // 1. Safety Checks
        if (caster == null || targetAnchor == null || prefab == null || ObjectPooler.Instance == null)
        {
            Debug.LogError("[TurretSO] Missing references! Check Caster, Anchor, Prefab, or Pooler.");
            return;
        }

        // 2. MAX COUNT LOGIC: Remove oldest if at limit
        // We check the static list in TurretBehaviour
        if (TurretBehaviour.ActiveTurrets.Count >= maxturretcount)
        {
            Debug.Log($"[TurretSO] Max turrets ({maxturretcount}) reached. Removing oldest.");

            // The first turret in the list is always the oldest one
            TurretBehaviour oldestTurret = TurretBehaviour.ActiveTurrets[0];

            if (oldestTurret != null)
            {
                oldestTurret.Deactivate();
            }
        }

        // 3. SPAWN NEW TURRET
        Vector3 spawnPos = targetAnchor.position;
        GameObject turretObj = ObjectPooler.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);

        if (turretObj == null)
        {
            Debug.LogError("[TurretSO] ObjectPooler failed to return an object.");
            return;
        }

        // 4. SETUP
        var turret = turretObj.GetComponent<TurretBehaviour>();
        if (turret != null)
        {
            turret.Setup(hp, lifetime, caster);
            turret.SetCombatStats(turretdmg, shootfreq);
            Debug.Log("[TurretSO] New turret spawned and oldest removed.");
        }
    }
}