using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;

    [SerializeField] private float lifespan = 3f; // Time before auto-destroy

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    private void Start()
    {
        // Automatically destroy after lifespan seconds
        Destroy(gameObject, lifespan);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Only damage objects that have an enemyHealth component
        if (collider.TryGetComponent<enemyHealth>(out enemyHealth enemy))
        {
            enemy.TakeDamage(damage);
            Debug.Log($"[Projectile] Hit {enemy.name} for {damage} damage");

            Destroy(gameObject);
        }
    }
}
