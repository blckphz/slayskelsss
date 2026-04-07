using System;

public static class NPCGlobalEvents
{
    public static event Action<int> OnTargetDestroyed;

    public static void NotifyDestroyed(int instanceID)
    {
        // Tell the manager to blacklist this ID so no one re-targets it this frame
        if (NPCList.Instance != null)
            NPCList.Instance.MarkAsDestroyed(instanceID);

        OnTargetDestroyed?.Invoke(instanceID);
    }
}