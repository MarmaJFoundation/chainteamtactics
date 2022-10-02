using System.Collections.Generic;
using UnityEngine;

public class NodeInfo
{
    public Vector2Int position;
    public TileType tileType;
    public bool blocked;
    public UnitController unit;
    //public readonly List<UnitController> bodies = new List<UnitController>();

    public Vector2Int parentPos;
    public Vector3 worldPos;
    public float offset;
    public int gCost;
    public int hCost;

    //private GameObject topObj;
    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public Vector3 WorldPosFloat
    {
        get
        {
            return worldPos + Vector3.up * offset;
        }
    }
}

