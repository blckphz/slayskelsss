using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // A dictionary to keep track of different pools for different prefabs
    private Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools.Add(prefab, new List<GameObject>());
        }

        // Look for an inactive object to reuse
        foreach (GameObject obj in pools[prefab])
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
                return obj;
            }
        }

        // If none found, create a new one and add to pool
        GameObject newObj = Instantiate(prefab, position, rotation);
        pools[prefab].Add(newObj);
        return newObj;
    }
}