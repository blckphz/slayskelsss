using UnityEngine;
using UnityEngine.Tilemaps;

public class TileTransparency : MonoBehaviour
{
    [Header("Player Collision")]
    [Range(0f, 1f)]
    public float playerOpacity = 0.5f;

    [Header("Mouse Hover")]
    [Range(0f, 1f)]
    public float mouseOpacity = 0.1f;

    [Header("Tilemaps")]
    public Tilemap tilemapRoof;
    public Tilemap tilemapWall;

    // How many player hitboxes are currently inside
    private int playersInTriggers = 0;

    // Is the mouse currently hovering this object?
    private bool mouseHovering = false;


    // =========================================================
    // PLAYER COLLISION
    // =========================================================

    public void PlayerEntered()
    {
        playersInTriggers++;

        UpdateOpacity();
    }


    public void PlayerExited()
    {
        playersInTriggers--;

        if (playersInTriggers < 0)
            playersInTriggers = 0;

        UpdateOpacity();
    }


    // =========================================================
    // MOUSE HOVER
    // =========================================================

    public void MouseEntered()
    {
        mouseHovering = true;

        UpdateOpacity();
    }


    public void MouseExited()
    {
        mouseHovering = false;

        UpdateOpacity();
    }


    // =========================================================
    // OPACITY LOGIC
    // =========================================================

    private void UpdateOpacity()
    {
        // Mouse hover has priority over player collision
        if (mouseHovering)
        {
            SetOpacity(mouseOpacity);
        }
        // Player is inside
        else if (playersInTriggers > 0)
        {
            SetOpacity(playerOpacity);
        }
        // Nothing is happening
        else
        {
            SetOpacity(1f);
        }
    }


    // =========================================================
    // SET OPACITY
    // =========================================================

    private void SetOpacity(float alpha)
    {
        // Roof
        if (tilemapRoof != null)
        {
            Color color = tilemapRoof.color;
            color.a = alpha;
            tilemapRoof.color = color;
        }

        // Wall
        if (tilemapWall != null)
        {
            Color color = tilemapWall.color;
            color.a = alpha;
            tilemapWall.color = color;
        }
    }
}