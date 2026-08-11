using System;

public static class BuildState
{
    private static bool isBuildMode;
    private static bool useGridPlacement = true;

    private static bool gridLocked;

    public static bool IsBuildMode => isBuildMode;
    public static bool UseGridPlacement => useGridPlacement;
    public static bool GridLocked => gridLocked;

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
        if (gridLocked)
            return;

        SetGridPlacement(!useGridPlacement);
    }


    public static void SetGridPlacement(bool value)
    {
        if (gridLocked)
            return;

        if (useGridPlacement == value)
            return;

        useGridPlacement = value;
        OnGridPlacementChanged?.Invoke(useGridPlacement);
    }


    public static void ForceGridPlacement(bool force)
    {
        gridLocked = force;

        if (force && !useGridPlacement)
        {
            useGridPlacement = true;
            OnGridPlacementChanged?.Invoke(true);
        }
    }


    // Used by BuildManager when changing build items
    public static void SetGridLocked(bool value)
    {
        gridLocked = value;

        if (value)
        {
            ForceGridPlacement(true);
        }
    }
}