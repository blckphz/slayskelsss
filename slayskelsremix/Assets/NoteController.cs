using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NoteController : MonoBehaviour
{
    private HitZone hitZone;
    private Rigidbody2D rb;

    public float speed = 5f;

    public void Init(HitZone zone)
    {
        hitZone = zone;
        hitZone.RegisterNote(this);

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        rb.linearVelocity = Vector2.up * speed;

        Debug.Log("🎵 Note spawned");
    }

    public void OnHit()
    {
        if (hitZone != null)
            hitZone.UnregisterNote(this);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (hitZone != null)
            hitZone.UnregisterNote(this);
    }
}