public interface ISaveableBuilding
{
    int GetItemID();

    void GetSaveData(
        out int ammo,
        out float progress,
        out int durability // 👈 Added parameter to track structural wear
    );

    void LoadSaveData(
        int ammo,
        float progress,
        int durability // 👈 Added parameter to restore structural wear
    );
}