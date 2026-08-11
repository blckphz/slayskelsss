using UnityEngine;

public class TileTransparencyTrigger : MonoBehaviour
{
    [Header("Manager")]
    public TileTransparency manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PlayerEntered();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.PlayerExited();
        }
    }
}
