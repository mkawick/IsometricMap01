using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;
using static UnityEditor.PlayerSettings;

public class MapGenerator : MonoBehaviour
{
    public TileGroup[] mapTiles;
    public Vector2Int dimensions;
    private Vector2 mapWorldOffset;
    private GameObject[,] generatedTiles;
    private GameObject[,] generatedObjects;
    List<Vector2Int> choosenStartingPositions;
    public System.Collections.Generic.List<UnityEngine.Vector2Int> ChoosenStartingPositions
    {
        get { return choosenStartingPositions; }
        set { choosenStartingPositions = value; }
    }
    enum MapType { Basic };

    private static Vector2Int InvalidLocation = new Vector2Int(-1, -1);

    public Vector2 WorldOffset() => mapWorldOffset;
    public GameObject GetTile(int x, int y) 
    {
        x -= (int)mapWorldOffset.x;
        y -= (int)mapWorldOffset.y;

        if (x < 0 || x >= dimensions.x) return null;
        if (y < 0 || y >= dimensions.y) return null;
        return generatedTiles[x, y];
    }
    public GameObject GetTile(Vector3 pos)
    {
        return GetTile((int)pos.x, (int)pos.z);
    }

    public Vector2 GetTilePosition(GameObject go)
    {
        for (int y = 0; y < dimensions.y; y++)
        {
            for (int x = 0; x < dimensions.x; x++)
            {
                if(generatedTiles[x, y] == go)
                {
                    return new Vector2(x, y);
                }
            }
        }
        return Vector2.negativeInfinity;
    }


    [SerializeField]
    GameObject tileNodeHeirarchyParent;
    [SerializeField]
    GameObject worldObjectsHeirarchyParent;

    void Start()
    {
        SetupMap(MapType.Basic);
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    void SetupMap(MapType mapType)// this need new types
    {
        if (mapType == MapType.Basic)
        {
            if (dimensions.sqrMagnitude < 10 || dimensions.x < 3 || dimensions.y < 3)
                Debug.LogError("unreasonable size");
            if (tileNodeHeirarchyParent == null)
                Debug.LogError("bad tile parent");
            if (worldObjectsHeirarchyParent == null)
                Debug.LogError("bad world objects parent");

            mapWorldOffset.y = (int)(-dimensions.y / 2);
            mapWorldOffset.x = (int)(-dimensions.x / 2);
            generatedTiles = new GameObject[dimensions.x, dimensions.y];
            generatedObjects = new GameObject[dimensions.x, dimensions.y];            

            MapUtils.Bounds = dimensions;
            MapUtils.MapOffset = new Vector2Int((int)mapWorldOffset.x, (int)mapWorldOffset.y);
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
        int numDirs = 4;
        int dir = Random.Range(0, numDirs);
        for (int d = 0; d < numDirs; d++, dir++)
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
        int numDirs = 8;
        int dir = Random.Range(0, numDirs);
        for (int d = 0; d < numDirs; d++, dir++)
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
        var newTile = Instantiate(prefab, new Vector3(location.x + mapWorldOffset.x, 0, location.y + mapWorldOffset.y), Quaternion.identity);
        newTile.transform.parent = tileNodeHeirarchyParent.transform;
        generatedTiles[location.x, location.y] = newTile;
        return newTile;
    }

    public bool CanBeBuiltOn(Vector3 pos)
    {
        int x = (int)pos.x - (int)mapWorldOffset.x;
        int y = (int)pos.z - (int)mapWorldOffset.y;
        if (generatedTiles[x, y].GetComponent<TileBehavior>().isWalkable && 
            generatedObjects[x, y] == null)
            return true;

            return false;
    }

    public bool IsWalkable(int x, int y, bool normalOffset = true)
    {
        if (normalOffset)
        {
            x -= (int)mapWorldOffset.x;
            y -= (int)mapWorldOffset.y;
        }
        if (generatedTiles[x, y].GetComponent<TileBehavior>().isWalkable 
            //&& generatedObjects[x, y] == null
           )
            return true;

        return false;
    }

    public PathingUtils.Passability GetPassability(int x, int y, bool normalOffset = true)
    {
        if (normalOffset)
        {
            x -= (int)mapWorldOffset.x;
            y -= (int)mapWorldOffset.y;
        }

        if (x < 0 || x >= dimensions.x) return PathingUtils.Passability.blocked;
        if (y < 0 || y >= dimensions.y) return PathingUtils.Passability.blocked;
        //PathingUtils.Passability.clear;
        return PathingUtils.Passability.clear;// generatedTiles[x, y];
    }

    public Vector3 TranslateMapToWorld(Vector2Int mapPos)
    {
        return new Vector3(mapPos.x + (int)mapWorldOffset.x, 0, mapPos.y + (int)mapWorldOffset.y);
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

    public List<GameObject> GetAllObjectsOnTile(GameObject tile)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        var position = tile.transform.position;
        Vector2 pos = new Vector2(position.x, position.z);
        pos -= mapWorldOffset;

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

        position -= mapWorldOffset;
        generatedObjects[(int)position.x, (int)position.y] = newDecoration;// todo .. should destroy prev obj
        newDecoration.transform.parent = worldObjectsHeirarchyParent.transform;
        return newDecoration;
    }

    float GetClosestDistance(Vector2 position, List<Vector2Int> arrayOfPoints)
    {
        float closest = float.MaxValue;
        foreach(var p in arrayOfPoints)
        {
            float distX = p.x - position.x;
            float distY = p.y - position.y;
            float dist = Mathf.Sqrt(distX * distX + distY * distY);
            if(dist < closest)
                closest = dist;
        }

        return closest;
    }

    List<Vector2Int> GenerateStartingPositions(int numPlayers, int numPositions)
    {
        // spread the players out as far as is reasonable 
        // since we are centered at 0, this is easy
        float currentAngle = Random.Range(0, 2f * Mathf.PI);
        float playerAngleAdd = 2f * Mathf.PI / numPlayers;
        Vector2 twoThirdsRadius = new Vector2(dimensions.x / 3f, dimensions.y / 3f);
        Vector2 middle = new Vector2(dimensions.x / 2f, dimensions.y / 2f);
        List<Vector2Int> positions = new List<Vector2Int>();

        for (int i=0; i< numPlayers; i++)
        {
            float cosCalc = Mathf.Cos(currentAngle);
            float sinCalc = Mathf.Sin(currentAngle);
            float maxX = cosCalc == 0 ? 0 : cosCalc * twoThirdsRadius.x;
            float maxY = sinCalc == 0 ? 0 : sinCalc * twoThirdsRadius.y;

            float x = maxX + middle.x;
            float y = maxY + middle.y;

            positions.Add(new Vector2Int( (int)x,(int)y));
            currentAngle += playerAngleAdd;
        }

        // subdivide the world into a grid and then choose the middle of each grid
        // then make sure that it's not too close to other chosen locations
        int worldDivisions = numPositions / 2;
        int xDivision = dimensions.x / worldDivisions;
        int midXDivision = xDivision / 2;
        int yDivision = dimensions.y / worldDivisions;
        int midYDivision = yDivision / 2;
        float minDist = (xDivision + yDivision) / 2;

        for (int i = numPlayers; i < numPositions; i++)
        {
            int numAttempts = 100;
            Vector2Int newPos = new Vector2Int();
            do
            {
                int x = Random.Range(0, worldDivisions) * xDivision + midXDivision;
                int y = Random.Range(0, worldDivisions) * yDivision + midYDivision;
                float dist = GetClosestDistance(new Vector2(x, y), positions);
                if (dist >= minDist)
                {
                    newPos = new Vector2Int(x, y);
                    break;
                }
            } while (--numAttempts > 0);
            Debug.Assert(numAttempts > 0);
            positions.Add(newPos);
        }

        return positions;
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

    void GenerateChunk(Vector2Int pos, int whichBiome, int numItemsToGenerate)
    {
        Stack<Vector2Int> tilePath = new Stack<Vector2Int>();
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

    // first positions are player positions
    public List<Vector2Int> Generate(int numPlayers) 
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

        if(chunksToGen < numPlayers)
            chunksToGen = numPlayers;

        choosenStartingPositions = GenerateStartingPositions(numPlayers, chunksToGen);
        int numItemsToGenerate = chunkSize;
        foreach (var spot in choosenStartingPositions)
        {
            int whichBiome = Random.Range(0, mapTiles.Length - 1);
            GenerateChunk(spot, whichBiome, numItemsToGenerate);
        }
        /*   for (int i=0; i< chunksToGen; i++)
           {
               int whichBiome = Random.Range(0, mapTiles.Length-1);
               GenerateChunk(whichBiome, chunkSize);
           }*/
        FillWithWater();

        //ToolGenerateDecoration();

        return choosenStartingPositions;
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
                var nemTile = Instantiate(tile, new Vector3(mapWorldOffset.x + x, 0, mapWorldOffset.y + y), Quaternion.identity);
                nemTile.transform.parent = tileNodeHeirarchyParent.transform;
            }
        }
    }
}
