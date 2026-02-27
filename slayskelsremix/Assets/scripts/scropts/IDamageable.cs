public interface IDamageable
{
    void TakeDamage(float damage);
    // Updated to include tick damage parameters
    void ApplySlow(float slowPercent, float duration, float tickDmg, float tickInterval);
}