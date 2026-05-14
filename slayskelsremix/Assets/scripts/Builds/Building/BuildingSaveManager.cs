using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

public class BuildingSaveManager : MonoBehaviour
{
    public static BuildingSaveManager Instance;

    [System.Serializable]
    public class BuildingData
    {
        public int itemID;
        public string uniqueID;
        public Vector3 position;
        public int currentAmmo;
        public float plantProgress;
        public float fuelAmount;
        public bool isBurning;
        public int fuelItemID;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<BuildingData> buildings = new List<BuildingData>();

        // NPC INVENTORIES
        public List<NpcInventorySaveData> npcInventories =
            new List<NpcInventorySaveData>();
    }

    public ItemDatabase database;

    private List<GameObject> placedBuildings =
        new List<GameObject>();

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

        savePath =
            Application.persistentDataPath +
            "/buildings.json";

        Debug.Log(
            $"[SaveSystem] Save path: {savePath}"
        );

        LoadBuildings();
    }

    public void RegisterBuilding(GameObject obj)
    {
        if (obj != null &&
            !placedBuildings.Contains(obj))
        {
            placedBuildings.Add(obj);
        }
    }

    public void UnregisterBuilding(GameObject obj)
    {
        if (obj != null &&
            placedBuildings.Contains(obj))
        {
            placedBuildings.Remove(obj);
        }
    }

    public void SaveNow()
    {
        SaveData data = new SaveData();

        Debug.Log(
            "<color=yellow>[SaveSystem] Starting Save Process...</color>"
        );

        // CLEAN BUILDING LIST
        placedBuildings.RemoveAll(
            item => item == null
        );

        // =========================
        // SAVE BUILDINGS
        // =========================
        foreach (GameObject obj in placedBuildings)
        {
            BuildingData b =
                new BuildingData
                {
                    position =
                        obj.transform.position
                };

            if (obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                b.itemID =
                    saveable.GetItemID();

                saveable.GetSaveData(
                    out b.currentAmmo,
                    out b.plantProgress
                );
            }
            else if (
                obj.TryGetComponent(
                    out BuildIdentity id
                ) &&
                id.item != null
            )
            {
                b.itemID =
                    id.item.itemID;
            }
            else
            {
                Debug.LogWarning(
                    $"[Save] Skipping {obj.name}: No save data found!"
                );
                continue;
            }

            // Campfire extra data
            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                b.fuelAmount =
                    campfire.fuelAmount;

                b.isBurning =
                    campfire.isBurning;

                b.fuelItemID =
                    campfire.fuelItem != null
                        ? campfire.fuelItem.itemID
                        : -1;
            }

            data.buildings.Add(b);
        }

        Debug.Log(
            $"[Save] Saved {data.buildings.Count} buildings."
        );

        // =========================
        // SAVE NPC INVENTORIES
        // =========================
        NpcInvBrain[] npcs =
            FindObjectsOfType<NpcInvBrain>();

        Debug.Log(
            $"[NPC Save] Found {npcs.Length} NPCs."
        );

        foreach (NpcInvBrain npc in npcs)
        {
            Debug.Log(
                $"[NPC Save] Saving {npc.name} | npcId={npc.npcId}"
            );

            data.npcInventories.Add(
                npc.GetSaveData()
            );
        }

        Debug.Log(
            $"[NPC Save] Saved {data.npcInventories.Count} NPC inventories."
        );

        // WRITE JSON
        string json =
            JsonUtility.ToJson(
                data,
                true
            );

        File.WriteAllText(
            savePath,
            json
        );

        Debug.Log(
            "<color=green>[SaveSystem] Save complete.</color>"
        );
    }

    public void LoadBuildings()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning(
                "[Load] No save file found."
            );
            return;
        }

        if (database == null)
        {
            Debug.LogError(
                "[Load] ItemDatabase is NOT assigned!"
            );
            return;
        }

        string json =
            File.ReadAllText(
                savePath
            );

        SaveData data =
            JsonUtility.FromJson<SaveData>(
                json
            );

        Debug.Log(
            $"[Load] Loading {data.buildings.Count} buildings."
        );

        // =========================
        // LOAD BUILDINGS
        // =========================
        foreach (var b in data.buildings)
        {
            ItemData item =
                database.GetItemByID(
                    b.itemID
                );

            if (item == null)
            {
                Debug.LogError(
                    $"[Load] Item ID {b.itemID} not found."
                );
                continue;
            }

            GameObject prefab = null;

            if (item is buildSO bSO)
                prefab =
                    bSO.placeablePrefab;
            else if (
                item is SeedSO sSO
            )
                prefab =
                    sSO.plantPrefab;

            if (prefab == null)
            {
                Debug.LogError(
                    $"[Load] No prefab for {item.name}"
                );
                continue;
            }

            GameObject obj =
                Instantiate(
                    prefab,
                    b.position,
                    Quaternion.identity
                );

            obj.name =
                prefab.name;

            // Restore interface data
            if (obj.TryGetComponent(
                out ISaveableBuilding saveable))
            {
                saveable.LoadSaveData(
                    b.currentAmmo,
                    b.plantProgress
                );
            }

            // Restore campfire
            if (obj.TryGetComponent(
                out CampfireBehav campfire))
            {
                campfire.fuelAmount =
                    b.fuelAmount;

                campfire.isBurning =
                    b.isBurning;

                if (
                    b.fuelItemID != -1
                )
                {
                    campfire.fuelItem =
                        database.GetItemByID(
                            b.fuelItemID
                        );
                }

                campfire.SendMessage(
                    "UpdateVisuals",
                    SendMessageOptions
                        .DontRequireReceiver
                );
            }

            RegisterBuilding(
                obj
            );
        }

        // =========================
        // LOAD NPC INVENTORIES
        // =========================
        NpcInvBrain[] npcs =
            FindObjectsOfType<NpcInvBrain>();

        Debug.Log(
            $"[NPC Load] Found {npcs.Length} NPCs."
        );

        foreach (NpcInvBrain npc in npcs)
        {
            var npcSave =
                data.npcInventories.Find(
                    x =>
                        x.npcId ==
                        npc.npcId
                );

            if (npcSave == null)
            {
                Debug.LogWarning(
                    $"[NPC Load] No save found for {npc.name} ({npc.npcId})"
                );
                continue;
            }

            Debug.Log(
                $"[NPC Load] Loading {npc.name} ({npc.npcId})"
            );

            npc.LoadFromSave(
                npcSave,
                database
            );
        }

        Debug.Log(
            "<color=green>[Load] Finished loading.</color>"
        );
    }

    public void SaveAfterChange()
    {
        SaveNow();
    }
}
