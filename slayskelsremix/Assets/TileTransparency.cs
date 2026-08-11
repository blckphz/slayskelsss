using UnityEngine;
using UnityEngine.Tilemaps;

public class TileTransparency : MonoBehaviour
{
    [Header("Settings")]
    [Range(0f, 1f)]
    public float fadedOpacity = 0.5f;

    [Range(0f, 1f)]
    public float fullOpacity = 1f;

    [Header("Tilemaps")]
    public Tilemap tilemapRoof;
    public Tilemap tilemapWall;

    // How many hitboxes the player is currently inside
    private int playersInTriggers = 0;

    public void PlayerEntered()
    {
        playersInTriggers++;

        // Fade both tilemaps
        SetOpacity(fadedOpacity);
    }

    public void PlayerExited()
    {
        playersInTriggers--;

        // Only restore opacity when player
        // has left BOTH hitboxes
        if (playersInTriggers <= 0)
        {
            playersInTriggers = 0;
            SetOpacity(fullOpacity);
        }
    }

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
