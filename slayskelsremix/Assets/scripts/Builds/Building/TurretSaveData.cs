using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurretSaveData
{
    public string id;
    public int currentAmmo;
    public Vector3 position; // Added to know WHERE to spawn the turret on load
}

[System.Serializable]
public class BuildingSaveData
{
    public List<TurretSaveData> savedTurrets = new List<TurretSaveData>();
}