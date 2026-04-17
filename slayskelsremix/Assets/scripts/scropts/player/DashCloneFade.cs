using UnityEngine;

public class DashCloneFade : MonoBehaviour
{
    public float lifetime = 0.4f;
    public bool shrinkOverTime = true;

    private SpriteRenderer sr;
    private float timer;
    private Color originalColor;
    private Vector3 originalScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifetime;

        // Smooth ease-out fade (quadratic)
        float eased = 1f - (t * t);

        // Apply alpha
        Color c = originalColor;
        c.a = originalColor.a * eased;
        sr.color = c;

        // Optional smooth shrink
        if (shrinkOverTime)
        {
            transform.localScale = originalScale * eased;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}