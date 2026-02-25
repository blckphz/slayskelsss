using UnityEngine;

[CreateAssetMenu(fileName = "turret", menuName = "Abilities/Turret")]
public class turretSO : defensiveAbilities
{
    public float lifetime = 10f;
    public float turretdmg;
    public float shootfreq;

    public override void Execute(Transform caster, Transform targetAnchor)
    {
        Debug.Log("[TurretSO] Execute called");

        if (caster == null)
        {
            Debug.LogError("[TurretSO] Caster is NULL");
            return;
        }

        if (targetAnchor == null)
        {
            Debug.LogError("[TurretSO] TargetAnchor is NULL");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("[TurretSO] Prefab is NOT assigned in the ScriptableObject!");
            return;
        }

        if (ObjectPooler.Instance == null)
        {
            Debug.LogError("[TurretSO] ObjectPooler.Instance is NULL");
            return;
        }

        Vector3 spawnPos = targetAnchor.position;

        Debug.Log($"[TurretSO] Attempting to spawn turret at {spawnPos}");

        GameObject turretObj = ObjectPooler.Instance.GetPooledObject(
            prefab,
            spawnPos,
            Quaternion.identity
        );

        if (turretObj == null)
        {
            Debug.LogError("[TurretSO] ObjectPooler returned NULL. Is the prefab registered in the pool?");
            return;
        }

        Debug.Log("[TurretSO] Turret object retrieved from pool successfully");

        var turret = turretObj.GetComponent<TurretBehaviour>();

        if (turret == null)
        {
            Debug.LogError("[TurretSO] TurretBehaviour component NOT found on prefab!");
            return;
        }

        Debug.Log("[TurretSO] Calling Setup on TurretBehaviour");

        turret.Setup(hp, lifetime, caster);
        turret.SetCombatStats(turretdmg, shootfreq);

        Debug.Log("[TurretSO] Turret setup complete");
    }
}
