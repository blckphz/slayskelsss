using UnityEngine;

public class Highlightable : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;

    public Color highlightColor = Color.yellow;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;
    }

    public void SetHighlighted(bool state)
    {
        if (sr == null) return;

        sr.color = state ? highlightColor : originalColor;
    }
}