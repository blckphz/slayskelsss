using UnityEngine;

public class NoteController : MonoBehaviour
{
    private HitZone hitZone;
    private Rigidbody2D rb;
    public float speed = 5f;
    private bool hasBeenProcessed = false; // 🔥 Prevent double-calls

    public void Init(HitZone zone)
    {
        hitZone = zone;
        hitZone.RegisterNote(this);
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.right * speed;
        }
    }

    public void OnHit()
    {
        if (hasBeenProcessed) return;
        hasBeenProcessed = true;

        if (hitZone != null)
            hitZone.UnregisterNote(this);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the note hits a 'Miss' boundary at the top
        if (other.CompareTag("Finish"))
        {
            Debug.Log("<color=red>[Note]</color> Note missed boundary!");
            OnHit();
        }
    }

    private void OnDestroy()
    {
        // Safety: If the note is destroyed for any other reason, 
        // make sure it's not still taking up space in the HitZone list.
        if (!hasBeenProcessed && hitZone != null)
        {
            hitZone.UnregisterNote(this);
        }
    }
}