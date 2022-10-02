using Anonym.Isometric;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum MapType
{
    Bridge = 0,
    Temple = 1,
    Volcano = 2,
    Tavern = 3
}
public class MapEditor : MonoBehaviour
{
    public GameObject[] mapObjs;
    public void GenerateXMLs()
    {
        List<MapData> mapDatas = new List<MapData>();
        for (int i = 0; i < mapObjs.Length; i++)
        {
            MapData mapData = new MapData
            {
                mapType = (MapType)i,
                tileInfos = new List<TileInfo>()
            };
            List<TileInfo> blockProps = new List<TileInfo>();
            foreach (IsoTile cube in mapObjs[i].GetComponentsInChildren<IsoTile>())
            {
                int.TryParse(cube.name.Substring(cube.name.IndexOf("MainSheet_") + 10), out int intTileType);
                string joinPos = cube.name.Substring(cube.name.IndexOf("(") + 1, cube.name.IndexOf(")") - cube.name.IndexOf("(") - 1);
                string[] positions = joinPos.Split(',');
                int.TryParse(positions[0], out int posX);
                int.TryParse(positions[1], out int posY);
                TileType tileType = ParseTileType(intTileType);
                if (tileType == TileType.None)
                {
                    continue;
                }
                if (tileType == TileType.BlockProp)
                {
                    blockProps.Add(new TileInfo() { tileType = TileType.BlockProp, position = new Vector2Int(posX, posY) });
                }
                else
                {
                    bool blocked = tileType == TileType.BlockMarble || tileType == TileType.BlockWater;
                    mapData.tileInfos.Add(new TileInfo() { tileType = tileType, blocked = blocked, position = new Vector2Int(posX, posY) });
                }
            }
            for (int k = 0; k < blockProps.Count; k++)
            {
                for (int l = 0; l < mapData.tileInfos.Count; l++)
                {
                    if (mapData.tileInfos[l].position == blockProps[k].position)
                    {
                        mapData.tileInfos[l].blocked = true;
                        break;
                    }
                }
            }
            mapDatas.Add(mapData);
        }
        string rawMapData = XMLGenerator.SerializeObject(mapDatas, typeof(List<MapData>));
        XMLGenerator.CreateXML(rawMapData, "Maps.xml");
    }
    private TileType ParseTileType(int tileType)
    {
        switch (tileType)
        {
            case 0:
            case 16:
            case 17:
            case 24:
            case 25:
            case 26:
            case 27:
            case 397:
            case 398:
            case 399:
            case 400:
            case 401:
            case 402:
            case 403:
                return TileType.Wood;
            case 11:
            case 12:
            case 13:
            case 14:
            case 18:
            case 50:
            case 59:
            case 60:
            case 61:
            case 73:
            case 74:
            case 75:
            case 76:
            case 77:
            case 78:
            case 79:
            case 132:
            case 121:
            case 102:
            case 104:
            case 106:
            case 107:
            case 108:
            case 109:
            case 110:
            case 111:
            case 120:
                return TileType.Grass;
            case 19:
            case 22:
            case 23:
            case 72:
            case 80:
                return TileType.Stone;
            case 90:
            case 91:
                return TileType.Marble;
            case 103:
            case 112:
            case 113:
            case 114:
            case 115:
            case 116:
            case 122:
            case 125:
            case 476:
            case 478:
                return TileType.Earth;
            case 105:
            case 117:
            case 118:
            case 119:
                return TileType.Lava;
            case 2:
            case 6:
            case 7:
            case 8:
            case 9:
            case 51:
            case 52:
            case 53:
            case 54:
            case 55:
            case 56:
            case 57:
            case 58:
                return TileType.Water;
            case 10:
            case 30:
            case 31:
            case 34:
            case 92:
            case 93:
            case 94:
            case 95:
            case 97:
            case 123:
            case 124:
            case 404:
            case 405:
            case 406:
            case 422:
            case 410:
            case 411:
            case 412:
            case 413:
            case 414:
            case 415:
            case 416:
            case 475:
                return TileType.BlockProp;
            case 134:
            case 135:
                return TileType.BlockMarble;
            //case 1:
            //    return TileType.BlockWater;
        }
        return TileType.None;
    }
}
public class MapData
{
    public MapType mapType;
    public List<TileInfo> tileInfos;
}
public class TileInfo
{
    public TileType tileType;
    public bool blocked;
    public Vector2Int position;
}
#if UNITY_EDITOR
[CustomEditor(typeof(MapEditor))]
public class MapEditorUI : Editor
{
    public override void OnInspectorGUI()
    {
        MapEditor generator = (MapEditor)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Generate XMLs"))
        {
            EditorUtility.SetDirty(generator);
            generator.GenerateXMLs();
            EditorUtility.ClearDirty(generator);
        }
    }
}
#endif
