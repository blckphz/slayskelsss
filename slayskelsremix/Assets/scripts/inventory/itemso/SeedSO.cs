using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Seed Item")]
public class SeedSO : ItemData
{
    [Header("Plant Prefab")]
    public GameObject plantPrefab;

    [Header("Growth Sprites")]
    public Sprite[] growthSprites;

    public override bool Use(Transform caster, Transform targetAnchor, GameObject target)
    {
        // If target is a dig hole, plant seed
        if (target != null && target.TryGetComponent(out EarthHoleDigBehav hole))
        {
            return hole.PlantSeed(plantPrefab);
        }

        InteractionUI.Instance?.Show("Cannot plant here");
        return false;
    }
}