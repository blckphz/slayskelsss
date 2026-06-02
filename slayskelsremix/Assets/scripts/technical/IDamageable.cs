public interface IDamageable
{
    void TakeDamage(int damage);

    void TakeDamage(int damage, ToolType toolType);

    void TakeDamage(int damage, ToolType toolType, ItemData toolItem);

    void ApplySlow(float slowPercent, float duration, int tickDmg, float tickInterval);

    bool IsSlowed { get; }
}