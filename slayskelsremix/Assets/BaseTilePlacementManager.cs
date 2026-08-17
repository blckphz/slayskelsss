using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseTilePlacementManager : MonoBehaviour
{
    public static BaseTilePlacementManager Instance;

    [System.Serializable]
    public class BaseTilePlacement
    {
        public buildSO item;
        public List<Vector3Int> cells = new List<Vector3Int>();
    }

    public List<BaseTilePlacement> placements =
        new List<BaseTilePlacement>();


    private void Awake()
    {
        Instance = this;
    }


    // =====================================================
    // REGISTER PLACEMENT
    // =====================================================

    public void Register(
        buildSO item,
        List<Vector3Int> cells)
    {
        if (item == null)
            return;

        if (cells == null ||
            cells.Count == 0)
            return;


        BaseTilePlacement placement =
            new BaseTilePlacement();


        placement.item =
            item;


        placement.cells =
            new List<Vector3Int>(cells);


        placements.Add(
            placement
        );
    }


    // =====================================================
    // FIND PLACEMENT AT CELL
    // =====================================================

    public BaseTilePlacement FindPlacement(
        Vector3Int cell)
    {
        for (int i = placements.Count - 1; i >= 0; i--)
        {
            BaseTilePlacement placement =
                placements[i];


            if (placement == null)
                continue;


            if (placement.cells.Contains(cell))
            {
                return placement;
            }
        }


        return null;
    }


    // =====================================================
    // REMOVE PLACEMENT
    // =====================================================

    public bool RemovePlacement(
        Tilemap tilemap,
        Vector3Int cell)
    {
        if (tilemap == null)
            return false;


        BaseTilePlacement placement =
            FindPlacement(cell);


        if (placement == null)
            return false;


        // =================================================
        // REMOVE ALL TILES
        // =================================================

        foreach (
            Vector3Int placementCell
            in placement.cells)
        {
            tilemap.SetTile(
                placementCell,
                null
            );
        }


        // =================================================
        // RETURN ITEM
        // =================================================

        if (
            placement.item != null &&
            InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(
                placement.item,
                1
            );
        }


        // =================================================
        // REMOVE RECORD
        // =================================================

        placements.Remove(
            placement
        );


        // =================================================
        // REFRESH
        // =================================================

        tilemap.RefreshAllTiles();


        return true;
    }
}