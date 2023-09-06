using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TileGroup[] mapTiles;
    public Vector2Int dimensions;
    private Vector2 offset;
    private GameObject[,] generatedTiles;
    private GameObject[,] generatedObjects;

    enum MapType { Basic };

    private static Vector2Int InvalidLocation = new Vector2Int(-1, -1);

    public GameObject GetTile(int x, int y) {
        x -= (int)offset.x;
        y -= (int)offset.y;

        if (x < 0 || x >= dimensions.x) return null;
        if (y < 0 || y >= dimensions.y) return null;
        return generatedTiles[x, y];
    }
    public GameObject GetTile(Vector3 pos)
    {
        return GetTile((int)pos.x, (int)pos.z);
    }

    [SerializeField]
    GameObject parent;

    void Start()
    {
        SetupMap(MapType.Basic);
    }

    private void OnDestroy()
    {
        Cleanup();
        // Destroy(parent);
    }

    void SetupMap(MapType mapType)// this 
    {
        if (mapType == MapType.Basic)
        {
            if (dimensions.sqrMagnitude < 10 || dimensions.x < 3 || dimensions.y < 3)
                Debug.LogError("unreasonable size");
            if (parent == null)
                Debug.LogError("bad parent");

            offset.y = (int)(-dimensions.y / 2);
            offset.x = (int)(-dimensions.x / 2);
            generatedTiles = new GameObject[dimensions.x, dimensions.y];
            generatedObjects = new GameObject[dimensions.x, dimensions.y];
            // var test = generatedTiles[0,0];
            MapUtils.Bounds = dimensions;
            MapUtils.MapOffset = new Vector2Int((int)offset.x, (int)offset.y);
        }
    }

    void Cleanup()
    {
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                Destroy(generatedTiles[x, y]);
                Destroy(generatedObjects[x, y]);
                generatedTiles[x, y] = null;
            }
        }
        generatedTiles = null;
    }

    Vector2Int FindOpenSpace()
    {
        int attempts = 100;
        while (attempts-- >= 0)
        {
            var attempt = new Vector2Int(Random.Range(0, dimensions.x), Random.Range(0, dimensions.y));
            var selected = generatedTiles[attempt.x, attempt.y];
            if (selected == null)
                return attempt;
        }

        return InvalidLocation;
    }

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
            var testDir = MapUtils.Dir4Lookup(dir) + new Vector2Int((int)pos.x, (int)pos.y);
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
        for (int d = 0; d < 8; d++, dir++)
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

    GameObject AddDecorationsPrefab(GameObject baseTile, GameObject[] decorations, bool guaranteedCreate = false)
    {
        if (decorations == null)
            return null;

        int emptyBias = 5;
        if (guaranteedCreate == true)
        {
            emptyBias = 0;
        }
        int which = Random.Range(0, decorations.Length + emptyBias);
        if (which >= decorations.Length)
            return null;

        Vector2 position = new Vector2(baseTile.transform.position.x, baseTile.transform.position.z);
        return AddDecorationsPrefab(position, decorations[which]);
    }
    /* public GameObject AddDecoration(GameObject baseTile, GameObject decoration)
     {
         if (decoration == null)
             return null;

         Vector2 position = new Vector2(baseTile.transform.position.x, baseTile.transform.position.z);
         return AddDecorationsPrefab(position, decoration);
     }*/

    public List<GameObject> GetAllObjectsOnTile(GameObject tile)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        var position = tile.transform.position;
        Vector2 pos = new  Vector2(position.x, position.z);
        pos -= offset;

        if (generatedObjects[(int)pos.x, (int)pos.y] != null)
            gameObjects.Add(generatedObjects[(int)pos.x, (int)pos.y]);
        return gameObjects;
    }

    public GameObject AddDecorationsPrefab(Vector3 position, GameObject decorationPrefab)
    {
        return AddDecorationsPrefab(new Vector2(position.x, position.z), decorationPrefab);
    }

    public GameObject AddDecorationsPrefab(Vector2 position, GameObject decorationPrefab)
    {
        if (decorationPrefab == null)
            return null;

        float angle = Random.Range(-180, 180);
        Vector3 newPos = new Vector3(position.x, 0.1f, position.y);
        var newDecoration = Instantiate(decorationPrefab, newPos, Quaternion.Euler(0, angle, 0));

        position -= offset;
        generatedObjects[(int)position.x, (int)position.y] = newDecoration;// todo .. should destroy prev obj
        return newDecoration;
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

    void ToolGenerateDecoration()
    {
        var tile = generatedTiles[0, 0];
        AddDecorationsPrefab(tile, mapTiles[0].Decorations, true);
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

        //ToolGenerateDecoration();
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
