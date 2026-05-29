using System;
using UnityEngine;

[System.Serializable]
public class TreeSaveData
{
    public string treeID;
    public bool isCut;
    public float cutTime;

    // ==============================
    // DEBUG HELPERS
    // ==============================

    public void LogSave(string context = "")
    {
        Debug.Log(
            $"[TREE SAVE DATA] {context} | ID={treeID} | isCut={isCut} | cutTime={cutTime}");
    }

    public void LogLoad(string context = "")
    {
        Debug.Log(
            $"[TREE LOAD DATA] {context} | ID={treeID} | isCut={isCut} | cutTime={cutTime}");
    }

    public override string ToString()
    {
        return $"TreeSaveData(ID={treeID}, isCut={isCut}, cutTime={cutTime})";
    }
}