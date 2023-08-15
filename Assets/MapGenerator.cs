using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Mathematics;

[System.Serializable]
public struct TileGroup
{
    [SerializeField] 
    public GameObject[] Tiles;
}

public class MapGenerator : MonoBehaviour
{
    public TileGroup[] mapTiles;
    public Vector2 dimensions;
    private Vector2 offset;

    [SerializeField]
    GameObject parent;

   // private UnityEngine.Random rand;
    void Start()
    {
       // rand = new UnityEngine.Random();
        if (dimensions.sqrMagnitude < 10 || dimensions.x < 3 || dimensions.y < 3)
            Debug.LogError ("unreasonable size");
        if (parent == null)
            Debug.LogError("bad parent");

        offset.y = (int)(-dimensions.y / 2);
        offset.x = (int)(-dimensions.x / 2);
    }

    int FindOpenSpace(int[] tracker)
    {
        int attempts = 100;
        // attempt random soloctien first to avoid bias
        while(attempts-- >= 0)
        {
            var selection = Random.Range(0, tracker.Length);
            if (tracker[selection] == 0)
                return selection;
        }

        // todo, start at random position
        for(int i=0; i<tracker.Length; i++)
        {
            if (tracker[i] == 0)
                return i;
        }
        return -1;
    }
    int ConvertToTileId(Vector2 pos, Vector2 bounds)
    {
        if (IsDirValid(pos, bounds) == false)
        {
           // Debug.LogError("bad position");
            return -1;
        }
        // todo .. better error checks
        return (int)(pos.x + pos.y * bounds.x);
    }
    static Vector2 DirLookup(int which)
    {
        which %= 8;
        Vector2[] directions = {
                new Vector2(-1, 0),
                new Vector2(-1, 1),
                new Vector2( 0, 1),
                new Vector2( 1, 1),
                new Vector2( 1, 0),
                new Vector2( 1,-1),
                new Vector2( 0,-1),
                new Vector2(-1,-1)};

        return directions[which];
    }

    bool IsDirValid(Vector2 dir, Vector2 bounds)
    {
        if (dir.x < 0 || dir.y < 0)
            return false;
        if (dir.x >= bounds.x || dir.y >= bounds.y)
            return false;
        return true;
    }

    int GenerateNewPosition(Stack<Vector2> tilePath, Vector2 bounds, int[] tracker)
    {
        var pos = tilePath.Peek();
        return GenerateNewPosition(pos, bounds, tracker);
    }
    int GenerateNewPosition(Vector2 pos, Vector2 bounds, int[] tracker)
    {
        int dir = Random.Range(0, 8);
        for(int d=0; d<8; d++, dir++)
        {
            var testDir = DirLookup(dir) + pos;
            int tileId = ConvertToTileId(testDir, bounds);
            if(tileId == -1)
            {
                continue;
            }
            if (IsDirValid(testDir, bounds) && tracker[tileId] == 0)
            {
                return tileId;
            }
        }
        return -1;
    }

    Vector2 ConvertToPos(int tileId)
    {
        return new Vector2((int)(tileId % (int)(dimensions.x)), (int)(tileId / (int)(dimensions.y)));

    }

    GameObject CreatePrefab(GameObject prefab, Vector2 location)
    {
        var newTile = Instantiate(prefab, new Vector3(location.x + offset.x, 0, location.y + offset.y), Quaternion.identity);
        newTile.transform.parent = parent.transform;
        return newTile;
    }

    void GenerateChunk(int whichBiome, int numItemsToGenerate, int[] tracker)
    {
        Stack<Vector2> tilePath = new Stack<Vector2>();
        int startSpot = FindOpenSpace(tracker);
        Vector2 pos = ConvertToPos(startSpot); 
        tilePath.Push(pos);
        tracker[startSpot] = 1;
        var biome = mapTiles[whichBiome];
        int whichBiomeTile = Random.Range(0, biome.Tiles.Length);
        var prefab = biome.Tiles[whichBiomeTile];
        CreatePrefab(prefab, pos);

        for (int i = 0; i < numItemsToGenerate; i++)
        {
            int whichTileInList = GenerateNewPosition(tilePath, dimensions, tracker);
            if (whichTileInList == -1)
                return;
            tracker[whichTileInList] = 1;

            whichBiomeTile = Random.Range(0, biome.Tiles.Length);
            prefab = biome.Tiles[whichBiomeTile];
            Vector2 newPos = ConvertToPos(whichTileInList);
            CreatePrefab(prefab, newPos);
            {// select 
                tilePath.Push(newPos);
            }
        }
        GrowChunk(biome, tilePath, tracker);
    }

    public void GrowChunk(TileGroup biome, Stack<Vector2> tilePath, int[] tracker)
    {
        foreach(var node in tilePath)
        {
            int whichTileInList = GenerateNewPosition(node, dimensions, tracker);
            if (whichTileInList == -1)
                return;
            tracker[whichTileInList] = 1;

            int whichBiomeTile = Random.Range(0, biome.Tiles.Length);
            var prefab = biome.Tiles[whichBiomeTile];
            Vector2 newPos = ConvertToPos(whichTileInList); 
            CreatePrefab(prefab, newPos);
            
        }
    }

    public void FillWithWater(int[] tracker)
    {
        int whichBiome =  mapTiles.Length - 1; // assume water
        var biome = mapTiles[whichBiome];

        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                var pos = new Vector2(x, y);
                int tileId = ConvertToTileId(pos, dimensions);
                if (tileId == -1)
                {
                    continue;
                }
                if (IsDirValid(pos, dimensions) && tracker[tileId] == 0)
                {
                    tracker[tileId] = 1;

                    int whichTile = Random.Range(0, biome.Tiles.Length);
                    var prefab = biome.Tiles[whichTile];
                    CreatePrefab(prefab, new Vector2(x, y));
                }
            }
        }
        // debugging only
        for(int i=0; i<tracker.Length; i++)
        {
            if (tracker[i] == 0)
            {
                Debug.Log("open " + i);
            }
        }
    }
    public void Generate()
    {
        int numItems = (int)(dimensions.x * dimensions.y);
        int [] tracker = new int[numItems];

        
        //while(odds > 0)
        for(int i=0; i<20; i++)
        {
            int whichBiome = Random.Range(0, mapTiles.Length-1);
            int numItemsToGenerate = 6;
            GenerateChunk(whichBiome, numItemsToGenerate, tracker);
        }
        FillWithWater(tracker);

    }

    void SimpleRandom()
    {
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                int whichBiome = Random.Range(0, mapTiles.Length);
                var biome = mapTiles[whichBiome];
                int whichTile = Random.Range(0, biome.Tiles.Length);
                var tile = biome.Tiles[whichTile];
                var nemTile = Instantiate(tile, new Vector3(offset.x + x, 0, offset.y + y), Quaternion.identity);
                nemTile.transform.parent = parent.transform;
            }
        }
    }
}
