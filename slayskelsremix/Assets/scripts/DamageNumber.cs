using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float lifeTime = 1f;
    public float spreadRange = 0.5f;

    private TMP_Text textMesh;
    private Color textColor;
    private Vector3 moveDirection;

    void Awake()
    {
        textMesh = GetComponentInChildren<TMP_Text>();
        if (textMesh != null)
        {
            textColor = textMesh.color;
        }

        // Random horizontal drift
        float randomX = Random.Range(-spreadRange, spreadRange);
        moveDirection = new Vector3(randomX, 1f, 0).normalized;
    }

    public void Setup(float damageAmount)
    {
        // --- AUTOMATIC FIND LOGIC ---
        GameObject canvasGO = GameObject.Find("DmgNumbersCanvas");

        if (canvasGO != null)
        {
            // Parent it to the canvas but KEEP the world position
            transform.SetParent(canvasGO.transform, worldPositionStays: true);
        }
        else
        {
            Debug.LogError("Could not find 'DmgNumbersCanvas' in the scene! Make sure the name matches exactly.");
        }

        if (textMesh != null)
        {
            textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move in world space
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Fade out logic
        if (textMesh != null)
        {
            textColor.a -= (1f / lifeTime) * Time.deltaTime;
            textMesh.color = textColor;
        }
    }
}