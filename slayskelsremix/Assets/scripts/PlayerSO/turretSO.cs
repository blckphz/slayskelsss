using UnityEngine;

[CreateAssetMenu(fileName = "turret", menuName = "Abilities/Turret")]
public class turretSO : defensiveAbilities, IStatProvider // 🔥 Added Interface
{
    [Header("Ability Settings")]
    public float lifetime = 10f;
    public float turretdmg;
    public float shootfreq;
    public int pierceCount;
    public int maxturretcount = 3;

    // --- IStatProvider Implementation ---
    public string GetStatsFormat()
    {
        // This returns the specific turret data to the PerkManager
        return $"Turret DMG: <color=#FF5555>{turretdmg}</color>\n" +
               $"Turret Speed: {shootfreq}s\n" +
               $"Pierce: {pierceCount}\n" +
               $"Max Active: {maxturretcount}\n" +
               $"Lifetime: {lifetime}s";
    }

    public override bool Execute(Transform caster, Transform targetAnchor, bool isHolding)
    {
        if (!isHolding) return false;

        if (caster == null || targetAnchor == null || prefab == null || ObjectPooler.Instance == null)
        {
            Debug.LogError("[TurretSO] Missing references!");
            return false;
        }

        if (TurretBehaviour.ActiveTurrets.Count >= maxturretcount)
        {
            TurretBehaviour oldestTurret = TurretBehaviour.ActiveTurrets[0];
            if (oldestTurret != null) oldestTurret.Deactivate();
        }

        Vector3 spawnPos = targetAnchor.position;
        GameObject turretObj = ObjectPooler.Instance.GetPooledObject(prefab, spawnPos, Quaternion.identity);

        if (turretObj == null) return false;

        var turret = turretObj.GetComponent<TurretBehaviour>();
        if (turret != null)
        {
            turret.Setup(hp, lifetime, caster);
            turret.SetCombatStats(turretdmg, shootfreq, pierceCount);
        }

        return true;
    }
}