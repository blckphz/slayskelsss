using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Build Item/Turret")]

public class turretbuildso : buildSO
{
    [Header("Permanent Turret Stats")]
    [Tooltip("Check this if the building is a turret to initialize combat stats.")]
    public float turretHealth = 100f;
    public float turretDamage = 10f;
    public float fireRate = 0.5f;
    public int pierceCount = 0; // New pierce stat

}
