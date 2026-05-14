
public interface ISaveableBuilding
{
    int GetItemID();

    void GetSaveData(
        out int ammo,
        out float progress
    );

    void LoadSaveData(
        int ammo,
        float progress
    );
}