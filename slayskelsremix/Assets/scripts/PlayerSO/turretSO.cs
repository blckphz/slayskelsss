using UnityEngine;

[CreateAssetMenu(fileName = "turret", menuName = "Abilities/Turret")]
public class turretSO : defensiveAbilities
{
    public float lifetime = 10f;
    public float turretdmg;
    public float shootfreq;
    public int maxturretcount = 3;

    public override void Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        // We only care about the initial click for turrets
        if (!isHolding) return;

        // 1. Safety Checks
        if (caster == null || targetAnchor == null || prefab == null || ObjectPooler.Instance == null)
        {
            Debug.LogError("[TurretSO] Missing references!");
            return;
        }

        // 2. MAX COUNT LOGIC: Remove oldest if at limit
        if (TurretBehaviour.ActiveTurrets.Count >= maxturretcount)
        {
            TurretBehaviour oldestTurret = TurretBehaviour.ActiveTurrets[0];
            if (oldestTurret != null)
            {
                oldestTurret.Deactivate();
            }
        }

        // 3. SPAWN NEW TURRET
        Vector3 spawnPos = targetAnchor.position;
        GameObject turretObj = ObjectPooler.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);

        if (turretObj == null) return;

        // 4. SETUP
        var turret = turretObj.GetComponent<TurretBehaviour>();
        if (turret != null)
        {
            turret.Setup(hp, lifetime, caster);
            turret.SetCombatStats(turretdmg, shootfreq);
        }
    }
}