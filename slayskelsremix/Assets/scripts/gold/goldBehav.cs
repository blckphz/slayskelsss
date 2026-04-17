using UnityEngine;

public class goldBehav : MonoBehaviour
{
    [Header("Settings")]
    public int goldValue = 1;
    public GameObject landParticlePrefab;

    [Header("Arc Physics")]
    public float minXForce = -2f;
    public float maxXForce = 2f;
    public float minYForce = 4f;
    public float maxYForce = 7f;

    [Header("Magnetism")]
    public float detectRadius = 5f;
    public float minMoveSpeed = 2f;    // Starting speed of the pull
    public float maxMoveSpeed = 20f;   // Top speed when close to player
    public float lerpSpeed = 5f;       // How fast the speed itself accelerates

    private bool isCollected = false;
    private bool hasLanded = false;
    private float currentSpeed;
    private Rigidbody2D rb;
    private Transform playerTransform;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        currentSpeed = minMoveSpeed;
    }

    void Start()
    {
        // Initial "Pop" out of the enemy
        float xForce = Random.Range(minXForce, maxXForce);
        float yForce = Random.Range(minYForce, maxYForce);
        rb.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);
        rb.AddTorque(Random.Range(-100f, 100f));
    }

    void Update()
    {
        if (isCollected || playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= detectRadius)
        {
            // Disable physics so we control the position directly
            rb.simulated = false;

            // 1. LERP THE SPEED: Gradually move from min to max speed
            currentSpeed = Mathf.Lerp(currentSpeed, maxMoveSpeed, lerpSpeed * Time.deltaTime);

            // 2. MOVE TO PLAYER: Use the lerped speed for consistent movement
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                currentSpeed * Time.deltaTime
            );

            // 3. AUTO-COLLECT: Safety check if it gets very close
            if (dist < 0.2f)
            {
                CollectGold();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            CollectGold();
        }
    }

    void CollectGold()
    {
        if (isCollected) return;
        isCollected = true;

        goldcontroler.gold += goldValue;

        // Add a little "pop" sound or particle call here if you have one
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !hasLanded)
        {
            hasLanded = true;
            if (landParticlePrefab != null)
                Instantiate(landParticlePrefab, transform.position, Quaternion.identity);

            rb.linearDamping = 5f;
            rb.angularDamping = 5f;
        }
    }
}