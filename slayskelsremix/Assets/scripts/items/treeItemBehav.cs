using UnityEngine;
using System.Collections;

public class treeItemBehav : ItemHealth, IDamageable
{
    [Header("Health")]
    private float maxHealth;

    [Header("Loot")]
    public GameObject woodPrefab;


    [Header("Tree Parts")]
    public GameObject topPrefab;
    public GameObject bottomPrefab;

    [Header("Player Reference")]
    public Transform player;

    [Header("Fall Settings")]
    public float fallSpeed = 2f;
    public float fallDistance = 0.5f;
    public float fallDelay = 0.05f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("VFX")]
    public GameObject leafParticlePrefab;

    private bool isDead = false;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        maxHealth = health;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // ---------------- DAMAGE ----------------

 
    // ---------------- DEATH ----------------

    private void Die()
    {
        StartCoroutine(FallSequence());
    }

    private IEnumerator FallSequence()
    {
        // Spawn stump
        if (bottomPrefab != null)
            Instantiate(bottomPrefab, transform.position, Quaternion.identity);

        // Spawn leaves
        if (leafParticlePrefab != null)
            Instantiate(leafParticlePrefab, transform.position, Quaternion.identity);

        // Spawn top
        GameObject top = null;
        if (topPrefab != null)
            top = Instantiate(topPrefab, transform.position, Quaternion.identity);

        // Hide original
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        yield return new WaitForSeconds(fallDelay);

        Vector2 dir = GetFallDirection();

        if (top != null)
        {
            yield return StartCoroutine(RotateTop(top.transform, dir));
        }

        // Tree is fully gone now
        Destroy(gameObject);
    }

    // ---------------- FALL DIRECTION ----------------

    private Vector2 GetFallDirection()
    {
        if (player == null)
            return Vector2.right;

        Vector2 dir = (Vector2)(transform.position - player.position);
        return dir.normalized;
    }

    // ---------------- FALL ANIMATION ----------------

    private IEnumerator RotateTop(Transform top, Vector2 dir)
    {
        float progress = 0f;
        Quaternion startRot = top.rotation;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion endRot = Quaternion.Euler(0, 0, angle - 90f);

        Vector3 startPos = top.position;
        Vector3 endPos = startPos + (Vector3)(dir * fallDistance);

        SpriteRenderer topSR = top.GetComponent<SpriteRenderer>();

        while (progress < 1f)
        {
            progress += Time.deltaTime * fallSpeed;
            float t = Mathf.Clamp01(progress);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            top.rotation = Quaternion.Lerp(startRot, endRot, easedT);
            top.position = Vector3.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        top.rotation = endRot;
        top.position = endPos;

        if (topSR != null)
        {
            yield return StartCoroutine(FadeOut(topSR));
        }
    }

    // ---------------- FADE ----------------

    private IEnumerator FadeOut(SpriteRenderer sr)
    {
        Color originalColor = sr.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            sr.color = newColor;
            yield return null;
        }

        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }


}