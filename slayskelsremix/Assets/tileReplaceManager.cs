using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;


public class tileReplaceManager : MonoBehaviour
{
    public static tileReplaceManager Instance;


    [Header("Tilemap To Remove From")]
    public Tilemap tilemap;


    private Dictionary<Vector3Int, TileBase> removedTiles =
        new Dictionary<Vector3Int, TileBase>();



    private void Awake()
    {
        Instance = this;
    }



    // ===============================
    // REMOVE TILE
    // ===============================

    public void ReplaceTile(Vector3 worldPosition, buildSO item)
    {
        if (!item.baseTile)
            return;


        Vector3Int cell =
            tilemap.WorldToCell(worldPosition);



        TileBase existingTile =
            tilemap.GetTile(cell);



        if (existingTile == null)
            return;



        // Save original tile
        if (!removedTiles.ContainsKey(cell))
        {
            removedTiles.Add(
                cell,
                existingTile
            );
        }



        // Remove tile
        tilemap.SetTile(
            cell,
            null
        );


        tilemap.RefreshTile(cell);
    }





    // ===============================
    // RESTORE TILE
    // ===============================

    public void RestoreTile(Vector3Int cell)
    {
        if (!removedTiles.TryGetValue(
            cell,
            out TileBase tile))
        {
            return;
        }



        tilemap.SetTile(
            cell,
            tile
        );


        tilemap.RefreshTile(cell);



        removedTiles.Remove(cell);
    }
}