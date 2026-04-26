using UnityEngine;

public class turretsave : MonoBehaviour
{
    private buildableTurret turret;

    [Header("Save Identity")]
    public string turretID; // Assign a new GUID when placed via BuildManager

    void Awake() => turret = GetComponent<buildableTurret>();

    public TurretSaveData GetSaveData()
    {
        // DEBUG: Verify individual data capture
        Debug.Log($"<color=magenta>[Save System]</color> Saving Turret {turretID} with {turret.currentAmmo} ammo.");

        return new TurretSaveData
        {
            id = turretID,
            currentAmmo = turret.currentAmmo,
            position = transform.position
        };
    }

    public void LoadData(TurretSaveData data)
    {
        turretID = data.id;
        turret.currentAmmo = data.currentAmmo;

        bool canShoot = turret.currentAmmo > 0;
        GetComponent<TurretBehaviour>().SetFiringPermission(canShoot);

        Debug.Log($"<color=magenta>[Save System]</color> Loaded Turret {turretID} with {turret.currentAmmo} ammo.");
    }
}

