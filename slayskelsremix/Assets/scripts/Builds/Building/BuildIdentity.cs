using UnityEngine;

public class BuildIdentity : MonoBehaviour
{
    public bool GetsDestroyedByPlayer;
    public buildSO item;
    public CraftingSO recipeUsed;

    [Header("Runtime")]
    public Vector3Int cell; // IMPORTANT
}