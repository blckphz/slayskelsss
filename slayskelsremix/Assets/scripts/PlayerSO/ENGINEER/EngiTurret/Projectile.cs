using UnityEngine;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    private float damage;
    private int pierceCount;
    private HashSet<int> hitEnemies = new HashSet<int>(); // Tracks InstanceIDs of enemies hit

    [SerializeField] private float lifespan = 3f;

    public void SetDamage(float dmg, int pierce)
    {
        damage = dmg;
        pierceCount = pierce;
    }

    private void Start()
    {
        Destroy(gameObject, lifespan);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<enemyHealth>(out enemyHealth enemy))
        {
            int id = enemy.gameObject.GetInstanceID();

            // If we already hit this enemy with this specific projectile, ignore it
            if (hitEnemies.Contains(id)) return;

            // Apply damage and track the hit
            enemy.TakeDamage(damage);
            hitEnemies.Add(id);

            // Pierce Logic
            if (pierceCount <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                pierceCount--;
                // Optional: You could add a visual spark here to show a pierce happened
            }
        }
    }
}