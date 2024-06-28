using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Mathematics;

[System.Serializable]
public enum  LandType
{
    Dirt, Desert, Forest, Water, Grassland, Ice
}

[System.Serializable]
public struct TileGroup
{
    [SerializeField] 
    public GameObject[] Tiles;

    [SerializeField]
    public GameObject[] Decorations;

    [SerializeField]
    public LandType type;
}

public class MapUtils
{
    public static Vector2Int Bounds  { get; set; }
    public static Vector2Int MapOffset { get; set; }
    public static Vector2Int Constrain (Vector2Int pos)
    { 
        return new Vector2Int (Mathf.Clamp(pos.x,-Bounds.x, Bounds.x), Mathf.Clamp(pos.y, -Bounds.y, Bounds.y)); 
    }

    public static Vector2Int ConvertScreenPositionToMap(Vector2Int screenPos)
    {
        return new Vector2Int(screenPos.x - MapOffset.x, screenPos.y - MapOffset.y);
    }
    public static Vector2Int ConvertScreenPositionToMap(Vector3 screenPos)
    {
        return new Vector2Int((int)screenPos.x - MapOffset.x, (int)screenPos.z - MapOffset.y);
    }

    public static Vector2Int Dir8Lookup(int which)
    {
        which %= 8;
        Vector2Int[] directions = {
                new Vector2Int(-1, 0),
                new Vector2Int(-1, 1),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 1),
                new Vector2Int( 1, 0),
                new Vector2Int( 1,-1),
                new Vector2Int( 0,-1),
                new Vector2Int(-1,-1)};

        return directions[which];
    }

    public static Vector2Int Dir4Lookup(int which)
    {
        which %= 4;
        Vector2Int[] directions = {
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 0),
                new Vector2Int( 0,-1)};

        return directions[which];
    }

    public static bool IsDirValid(Vector2Int dir, Vector2Int bounds)
    {
        if (dir.x < 0 || dir.y < 0)
            return false;
        if (dir.x >= bounds.x || dir.y >= bounds.y)
            return false;
        return true;
    }

    public static bool IsDirValid(Vector2Int dir)
    {
        return IsDirValid(dir, Bounds);
    }
}
