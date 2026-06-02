using UnityEngine;

public class berryBehaviour : MonoBehaviour
{
    [Header("Visual Settings")]
    public float smearStrengthPerHit = 0.3f; // ~3-4 hits for full intensity

    private int _damage;
    private float _speed;
    private Vector2 _direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(int dmg, float spd, Vector2 dir)
    {
        _damage = dmg;
        _speed = spd;
        _direction = dir;

        rb.linearVelocity = _direction * _speed;

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable damageable = collision.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(_damage);

            if (collision.TryGetComponent<enemyHealth>(out enemyHealth enemy))
            {
                // Apply physics
                Vector2 knockbackForce = _direction * 2f;
                enemy.ApplyImpulse(knockbackForce);

                // Add to Berry Smear ONLY
                enemy.AddSmear(smearStrengthPerHit);
            }

            Deactivate();
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}