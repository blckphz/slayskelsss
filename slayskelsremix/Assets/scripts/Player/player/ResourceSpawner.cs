using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ResourceEntry
    {
        public string id;
        public GameObject prefab;
        public float respawnTime = 5f;
    }

    public List<ResourceEntry> resources;

    private Dictionary<string, ResourceEntry> lookup;

    private void Awake()
    {
        lookup = new Dictionary<string, ResourceEntry>();

        foreach (var r in resources)
        {
            lookup[r.id] = r;
        }
    }

    public void NotifyDeath(string id, Vector3 position)
    {
        if (!lookup.ContainsKey(id)) return;

        StartCoroutine(RespawnRoutine(lookup[id], position));
    }

    private IEnumerator RespawnRoutine(ResourceEntry entry, Vector3 position)
    {
        yield return new WaitForSeconds(entry.respawnTime);

        Instantiate(entry.prefab, position, Quaternion.identity);
    }
}