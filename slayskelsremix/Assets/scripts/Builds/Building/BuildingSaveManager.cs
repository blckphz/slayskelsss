using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    [System.Serializable]
    public class BuildingData
    {
        public int itemID;
        public string uniqueID;
        public Vector3 position;

        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;

        public int currentAmmo;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
    }

    [Header("References")]
    public ItemDatabase database;

    private List<GameObject> placedBuildings = new List<GameObject>();
    private string savePath;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/buildings.json";

        LoadBuildings();
        AutoRegisterSceneBuildings();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null && !placedBuildings.Contains(obj))
            placedBuildings.Add(obj);
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null)
            placedBuildings.Remove(obj);
    }

    public void SaveAfterChange() => SaveNow();

    public void SaveNow()
    {
        SaveData data = new SaveData();

        SceneLoader.Instance?.SetLoadingText("Saving buildings...");

        foreach (GameObject obj in placedBuildings)
        {
            if (obj == null) continue;

            BuildIdentity id = obj.GetComponent<BuildIdentity>();
            if (id == null || id.item == null) continue;

            BuildingData b = new BuildingData
            {
                itemID = id.item.itemID,
                position = obj.transform.position
            };

            if (obj.TryGetComponent(out buildableTurret turret))
            {
                SceneLoader.Instance?.SetLoadingText("Saving turret...");

                b.currentAmmo = turret.currentAmmo;

                if (obj.TryGetComponent(out turretsave tSave))
                    b.uniqueID = tSave.turretID;
            }

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                SceneLoader.Instance?.SetLoadingText("Saving campfire...");

                b.fuelAmount = campfire.fuelAmount;
                b.isBurning = campfire.isBurning;
                b.fuelItemID = campfire.fuelItem != null
                    ? campfire.fuelItem.itemID
                    : -1;
            }

            data.buildings.Add(b);
        }

        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        SceneLoader.Instance?.SetLoadingText(
            "Buildings saved successfully."
        );
    }

    public void LoadBuildings()
    {
        SceneLoader.Instance?.SetLoadingText(
            "Loading buildings..."
        );

        if (!File.Exists(savePath))
        {
            SceneLoader.Instance?.SetLoadingText(
                "No building save found."
            );
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        foreach (var b in data.buildings)
        {
            SceneLoader.Instance?.SetLoadingText(
                $"Generating item {b.itemID}..."
            );

            ItemData item = database.GetItemByID(b.itemID);

            if (item == null || item is not buildSO buildItem)
                continue;

            GameObject obj = Instantiate(
                buildItem.placeablePrefab,
                b.position,
                Quaternion.identity
            );

            if (obj.TryGetComponent(out buildableTurret turret))
            {
                SceneLoader.Instance?.SetLoadingText(
                    $"Restoring turret ammo ({b.currentAmmo})..."
                );

                turret.currentAmmo = b.currentAmmo;

                if (obj.TryGetComponent(out turretsave tSave))
                    tSave.turretID = b.uniqueID;

                if (obj.TryGetComponent(out TurretBehaviour brain))
                    brain.SetFiringPermission(
                        turret.currentAmmo > 0
                    );
            }

            if (obj.TryGetComponent(out CampfireBehav campfire))
            {
                SceneLoader.Instance?.SetLoadingText(
                    $"Loading campfire fuel ({b.fuelAmount})..."
                );

                campfire.fuelAmount = b.fuelAmount;
                campfire.isBurning = b.isBurning;

                if (b.fuelItemID != -1)
                    campfire.fuelItem =
                        database.GetItemByID(b.fuelItemID);

                campfire.SendMessage(
                    "UpdateVisuals",
                    SendMessageOptions.DontRequireReceiver
                );
            }

            BuildIdentity id =
                obj.GetComponent<BuildIdentity>()
                ?? obj.AddComponent<BuildIdentity>();

            id.item = buildItem;

            RegisterBuilding(obj);
        }

        SceneLoader.Instance?.SetLoadingText(
            "Buildings loaded successfully."
        );
    }

    void AutoRegisterSceneBuildings()
    {
        foreach (var id in FindObjectsOfType<BuildIdentity>())
            RegisterBuilding(id.gameObject);
    }
}