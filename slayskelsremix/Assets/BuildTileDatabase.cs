using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(
    fileName = "BuildTileDatabase",
    menuName = "Building/Build Tile Database"
)]
public class BuildTileDatabase : ScriptableObject
{
    [System.Serializable]
    public class TileEntry
    {
        public int tileID;

        public TileBase tile;
    }

    [Header("Tiles")]
    public List<TileEntry> tiles =
        new List<TileEntry>();

    // =====================================================
    // GET ID FROM TILE
    // =====================================================

    public int GetTileID(TileBase tile)
    {
        if (tile == null)
            return -1;

        for (int i = 0; i < tiles.Count; i++)
        {
            TileEntry entry = tiles[i];

            if (entry == null)
                continue;

            if (entry.tile == tile)
                return entry.tileID;
        }

        return -1;
    }

    // =====================================================
    // GET TILE FROM ID
    // =====================================================

    public TileBase GetTile(int tileID)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            TileEntry entry = tiles[i];

            if (entry == null)
                continue;

            if (entry.tileID == tileID)
                return entry.tile;
        }

        return null;
    }

    // =====================================================
    // CHECK IF TILE EXISTS
    // =====================================================

    public bool HasTile(int tileID)
    {
        return GetTile(tileID) != null;
    }
}