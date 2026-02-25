using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifeTime = 1f;
    public float spreadRange = 0.5f; // How far left/right it can drift

    private TMP_Text textMesh;
    private Color textColor;
    private Vector3 moveDirection;

    void Awake()
    {
        textMesh = GetComponentInChildren<TMP_Text>();

        if (textMesh == null)
        {
            Debug.LogError($"No TMP component found on {gameObject.name} or its children!");
            return;
        }

        textColor = textMesh.color;

        // Create a random horizontal offset for the "burst" effect
        // This gives us a vector that points mostly up, but slightly left or right
        float randomX = Random.Range(-spreadRange, spreadRange);
        moveDirection = new Vector3(randomX, 1f, 0).normalized;
    }

    public void Setup(float damageAmount)
    {
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
        }
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move in the randomized direction
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (textMesh != null)
        {
            // Fade out over the lifetime
            textColor.a -= (1f / lifeTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}