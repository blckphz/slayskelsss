using UnityEngine;

public class leafBehav : ItemHealth
{
    public bool collected;

    public void OnSpawnFromWorld(bool hasSaveData)
    {
        EnsureID(hasSaveData);
    }

    public leafData GetSaveData()
    {
        return new leafData
        {
            leafID = UniqOverworldItemID,
            position = transform.position,
            collected = collected
        };
    }

    public void LoadData(leafData data)
    {
        if (data == null)
        {
            EnsureID(false);
            return;
        }

        UniqOverworldItemID = data.leafID;
        transform.position = data.position;
        collected = data.collected;

        if (collected)
            gameObject.SetActive(false);
    }
}