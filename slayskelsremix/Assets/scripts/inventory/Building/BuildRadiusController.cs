using UnityEngine;

public class BuildRadiusController : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public GameObject visualRoot;

    [Header("Settings")]
    public float buildRadius = 5f;

    private bool isActive = false;

    void Start()
    {
        if (visualRoot != null)
            visualRoot.SetActive(false);
    }

    void Update()
    {
        if (PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.BuildModePressed())
        {
            Toggle();
        }

        if (!isActive || player == null || visualRoot == null) return;

        UpdatePosition();
        UpdateScale();
    }

    void Toggle()
    {
        isActive = !isActive;

        if (visualRoot != null)
            visualRoot.SetActive(isActive);
    }

    void UpdatePosition()
    {
        visualRoot.transform.position = player.position;
    }

    void UpdateScale()
    {
        float diameter = buildRadius * 2f;
        visualRoot.transform.localScale = new Vector3(diameter, diameter, 1f);
    }

    public bool IsWithinRadius(Vector3 position)
    {
        if (player == null) return true;

        float dist = Vector2.Distance(position, player.position);
        return dist <= buildRadius;
    }
}