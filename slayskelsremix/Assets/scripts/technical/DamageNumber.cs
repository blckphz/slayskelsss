using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class DamageNumber : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float lifeTime = 1f;
    public float spreadRange = 0.5f;

    [Header("References")]
    private TMP_Text textMesh;
    private Color textColor;
    private Camera mainCam;
    private Rigidbody2D rb;

    void Awake()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        textMesh = GetComponentInChildren<TMP_Text>();

        if (textMesh != null)
        {
            textColor = textMesh.color;
        }
        else
        {
            Debug.LogError($"<color=red>[DamageNumber]</color> Missing TMP_Text on {gameObject.name}");
        }

        // Configure Rigidbody for smooth motion
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void Setup(float damageAmount)
    {
        // Find and parent to Canvas
        GameObject canvasGO = GameObject.Find("DmgNumbersCanvas");
        if (canvasGO != null)
        {
            transform.SetParent(canvasGO.transform, true);

            // Fix scale issues often caused by UI parenting
            if (transform.localScale.magnitude < 0.1f)
            {
                transform.localScale = Vector3.one;
            }
        }

        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        }

        // --- PHYSICS BOUNCE LOGIC ---
        // Create a random upward arc
        float randomX = Random.Range(-spreadRange, spreadRange);
        Vector2 launchDirection = new Vector2(randomX, 1f).normalized;

        // Apply velocity once. Physics engine takes it from here!
        rb.linearVelocity = launchDirection * moveSpeed;

    }

    void Update()
    {
        // Fade out logic remains in Update for smoothness
        if (textMesh != null)
        {
            textColor.a -= (1f / lifeTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }

    void LateUpdate()
    {
        // Billboarding moved to LateUpdate to prevent jitter 
        // if the camera is moving or following a target.
        if (mainCam != null)
        {
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
        }
    }

}