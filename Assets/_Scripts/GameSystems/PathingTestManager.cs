using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;
using U8 = System.Byte;

public class PathingTestManager : MonoBehaviour
{
   // public PlayerTurnTaker[] playerArchetypes;
    public GameObject mouseCursor;
    enum MouseCursorState { None, StartCursor, EndCursor };
    MouseCursorState mouseCursorState;
    Camera currentCamera;
    public MapGenerator mapGenerator;
    IntVector3 startPosition, endPosition;

    bool pathShouldBeDrawn = false;

    TileMap_PathFinder tileMap = new TileMap_PathFinder();
    List<U8> map;
    IntVector3 dimensions;
    List<List<U8>> mapData;

    public static event Action<int> OnTurnChanged;

    void Start()
    {
        //if (isRegularGame)
        //{
        // to do create players
        GameModeManager.OnGameModeChanged += OnGameModeChanged;
        currentCamera = Camera.main;
        SetMouseCursor(MouseCursorState.None);

        startPosition = new IntVector3();
        endPosition = new IntVector3();
        tileMap = null;

        // }
        //else
        // {     
        //players.Add(Instantiate(playerArchetypes[0], playerCollectionNode.transform));
        // players.Add(Instantiate(playerArchetypes[1], playerCollectionNode.transform));
        // playerArchetypes[0].gameObject.SetActive(false);
        //  playerArchetypes[1].gameObject.SetActive(false);
        // }

    }

    void SetMouseCursor(MouseCursorState newState)
    {
        mouseCursorState = newState;
        switch (mouseCursorState)
        {
            case MouseCursorState.None:
                mouseCursor?.SetActive(false);
                break;

            case MouseCursorState.StartCursor:
                if(mouseCursor)
                {
                    mouseCursor.SetActive(true);
                    var renderer = mouseCursor.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", UnityEngine.Color.red);
                }
                break;

            case MouseCursorState.EndCursor:
                if (mouseCursor)
                {
                    mouseCursor.SetActive(true);
                    var renderer = mouseCursor.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", UnityEngine.Color.yellow);
                }
                break;
        }
    }
    void OnGameModeChanged(GameModeManager.GameMode mode, bool regularGame)
    {
        if (regularGame == true)// only test mode
            return;
        if (mode == GameModeManager.GameMode.PathingTest)
        {
            SetMouseCursor(MouseCursorState.None);
        }

    }

    private void Update()
    {
        if (currentCamera == null) return;
        if (mouseCursorState == MouseCursorState.None) return;
        TryMouseHitGround();

        if (Input.GetMouseButtonDown(0))
            MouseClickHandler();
        
    }

    void DrawPath(List<PathNode> path, Vector2 offset)
    {
        foreach (var p in path)
        {
            //Debug.DrawLine(p);
            var v = new Vector3(p.pos.x + offset.x, 1, p.pos.y + offset.y);
            Gizmos.DrawSphere(v, 0.3f);
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = UnityEngine.Color.yellow;
        

        if (pathShouldBeDrawn)
        {
            SetupTilemapInfo();

            //auto beginTime = std::chrono::high_resolution_clock::now();
            //var path = FindPath(new IntVector3(0, 0, 0), new IntVector3(4, 16, 0), tileMap);
            var path = FindPath(startPosition, endPosition, tileMap);
            var offset = mapGenerator.WorldOffset();

            DrawPath(path, offset);
        }
    }

    bool TryMouseHitGround()
    {
        RaycastHit raycastHit;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        int mask = (1 << LayerMask.NameToLayer("Ground"));
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, mask))
        {
            if (raycastHit.transform != null)
            {
                var obj = raycastHit.transform.gameObject;
                mouseCursor.transform.position = raycastHit.transform.position + new Vector3(0, 0.5f, 0);
                var tilePosition = mapGenerator.GetTilePosition(obj);
                Debug.Log("tile: {" + (int)tilePosition.x + ", " + (int)tilePosition.y + " }");

                // TODO: we seem the be grabbing the wrong tile positions
                if(mouseCursorState == MouseCursorState.StartCursor)
                {
                    startPosition = new IntVector3((int)tilePosition.x, (int)tilePosition.y, 0);
                }
                else if (mouseCursorState == MouseCursorState.EndCursor)
                {
                    endPosition = new IntVector3((int)tilePosition.x, (int)tilePosition.y, 0);
                }
                pathShouldBeDrawn = true;
                //gameUnitSelector.SelectUnit(obj);
                return true;
            }
            else
            {
                //gameUnitSelector.SelectUnit(null);
            }
        }
        else
        {
            //gameUnitSelector.SelectUnit(null);
        }
        return false;
    }

    private void OnEnable()
    {
        SetMouseCursor(MouseCursorState.None);
    }

    private void OnDisable()
    {
        SetMouseCursor(MouseCursorState.None);
    }


    void GenerateRandomMap(List<U8> map, IntVector3 dimensions)
    {
        int num = dimensions.x * dimensions.y;
        int lineStartPosition = 6 * dimensions.x;
        int lineEndPosition = lineStartPosition + 14;

        for (int i = 0; i < num; i++)
        {
            if (i >= lineStartPosition && i <= lineEndPosition)
            {
                map.Add(item: (byte)255);
                continue;
            }

           // UnityEngine.Random rand = UnityEngine.Random;
            var oddsOfBlock = UnityEngine.Random.value * 10.0f;
            if (oddsOfBlock < 1)
            {
                map.Add(item: (byte)255);
            }
            else
            {
                var passability = UnityEngine.Random.value * 30.0f;

                if (passability == 1)
                    map.Add(1);
                else if (passability > 1 && passability <= 3)
                    map.Add(2);
                else if (passability > 3 && passability <= 6)
                    map.Add(3);
                else if (passability > 6 && passability <= 10)
                    map.Add(4);
                else if (passability > 10 && passability <= 21)
                    map.Add(5);
                else if (passability > 21 && passability <= 24)
                    map.Add(6);
                else if (passability > 24 && passability <= 26)
                    map.Add(7);
                else if (passability > 26 && passability <= 29)
                    map.Add(8);
                else //if (passability == 0)
                    map.Add(9);
            }
        }
    }

    List<PathNode> FindPath(IntVector3 startPos, IntVector3 endPos, TileMap_PathFinder originalMap)
    {
        originalMap.ClearHistory();

        if (endPos.z == startPos.z)// same horizon... likely a straight path
        {
            List<PathNode> openSet = new List<PathNode>();
            List<PathNode> closedSet = new List<PathNode>();
            var outPath = originalMap.FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);
            if (outPath.path.Count != 0)
            {
                return outPath.path;
            }
        }

        // we either have different layers, or there is no direct path between the nodes 
        {
            var queueOfNodes = originalMap.FindPathWaypoints2D(startPos, endPos);
            List<PathNode> openSet = new List<PathNode>();
            List<PathNode> closedSet = new List<PathNode>();
            List<PathNode> totalPath = new List<PathNode>();

            foreach (var node in queueOfNodes)
            {
                //originalMap.ClearHistory();
                var outPath = originalMap.FindPathOnSingleLayer(node.Item1, node.Item2, openSet, closedSet);

                if (outPath.path.Count != 0)
                {
                    totalPath.AddRange(outPath.path);
                }
            }

            return totalPath;
        }

        return new List<PathNode>();
    }

    IntVector3 SetupPassibilityMap(List<U8> map)
    {
        var dims = mapGenerator.dimensions;
        IntVector3 dimensions = new IntVector3(dims.x, dims.y, 1);

        if (mapGenerator == null) return dimensions;
        map.Clear();

        for(int y = 0; y < dims.y; y++)
        {
            for(int x=0 ; x < dims.x; x++)
            {
                var passability = mapGenerator.GetPassability(x, y);
                map.Add((byte)passability);
               // tileMap.AddToMap(new IntVector3(x, y, 0), cost, cost == 255, PathingUtils.Passability.clear);
            }
        }

        return dimensions;
    }

    public void OnStartButtonPressed()
    {
        SetMouseCursor(MouseCursorState.StartCursor);
        
        pathShouldBeDrawn = false;
    }

    public void OnEndButtonPressed()
    {
        SetMouseCursor(MouseCursorState.EndCursor);
    }
    public void OnTestButtonPressed()
    {
        /* TileMap_PathFinder pather;
         pather*/
        TileMap_PathFinder tileMap = new TileMap_PathFinder();
        //tileMap.CreateMap(mapCosts, 1);
        //auto path = FindPath({ 0, 0, 0 }, { 6, 2, 0}, tileMap);

        //tileMap.CreateMap(mapCosts, 2);

        //tileMap.AddTunnel(Vector3(6, 6, 0), Vector3(6, 6, 1));

        //auto path = FindPath({ 0, 0, 0 }, { 7, 6, 1}, tileMap);
        List<U8> map = new List<U8>();
        IntVector3 dimensions = new IntVector3(20, 20, 1);
        GenerateRandomMap(map, dimensions);
        List<List<U8>> madData = new List<List<U8>>();
        madData.Add(map);
        TileMapMainInteraction.CreateMap(tileMap, madData, 1, dimensions);

        //auto beginTime = std::chrono::high_resolution_clock::now();
        var path = FindPath(new IntVector3(0, 0, 0), new IntVector3(4, 16, 0), tileMap);
        //auto endTime = std::chrono::high_resolution_clock::now();

        //auto duration = std::chrono::duration_cast<std::chrono::microseconds>(endTime - beginTime);

        tileMap.PrintMap(tileMap.GetMinExtents(), tileMap.GetMaxExtents());
        //PrintPath(path, tileMap.GetMinExtents(), tileMap.GetMaxExtents());

        // std::cout << "Elapsed Time: " << duration.count();
    }
    public void MouseClickHandler()
    {
        Debug.Log("Click");
        if (mouseCursorState == MouseCursorState.None)
            return;
        if (mouseCursorState == MouseCursorState.StartCursor)
        {
            mouseCursorState = MouseCursorState.None;
           /// startPosition = new IntVector3((int)tilePosition.x, (int)tilePosition.y, 0);
        }
        else if (mouseCursorState == MouseCursorState.EndCursor)
        {
            mouseCursorState = MouseCursorState.None;
            /// endPosition = new IntVector3((int)tilePosition.x, (int)tilePosition.y, 0);
        }
    }
    private void SetupTilemapInfo()
    {
        if (map != null)
            return;

        tileMap = new TileMap_PathFinder();
        map = new List<U8>();
        dimensions = SetupPassibilityMap(map);

        mapData = new List<List<U8>>();
        mapData.Add(map);
        TileMapMainInteraction.CreateMap(tileMap, mapData, 1, dimensions);// should be done once
    }
}
