using System.Collections.Generic;
using UnityEngine;

public class PassivePerkManager : MonoBehaviour
{
    public static PassivePerkManager Instance;

    private readonly List<PassivePerkSO> activePassives = new();

    private void Awake()
    {
        Instance = this;
        Debug.Log("[PassivePerkManager] Initialized singleton instance.");
    }

    public void AddPassive(PassivePerkSO perk)
    {
        if (perk == null)
        {
            Debug.LogWarning("[PassivePerkManager] AddPassive called with NULL perk.");
            return;
        }

        if (activePassives.Contains(perk))
        {
            Debug.Log($"[PassivePerkManager] Perk already active: {perk.name}");
            return;
        }

        activePassives.Add(perk);

        Debug.Log($"[PassivePerkManager] Adding passive perk: {perk.name}");

        try
        {
            perk.ApplyPassive();
            Debug.Log($"[PassivePerkManager] Successfully applied passive: {perk.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PassivePerkManager] Error applying passive {perk.name}: {e}");
        }

        Debug.Log($"[PassivePerkManager] Total active passives: {activePassives.Count}");
    }

    public void RemovePassive(PassivePerkSO perk)
    {
        if (perk == null)
        {
            Debug.LogWarning("[PassivePerkManager] RemovePassive called with NULL perk.");
            return;
        }

        if (!activePassives.Contains(perk))
        {
            Debug.LogWarning($"[PassivePerkManager] Tried to remove non-active perk: {perk.name}");
            return;
        }

        activePassives.Remove(perk);

        Debug.Log($"[PassivePerkManager] Removing passive perk: {perk.name}");

        try
        {
            perk.RemovePassive();
            Debug.Log($"[PassivePerkManager] Successfully removed passive: {perk.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PassivePerkManager] Error removing passive {perk.name}: {e}");
        }

        Debug.Log($"[PassivePerkManager] Total active passives: {activePassives.Count}");
    }
}