using UnityEngine;

public static class ActionLock
{
    public static bool IsLocked { get; private set; }
    private static int lockFrame = -1;

    public static void LockThisFrame()
    {
        IsLocked = true;
        lockFrame = Time.frameCount;
        Debug.Log($"[ActionLock] LOCK ACTIVATED on frame {lockFrame}");
    }

    // Call this from any Update() in your project (BuildManager / TiltManager etc.)
    public static void Tick()
    {
        if (IsLocked && Time.frameCount != lockFrame)
        {
            IsLocked = false;
            Debug.Log("[ActionLock] Lock released.");
        }
    }
}