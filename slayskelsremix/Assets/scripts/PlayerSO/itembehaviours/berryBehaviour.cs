using UnityEngine;

public class berryBehaviour : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Vector2 _direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float dmg, float spd, Vector2 dir)
    {
        _damage = dmg;
        _speed = spd;
        _direction = dir;

        // Set velocity
        rb.linearVelocity = _direction * _speed;

        // Rotate to face travel direction
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Look for anything that implements IDamageable (Enemies or Items)
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            // 1. Apply the damage
            damageable.TakeDamage(_damage);

            // 2. (Optional) Apply knockback if it's an enemy
            if (collision.TryGetComponent<enemyHealth>(out enemyHealth enemy))
            {
                // Calculate a small push force based on the berry's direction
                Vector2 knockbackForce = _direction * 2f;
                enemy.ApplyImpulse(knockbackForce);
            }

            // 3. Clean up the projectile
            Deactivate();
        }
    }

    private void Deactivate()
    {
        // Return to pool
        gameObject.SetActive(false);
    }
}