using UnityEngine;

public class BuildRadiusController : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Settings")]
    public float buildRadius = 5f;

    private bool isActive = false;

    void Update()
    {
        if (PlayerInputHandler.Instance != null &&
            PlayerInputHandler.Instance.BuildModePressed())
        {
            Toggle();
        }
    }

    void Toggle()
    {
        isActive = !isActive;
    }

    // Used by BuildManager (LineRenderer circle)
    public float GetRadius()
    {
        return isActive ? buildRadius : 0f;
    }

    public bool IsActive()
    {
        return isActive;
    }

    public bool IsWithinRadius(Vector3 position)
    {
        if (player == null) return true;

        if (!isActive) return true; // optional: allow placement outside when off

        return Vector2.Distance(position, player.position) <= buildRadius;
    }
}