using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Mathematics;

public class MapGenerator : MonoBehaviour
{
    public TileGroup[] mapTiles;
    public Vector2Int dimensions;
    private Vector2 offset;
    private GameObject[,] generatedTiles;

    private static Vector2Int InvalidLocation = new Vector2Int(-1,-1);

    [SerializeField]
    GameObject parent;

    void Start()
    {
        
        if (dimensions.sqrMagnitude < 10 || dimensions.x < 3 || dimensions.y < 3)
            Debug.LogError ("unreasonable size");
        if (parent == null)
            Debug.LogError("bad parent");

        offset.y = (int)(-dimensions.y / 2);
        offset.x = (int)(-dimensions.x / 2);
        generatedTiles = new GameObject[dimensions.x, dimensions.y];
        var test = generatedTiles[0,0];
        MapUtils.Bounds = dimensions;
        MapUtils.MapOffset = new Vector2Int((int)offset.x, (int)offset.y);
    }

    Vector2Int FindOpenSpace()
    {
        int attempts = 100;
        while(attempts-- >= 0)
        {
            var attempt = new Vector2Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y));
            var selected = generatedTiles[attempt.x, attempt.y];
            if (selected == null)
                return attempt;
        }

        return InvalidLocation;
    }

    int ConvertToTileId(Vector2Int pos, Vector2Int bounds)
    {
        if (MapUtils.IsDirValid(pos, bounds) == false)
        {
           // Debug.LogError("bad position");
            return -1;
        }
        // todo .. better error checks
        return (int)(pos.x + pos.y * bounds.x);
    }
  /*  static Vector2Int Dir8Lookup(int which)
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

    static Vector2Int Dir4Lookup(int which)
    {
        which %= 4;
        Vector2Int[] directions = {
                new Vector2Int(-1, 0),
                new Vector2Int( 0, 1),
                new Vector2Int( 1, 0),
                new Vector2Int( 0,-1)};

        return directions[which];
    }*/

   /* bool IsDirValid(Vector2 dir, Vector2 bounds)
    {
        if (dir.x < 0 || dir.y < 0)
            return false;
        if (dir.x >= bounds.x || dir.y >= bounds.y)
            return false;
        return true;
    }*/

    Vector2Int GenerateNewPosition(Stack<Vector2Int> tilePath, Vector2Int bounds)
    {
        var pos = tilePath.Peek();
        return GenerateNew4Position(pos, bounds);
    }

    Vector2Int GenerateNew4Position(Vector2Int pos, Vector2Int bounds)
    {
        int dir = Random.Range(0, 4);
        for (int d = 0; d < 4; d++, dir++)
        {
            var testDir = MapUtils.Dir4Lookup(dir) + new Vector2Int((int)pos.x, (int) pos.y);
            if (MapUtils.IsDirValid(testDir, bounds) == false)
                continue;

            if (generatedTiles[testDir.x, testDir.y] == null)
                return testDir;
        }
        return InvalidLocation;
    }

    Vector2Int GenerateNewPosition(Vector2Int pos, Vector2Int bounds)
    {
        int dir = Random.Range(0, 8);
        for(int d=0; d<8; d++, dir++)
        {
            var testDir = MapUtils.Dir8Lookup(dir) + new Vector2Int((int)pos.x, (int)pos.y);
            if (MapUtils.IsDirValid(testDir, bounds) == false)
                continue;

            if (generatedTiles[testDir.x, testDir.y] == null)
                return testDir;
        }
        return InvalidLocation;
    }

    GameObject CreatePrefab(GameObject prefab, Vector2Int location)
    {
        var name = prefab.name;
        var newTile = Instantiate(prefab, new Vector3(location.x + offset.x, 0, location.y + offset.y), Quaternion.identity);
        newTile.transform.parent = parent.transform;
        generatedTiles[location.x, location.y] = newTile;
        return newTile;
    }

    GameObject AddDecorationsPrefab(GameObject baseTile, GameObject[] decorations)
    {
        if (decorations == null)
            return null;

        int which = Random.Range(0, decorations.Length + 5);
        if (which >= decorations.Length)
            return null;

        float angle = Random.Range(-180, 180);
        var decoration = Instantiate(decorations[which], baseTile.transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(0, angle, 0));

        decoration.transform.parent = baseTile.transform;
        return decoration;
    }

    void GenerateChunk(int whichBiome, int numItemsToGenerate)
    {
        Stack<Vector2Int> tilePath = new Stack<Vector2Int>();
        Vector2Int pos = FindOpenSpace();
        if (pos == InvalidLocation)
            return;

        tilePath.Push(pos);
        var biome = mapTiles[whichBiome];
         int whichBiomeTile = Random.Range(0, biome.Tiles.Length);
         var prefab = biome.Tiles[whichBiomeTile];
         var tile = CreatePrefab(prefab, pos);
         AddDecorationsPrefab(tile, biome.Decorations);

        for (int i = 0; i < numItemsToGenerate; i++)
        {
            var location = GenerateNewPosition(tilePath, dimensions);
            if (location == InvalidLocation)
                return;

            whichBiomeTile = Random.Range(0, biome.Tiles.Length);
            prefab = biome.Tiles[whichBiomeTile];
            tile = CreatePrefab(prefab, location);
            AddDecorationsPrefab(tile, biome.Decorations);
            {
                tilePath.Push(location);
            }
        }
        GrowChunk(biome, tilePath);
    }

    public void GrowChunk(TileGroup biome, Stack<Vector2Int> tilePath)
    {
        foreach(var node in tilePath)
        {
            var pos = GenerateNewPosition(tilePath, dimensions);
            if (pos == InvalidLocation)
                continue;

            int whichBiomeTile = Random.Range(0, biome.Tiles.Length);
            var prefab = biome.Tiles[whichBiomeTile];
            CreatePrefab(prefab, pos);            
        }
    }

    bool IsPositionNextToLand(Vector2Int pos, Vector2Int bounds)
    {
        if (generatedTiles[pos.x, pos.y] != null)
            return true;

        for (int d = 0; d < 4; d++)
        {
            var testDir = pos + MapUtils.Dir4Lookup(d);
            if(MapUtils.IsDirValid(testDir, bounds) == false)
            {
                continue;
            }
            var obj = generatedTiles[(int)testDir.x, (int)testDir.y];
            if (obj != null)
            {
                var tileBehaviour = obj.GetComponent<TileBehavior>();
                if(tileBehaviour.isFloatable == false)
                {
                    return true;
                }

            }
        }
        return false;
    }

    public void FillWithWater()
    {
        int whichBiome =  mapTiles.Length - 1; // assume water
        var biome = mapTiles[whichBiome];

        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                var pos = new Vector2Int(x, y);
                if(MapUtils.IsDirValid(pos, dimensions) == false) 
                    continue;
                if (generatedTiles[pos.x, pos.y] != null)
                    continue;

                bool isNextToLand = IsPositionNextToLand(pos, dimensions);
                int whichTile = 0;
                if(isNextToLand == true)
                {
                    whichTile = biome.Tiles.Length-1;
                }
                var prefab = biome.Tiles[whichTile];
                CreatePrefab(prefab, pos);               
            }
        }
    }
    public void Generate()
    {
        int numItems = (int)(dimensions.x * dimensions.y);

        int chunksToGen = 5;
        int chunkSize = 7;

        if (numItems > 1599)
        {
            chunksToGen = 50;
            chunkSize = 12;
        }
        else if (numItems > 899)
        {
            chunksToGen = 30;
            chunkSize = 10;
        }
        else if (numItems > 399)
        {
            chunksToGen = 20;
            chunkSize = 7;
        }
        else if (numItems > 99)
        {
            chunksToGen = 1;
            chunkSize = 16;
        }

        


        for (int i=0; i< chunksToGen; i++)
        {
            int whichBiome = Random.Range(0, mapTiles.Length-1);
            GenerateChunk(whichBiome, chunkSize);
        }
        FillWithWater();

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
