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

    [Header("Inspector Testing (Drag objects here)")]
    public GameObject testTentObject;
    public GameObject testCampfireObject;

    private List<MapPointer> questPointers = new List<MapPointer>();
    private Dictionary<string, MapPointer> permanentPointers = new Dictionary<string, MapPointer>();
    private Camera mainCam;

    // We use these to track if the inspector reference changed
    private GameObject lastTent;
    private GameObject lastCampfire;

    void Awake()
    {
        if (Instance == null) Instance = this;
        mainCam = Camera.main;
    }

    void Update()
    {
        // TESTING LOGIC: 
        // This checks if you've dragged a new object into the inspector slots during runtime.
        if (testTentObject != lastTent)
        {
            SetLandmark("Tent", testTentObject, tentIcon);
            lastTent = testTentObject;
        }

        if (testCampfireObject != lastCampfire)
        {
            SetLandmark("Campfire", testCampfireObject, campfireIcon);
            lastCampfire = testCampfireObject;
        }
    }

    // --- LANDMARK SYSTEM ---
    public void SetLandmark(string landmarkID, GameObject target, Sprite icon)
    {
        // 1. Remove old pointer for this ID if it exists
        if (permanentPointers.ContainsKey(landmarkID))
        {
            if (permanentPointers[landmarkID] != null)
                Destroy(permanentPointers[landmarkID].gameObject);
            permanentPointers.Remove(landmarkID);
        }

        // 2. Create the new pointer
        if (target != null)
        {
            GameObject go = Instantiate(pointerPrefab, canvasParent);
            MapPointer pointer = go.GetComponent<MapPointer>();
            pointer.Initialize(target.transform, mainCam, icon);

            permanentPointers.Add(landmarkID, pointer);
            Debug.Log($"<color=lime>[MapPointer]</color> Landmark {landmarkID} set to {target.name}");
        }
    }

    // --- QUEST SYSTEM ---
    public void AddPointersToGroup(GameObject[] targets, Sprite icon = null)
    {
        foreach (GameObject t in targets)
        {
            if (t == null) continue;
            GameObject go = Instantiate(pointerPrefab, canvasParent);
            MapPointer pointer = go.GetComponent<MapPointer>();
            pointer.Initialize(t.transform, mainCam, icon);
            questPointers.Add(pointer);
        }
    }
}