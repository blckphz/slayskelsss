public interface ISaveableBuilding
{
    int GetItemID();      // The ID used to find the prefab in the database
    void GetSaveData(out int ammo, out float progress); // Package data for saving
    void LoadSaveData(int ammo, float progress);        // Restore data on load
}