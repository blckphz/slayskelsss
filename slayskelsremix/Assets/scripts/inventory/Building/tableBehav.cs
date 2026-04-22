using UnityEngine;

public class tableBehav : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string _id;
    public string ID => _id;

    // Call this when placing a brand new table
    public void GenerateUniqueID()
    {
        if (string.IsNullOrEmpty(_id))
        {
            _id = System.Guid.NewGuid().ToString();
            Debug.Log($"[TABLE] Generated New ID: {_id}");
        }
    }

    // Call this when loading from a save file
    public void LoadPersistentID(string existingID)
    {
        _id = existingID;
    }
}