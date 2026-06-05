using UnityEngine;

public class ResourceBehav : ItemHealth
{
    [Header("Save")]
    public string prefabName;

    protected override void Awake()
    {
        base.Awake();
    }

    // =========================
    // SAVE
    // =========================

    public ResourceSaveData GetSaveData()
    {
        return new ResourceSaveData
        {
            id = UniqOverworldItemID,
            destroyed = health <= 0,
            health = health,
            position = transform.position,
            prefabName = prefabName
        };
    }

    // =========================
    // LOAD
    // =========================

    public void LoadData(ResourceSaveData data)
    {
        if (data == null)
        {
            // fresh spawn case
            EnsureID(false);
            return;
        }

        UniqOverworldItemID = data.id;

        health = data.health;
        transform.position = data.position;
        prefabName = data.prefabName;

        // if destroyed in save, optionally disable
        if (data.destroyed)
        {
            gameObject.SetActive(false);
        }
    }

    // =========================
    // DEATH
    // =========================

    protected override void Die(ToolType tool)
    {
        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        base.Die(tool);
    }
}