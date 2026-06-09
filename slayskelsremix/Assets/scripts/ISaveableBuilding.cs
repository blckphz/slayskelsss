public interface ISaveableBuilding
{
    int GetItemID();

    void GetSaveData(
        out int ammo,
        out float progress,
        out int durability
    );

    void LoadSaveData(
        int ammo,
        float progress,
        int durability
    );
}