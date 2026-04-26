using System.Collections.Generic;
using UnityEngine;

public class MapPointerManager : MonoBehaviour
{
    public static MapPointerManager Instance;

    [Header("Setup")]
    [SerializeField] private GameObject pointerPrefab;
    [SerializeField] private Transform canvasParent;

    [Header("Landmark Icons")]
    public Sprite tentIcon;
    public Sprite campfireIcon;

    private Dictionary<string, MapPointer> permanentPointers = new Dictionary<string, MapPointer>();
    private Camera mainCam;

    void Awake()
    {
        if (Instance == null) Instance = this;
        mainCam = Camera.main;
    }

    // REMOVED: The Update() loop polling. 
    // Instead, call SetLandmark only when the target actually changes in your game logic.

    public void SetLandmark(string landmarkID, GameObject target, Sprite icon)
    {
        // 1. Clean up existing pointer for this ID
        if (permanentPointers.TryGetValue(landmarkID, out MapPointer existing))
        {
            if (existing != null) Destroy(existing.gameObject);
            permanentPointers.Remove(landmarkID);
        }

        // 2. If target is null, we just wanted to remove it. If not, create new.
        if (target != null)
        {
            GameObject go = Instantiate(pointerPrefab, canvasParent);
            MapPointer pointer = go.GetComponent<MapPointer>();
            pointer.Initialize(target.transform, mainCam, icon);
            permanentPointers.Add(landmarkID, pointer);
        }
    }

    // Optimized Group Adder (e.g. for Quest objectives)
    public void AddPointersToGroup(IEnumerable<GameObject> targets, Sprite icon = null)
    {
        foreach (GameObject t in targets)
        {
            if (t == null) continue;
            GameObject go = Instantiate(pointerPrefab, canvasParent);
            go.GetComponent<MapPointer>().Initialize(t.transform, mainCam, icon);
        }
    }

    // Add this to MapPointerManager.cs
    public bool IsTrackingObject(string landmarkID, Transform targetTransform)
    {
        // Try to get the pointer from the dictionary
        if (permanentPointers.TryGetValue(landmarkID, out MapPointer pointer))
        {
            // Check if the pointer exists and is looking at the correct transform
            return pointer != null && pointer.GetTarget() == targetTransform;
        }
        return false;
    }

}