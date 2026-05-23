using System;

public static class BuildState
{
    private static bool isBuildMode;

    public static bool IsBuildMode => isBuildMode;

    public static event Action<bool> OnBuildModeChanged;

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
}