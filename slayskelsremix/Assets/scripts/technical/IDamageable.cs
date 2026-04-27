public interface IDamageable
{
    void TakeDamage(float damage);
    void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval);
    bool IsSlowed { get; }
}