using System;

public static class BuildState
{
    private static bool isBuildMode;
    private static bool useGridPlacement = true;

    public static bool IsBuildMode => isBuildMode;
    public static bool UseGridPlacement => useGridPlacement;

    public static event Action<bool> OnBuildModeChanged;
    public static event Action<bool> OnGridPlacementChanged;

    public static void Toggle()
    {
        Set(!isBuildMode);
    }

    public static void Set(bool value)
    {
        if (isBuildMode == value)
            return;

        isBuildMode = value;
        OnBuildModeChanged?.Invoke(isBuildMode);
    }

    public static void ToggleGrid()
    {
        SetGridPlacement(!useGridPlacement);
    }

    public static void SetGridPlacement(bool value)
    {
        if (useGridPlacement == value)
            return;

        useGridPlacement = value;
        OnGridPlacementChanged?.Invoke(useGridPlacement);
    }
}