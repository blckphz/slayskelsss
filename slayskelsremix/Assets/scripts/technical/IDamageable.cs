public interface IDamageable
{
    void TakeDamage(float damage);

    // Overload to support specific tool types (like Axe, Pickaxe, etc.)
    void TakeDamage(float damage, ToolType toolType);

    // NEW OVERLOAD: Explicitly includes the tool item asset to track dynamic parameters like durability percentages
    void TakeDamage(float damage, ToolType toolType, ItemData toolItem);

    void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval);
    bool IsSlowed { get; }
}