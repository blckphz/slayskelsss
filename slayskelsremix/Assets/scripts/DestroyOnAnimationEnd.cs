using UnityEngine;

public class DestroyOnAnimationEnd : MonoBehaviour
{
    // This method will be called by the Animation Event
    public void OnAnimationComplete()
    {
        Destroy(gameObject);
    }
}