using UnityEngine;

public class StoneBehavior : MonoBehaviour
{
    private float _damage;
    private float _speed;
    private Vector2 _direction;
    private int _remainingPierces;
    private float _knockback;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float dmg, float spd, Vector2 dir, int pierce, float kb)
    {
        _damage = dmg;
        _speed = spd;
        _direction = dir;
        _remainingPierces = pierce;
        _knockback = kb;

        // Set initial velocity
        rb.linearVelocity = _direction * _speed;

        // Optional: Rotate stone to face direction or add spin
        transform.up = _direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy (adjust tag as needed)
        if (collision.GetComponent<enemyHealth>() != null)
        {
            ApplyDamage(collision.gameObject);

            if (_remainingPierces <= 0)
            {
                Deactivate();
            }
            else
            {
                _remainingPierces--;
            }
        }

        /*

        // Hit a wall
        if (collision.CompareTag("Environment"))
        {
            Deactivate();
        }

        */

    }

    private void ApplyDamage(GameObject enemy)
    {
        // Replace with your actual Damage/Health system call
        Debug.Log($"Stone hit {enemy.name} for {_damage} damage!");
    }

    private void Deactivate()
    {
        // Return to pool instead of Destroying
        gameObject.SetActive(false);
    }
}