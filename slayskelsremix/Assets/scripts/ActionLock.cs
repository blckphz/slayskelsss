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

    public static void LateUpdate()
    {
        if (IsLocked && Time.frameCount != lockFrame)
        {
            IsLocked = false;
            Debug.Log("[ActionLock] Lock released.");
        }
    }
}