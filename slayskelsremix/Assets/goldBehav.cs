using UnityEngine;

public class goldBehav : MonoBehaviour
{
    [Header("Settings")]
    public int goldValue = 1;
    public GameObject landParticlePrefab; // Assign a "Dust/Sparkle" particle prefab

    [Header("Arc Physics")]
    public float minXForce = -2f;
    public float maxXForce = 2f;
    public float minYForce = 4f;
    public float maxYForce = 7f;

    private bool isCollected = false;
    private bool hasLanded = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Apply the "Pop" arc when the gold spawns
        float xForce = Random.Range(minXForce, maxXForce);
        float yForce = Random.Range(minYForce, maxYForce);

        rb.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);

        // Give it a random spin
        rb.AddTorque(Random.Range(-100f, 100f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only collect if it's the player and hasn't been collected yet
        if (collision.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            goldcontroler.gold += goldValue;

            Debug.Log("Gold collected! Total: " + goldcontroler.gold);

            // You could spawn a "pickup" particle here too!
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Detect hitting the floor (make sure your floor has the "Ground" tag)
        if (collision.gameObject.CompareTag("Ground") && !hasLanded)
        {
            hasLanded = true;

            if (landParticlePrefab != null)
            {
                Instantiate(landParticlePrefab, transform.position, Quaternion.identity);
            }

            // Slow down the physics after landing so it doesn't roll forever
            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
        }
    }
}