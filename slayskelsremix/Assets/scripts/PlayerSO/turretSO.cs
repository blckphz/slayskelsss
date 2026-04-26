using UnityEngine;

[CreateAssetMenu(fileName = "turret", menuName = "Abilities/Turret")]
public class turretSO : defensiveAbilities
{
    [Header("Ability Settings")]
    public float lifetime = 10f;
    public float turretdmg;
    public float shootfreq;
    public int pierceCount; // New pierce stat
    public int maxturretcount = 3;

    // Updated return type to bool and added the return handshake
    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // 1. Initial Logic
        // We only care about the initial click. If we aren't holding/pressing, don't execute.
        if (!isHolding) return false;

        // 2. Safety Checks
        if (caster == null || targetAnchor == null || prefab == null || ObjectPooler.Instance == null)
        {
            Debug.LogError("[TurretSO] Missing references!");
            return false;
        }

        // 3. MAX COUNT LOGIC: Remove oldest if at limit
        if (TurretBehaviour.ActiveTurrets.Count >= maxturretcount)
        {
            TurretBehaviour oldestTurret = TurretBehaviour.ActiveTurrets[0];
            if (oldestTurret != null)
            {
                oldestTurret.Deactivate();
            }
        }

        // 4. SPAWN NEW TURRET
        Vector3 spawnPos = targetAnchor.position;
        GameObject turretObj = ObjectPooler.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);

        if (turretObj == null) return false;

        // 5. SETUP
        var turret = turretObj.GetComponent<TurretBehaviour>();
        if (turret != null)
        {
            // Set stats including the new pierceCount
            turret.Setup(hp, lifetime, caster);
            turret.SetCombatStats(turretdmg, shootfreq, pierceCount);
        }

        // Return true to trigger the cooldown in PlayerAttack immediately
        return true;
    }
}