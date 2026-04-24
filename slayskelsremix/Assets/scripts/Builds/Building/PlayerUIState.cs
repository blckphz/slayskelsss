public static class PlayerUIState
{
    public static bool IsInventoryOpen;
    public static bool IsChestOpen;
    public static bool IsCampfireOpen;

    public static bool IsAnyUIOpen =>
        IsInventoryOpen || IsChestOpen || IsCampfireOpen;
}