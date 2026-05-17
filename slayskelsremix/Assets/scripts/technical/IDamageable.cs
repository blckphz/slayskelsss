public interface IDamageable
{
    void TakeDamage(float damage);

    // Overload to support specific tool types (like Axe, Pickaxe, etc.)
    void TakeDamage(float damage, ToolType toolType);

    void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval);
    bool IsSlowed { get; }
}