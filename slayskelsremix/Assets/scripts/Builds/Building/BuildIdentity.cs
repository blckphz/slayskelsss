using UnityEngine;

public class BuildIdentity : MonoBehaviour
{

    public bool GetsDestroyedByPlayer;
    public buildSO item;
    public CraftingSO recipeUsed; // Added this to track what it cost to build
}