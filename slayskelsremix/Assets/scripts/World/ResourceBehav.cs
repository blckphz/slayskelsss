using UnityEngine;

public class ResourceBehav : ItemHealth
{
    [Header("Save")]
    public string prefabName;

    protected override void Awake()
    {
        base.Awake();

        // auto assign prefab name if empty
        if (string.IsNullOrEmpty(prefabName))
            prefabName = gameObject.name.Replace("(Clone)", "");
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
        if (data == null) return;

        UniqOverworldItemID = data.id;
        health = data.health;
        transform.position = data.position;
        prefabName = data.prefabName;
    }

    // =========================
    // DEATH
    // =========================

    protected override void Die()
    {
        if (BuildingSaveManager.Instance != null)
            BuildingSaveManager.Instance.SaveAfterChange();

        base.Die();
    }
}