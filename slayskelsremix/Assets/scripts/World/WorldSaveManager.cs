using UnityEngine;

public class WorldSaveManager : MonoBehaviour
{
    public ItemDatabase itemDatabase;

    public WorldSaveData currentSave = new WorldSaveData();

    public void SaveNpcInventories()
    {
        Debug.Log("========== SAVE NPC INVENTORIES ==========");

        currentSave.npcInventories.Clear();

        NpcInvBrain[] npcs = FindObjectsOfType<NpcInvBrain>();

        Debug.Log($"Found {npcs.Length} NPCs.");

        foreach (NpcInvBrain npc in npcs)
        {
            Debug.Log($"Saving NPC: {npc.name} (npcId={npc.npcId})");

            currentSave.npcInventories.Add(
                npc.GetSaveData()
            );
        }

        Debug.Log($"TOTAL NPC SAVES: {currentSave.npcInventories.Count}");
        Debug.Log("========== SAVE COMPLETE ==========");
    }

    public void LoadNpcInventories()
    {
        Debug.Log("========== LOAD NPC INVENTORIES ==========");

        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase is NULL!");
            return;
        }

        NpcInvBrain[] npcs = FindObjectsOfType<NpcInvBrain>();

        Debug.Log($"Found {npcs.Length} NPCs.");
        Debug.Log($"Save contains {currentSave.npcInventories.Count} NPC inventories.");

        foreach (NpcInvBrain npc in npcs)
        {
            Debug.Log($"Searching save for NPC: {npc.name} (npcId={npc.npcId})");

            var npcSave = currentSave.npcInventories.Find(
                x => x.npcId == npc.npcId
            );

            if (npcSave == null)
            {
                Debug.LogWarning($"NO SAVE FOUND for npcId={npc.npcId}");
                continue;
            }

            npc.LoadFromSave(npcSave, itemDatabase);
        }

        Debug.Log("========== LOAD COMPLETE ==========");
    }
}