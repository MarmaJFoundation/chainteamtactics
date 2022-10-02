using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    None = 0,
    Water = 1,
    Lava = 2,
    Earth = 3,
    Marble = 4,
    Grass = 5,
    Stone = 6,
    Wood = 7,
    BlockProp = 8,
    BlockWater = 9,
    BlockMarble = 10
}
[CreateAssetMenu(fileName = "New Tile", menuName = "Scriptable/Tile")]
public class ScriptableTile : ScriptableObject
{
    public TileType tileType;
    public bool blocks;
    public int damage;
    public int weight;
    public bool hasMask;
    public float offset;
}
