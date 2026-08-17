using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileSaver : MonoBehaviour
{
    public static TileSaver Instance;


    // =====================================================
    // TILEMAPS
    // =====================================================

    [Header("Tilemaps To Save")]
    public Tilemap[] tilemaps;


    // =====================================================
    // SAVE KEY
    // =====================================================

    private const string SAVE_KEY =
        "TileSaver_Data";


    // =====================================================
    // DATA
    // =====================================================

    [Serializable]
    private class TileSaveData
    {
        public List<TilemapSaveData> tilemaps =
            new List<TilemapSaveData>();
    }


    [Serializable]
    private class TilemapSaveData
    {
        public int mapIndex;

        public List<TileData> tiles =
            new List<TileData>();
    }


    [Serializable]
    private class TileData
    {
        public int x;
        public int y;
        public int z;

        public string tileName;
    }


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        if (tilemaps == null)
        {
            return;
        }


        for (
            int i = 0;
            i < tilemaps.Length;
            i++)
        {
            if (tilemaps[i] == null)
            {
                Debug.LogError(
                    "[TileSaver] Tilemap " +
                    i +
                    " is NULL!"
                );

                continue;
            }
        }
    }


    // =====================================================
    // GET TILEMAP
    // =====================================================

    public Tilemap GetTilemap(
        int index)
    {
        if (
            tilemaps == null ||
            index < 0 ||
            index >= tilemaps.Length)
        {
            return null;
        }


        return tilemaps[index];
    }


    // =====================================================
    // SAVE ALL TILEMAPS
    // =====================================================

    public void SaveTiles()
    {
        if (tilemaps == null)
        {
            Debug.LogWarning(
                "[TileSaver] No tilemaps assigned."
            );

            return;
        }


        TileSaveData saveData =
            new TileSaveData();


        // =================================================
        // LOOP TILEMAPS
        // =================================================

        for (
            int mapIndex = 0;
            mapIndex < tilemaps.Length;
            mapIndex++)
        {
            Tilemap tilemap =
                tilemaps[mapIndex];


            if (tilemap == null)
                continue;


            TilemapSaveData mapData =
                new TilemapSaveData();


            mapData.mapIndex =
                mapIndex;


            // =============================================
            // GET BOUNDS
            // =============================================

            BoundsInt bounds =
                tilemap.cellBounds;


            // =============================================
            // LOOP CELLS
            // =============================================

            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase tile =
                    tilemap.GetTile(cell);


                if (tile == null)
                    continue;


                TileData data =
                    new TileData();


                data.x =
                    cell.x;

                data.y =
                    cell.y;

                data.z =
                    cell.z;


                data.tileName =
                    tile.name;


                mapData.tiles.Add(
                    data
                );
            }


            saveData.tilemaps.Add(
                mapData
            );
        }


        // =================================================
        // SERIALIZE
        // =================================================

        string json =
            JsonUtility.ToJson(
                saveData
            );


        PlayerPrefs.SetString(
            SAVE_KEY,
            json
        );


        PlayerPrefs.Save();


        Debug.Log(
            "[TileSaver] Saved tilemaps."
        );
    }


    // =====================================================
    // LOAD ALL TILEMAPS
    // =====================================================

    public void LoadTiles()
    {
        if (
            !PlayerPrefs.HasKey(
                SAVE_KEY
            ))
        {
            Debug.Log(
                "[TileSaver] No tile save found."
            );

            return;
        }


        if (tilemaps == null)
        {
            Debug.LogError(
                "[TileSaver] tilemaps == NULL"
            );

            return;
        }


        string json =
            PlayerPrefs.GetString(
                SAVE_KEY
            );


        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning(
                "[TileSaver] Save data is empty."
            );

            return;
        }


        TileSaveData saveData;


        try
        {
            saveData =
                JsonUtility.FromJson<TileSaveData>(
                    json
                );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[TileSaver] Failed to load tile data:\n" +
                e
            );

            return;
        }


        if (saveData == null)
        {
            Debug.LogError(
                "[TileSaver] Save data is NULL."
            );

            return;
        }


        // =================================================
        // CLEAR CURRENT TILEMAPS
        // =================================================

        ClearTilemaps();


        // =================================================
        // RESTORE
        // =================================================

        foreach (
            TilemapSaveData mapData
            in saveData.tilemaps)
        {
            if (
                mapData == null ||
                mapData.tiles == null)
            {
                continue;
            }


            if (
                mapData.mapIndex < 0 ||
                mapData.mapIndex >= tilemaps.Length)
            {
                Debug.LogWarning(
                    "[TileSaver] Invalid map index: " +
                    mapData.mapIndex
                );

                continue;
            }


            Tilemap tilemap =
                tilemaps[
                    mapData.mapIndex
                ];


            if (tilemap == null)
                continue;


            foreach (
                TileData data
                in mapData.tiles)
            {
                TileBase tile =
                    FindTile(
                        data.tileName
                    );


                if (tile == null)
                {
                    Debug.LogWarning(
                        "[TileSaver] Could not find tile: " +
                        data.tileName
                    );

                    continue;
                }


                Vector3Int cell =
                    new Vector3Int(
                        data.x,
                        data.y,
                        data.z
                    );


                tilemap.SetTile(
                    cell,
                    tile
                );
            }


            tilemap.RefreshAllTiles();
        }


        Debug.Log(
            "[TileSaver] Loaded tilemaps."
        );
    }


    // =====================================================
    // FIND TILE
    // =====================================================

    private TileBase FindTile(
        string tileName)
    {
        if (string.IsNullOrEmpty(tileName))
            return null;


        // =================================================
        // SEARCH ALL RESOURCES
        // =================================================

        TileBase[] tiles =
            Resources.LoadAll<TileBase>("");


        foreach (
            TileBase tile
            in tiles)
        {
            if (
                tile != null &&
                tile.name == tileName)
            {
                return tile;
            }
        }


        return null;
    }


    // =====================================================
    // CLEAR TILEMAPS
    // =====================================================

    public void ClearTilemaps()
    {
        if (tilemaps == null)
            return;


        foreach (
            Tilemap tilemap
            in tilemaps)
        {
            if (tilemap == null)
                continue;


            tilemap.ClearAllTiles();
        }


        Debug.Log(
            "[TileSaver] Cleared tilemaps."
        );
    }


    // =====================================================
    // SAVE AFTER CHANGE
    // =====================================================

    public void SaveAfterChange()
    {
        SaveTiles();
    }


    // =====================================================
    // DELETE SAVE
    // =====================================================

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(
            SAVE_KEY
        );


        PlayerPrefs.Save();


        Debug.Log(
            "[TileSaver] Tile save deleted."
        );
    }


    // =====================================================
    // DEBUG TILEMAP CONTENT
    // =====================================================

    public void DebugTilemaps()
    {
        Debug.Log(
            "[TileSaver] ===== TILEMAP DEBUG ====="
        );


        if (tilemaps == null)
        {
            Debug.LogError(
                "[TileSaver] tilemaps == NULL"
            );

            return;
        }


        for (
            int mapIndex = 0;
            mapIndex < tilemaps.Length;
            mapIndex++)
        {
            Tilemap tilemap =
                tilemaps[mapIndex];


            if (tilemap == null)
            {
                Debug.LogError(
                    "[TileSaver] Tilemap " +
                    mapIndex +
                    " is NULL"
                );

                continue;
            }


            BoundsInt bounds =
                tilemap.cellBounds;


            int count = 0;


            foreach (
                Vector3Int cell
                in bounds.allPositionsWithin)
            {
                TileBase tile =
                    tilemap.GetTile(cell);


                if (tile == null)
                    continue;


                count++;


                Debug.Log(
                    "[TileSaver] MAP " +
                    mapIndex +
                    " | " +
                    tilemap.name +
                    " | Cell=" +
                    cell +
                    " | Tile=" +
                    tile.name
                );
            }


            Debug.Log(
                "[TileSaver] MAP " +
                mapIndex +
                " TOTAL TILES = " +
                count
            );
        }


        Debug.Log(
            "[TileSaver] ========================="
        );
    }
}