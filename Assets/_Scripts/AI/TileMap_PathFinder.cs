using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

using GameMap = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapTile>>;
using U8 = System.Byte;

//----------------------------------------------------------------
//----------------------------------------------------------------

public class ShaftEntry
{
	public IntVector3 startLayer, endLayer;
	public int shaftIndex;
	public int elevatorIndex;
	public float hCost, gCost;
	public float fCost() { return gCost + hCost; }
	public float costToUse;

	public ShaftEntry(IntVector3 startLayer,
		IntVector3 endLayer,
		int shaftIndex,
		int elevatorIndex,
		float hCost,
		float gCost,
		float costToUse) 
	{ 
		this.startLayer = startLayer; 
		this.endLayer = endLayer; 
		this.shaftIndex = shaftIndex;
		this.elevatorIndex = elevatorIndex;
		this.hCost = hCost;
		this.gCost = gCost;
		this.costToUse = costToUse;
	}
    //new ShaftEntry( pair.start, pair.end, shaftIndex, -1, pair.distCalc, endDistCalc, pair.cost);
    public bool IsShaft() { return shaftIndex != -1; }
};


//----------------------------------------------------------------
//----------------------------------------------------------------

struct HistoricalSearches
{
	LinkedList<Tuple<IntVector3, IntVector3>> storedSearch;
    float cost;
};

//----------------------------------------------------------------
//----------------------------------------------------------------


public class LRUCache<K, V>
{
    private int capacity;
    private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
    private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

    public LRUCache(int capacity)
    {
        this.capacity = capacity;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public V get(K key)
    {
        LinkedListNode<LRUCacheItem<K, V>> node;
        if (cacheMap.TryGetValue(key, out node))
        {
            V value = node.Value.value;
            lruList.Remove(node);
            lruList.AddLast(node);
            return value;
        }
        return default(V);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void add(K key, V val)
    {
        if (cacheMap.TryGetValue(key, out var existingNode))
        {
            lruList.Remove(existingNode);
        }
        else if (cacheMap.Count >= capacity)
        {
            RemoveFirst();
        }

        LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
        LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
        lruList.AddLast(node);
        // cacheMap.Add(key, node); - here's bug if try to add already existing value
        cacheMap[key] = node;
    }

    private void RemoveFirst()
    {
        // Remove from LRUPriority
        LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
        lruList.RemoveFirst();

        // Remove from cache
        cacheMap.Remove(node.Value.key);
    }
}

class LRUCacheItem<K, V>
{
    public LRUCacheItem(K k, V v)
    {
        key = k;
        value = v;
    }
    public K key;
    public V value;
}

//----------------------------------------------------------------
//----------------------------------------------------------------

public class TileMapMainInteraction
{
	public void CreateMap(TileMap_PathFinder tileMap, List<List<U8>> mapCosts, int numLayers, IntVector3 dimensions)
	{
		int bigInt = 100000;
		int smallInt = -bigInt;
		IntVector3 minExtent = new IntVector3(bigInt, bigInt, bigInt);
        IntVector3 maxExtent = new IntVector3(smallInt, smallInt, smallInt);
		tileMap.SetMaxExtents(minExtent, minExtent);

		tileMap.WipeMap();
		tileMap.ClearHistory();

		// MUST be not hard coded
		for (int z = 0; z < numLayers; z++)
		{
			for (int x = 0; x < dimensions.x; x++)
			{
				for (int y = 0; y < dimensions.y; y++)
				{
					float cost = (float)(mapCosts[z][y * dimensions.x + x]);
					tileMap.AddToMap(new IntVector3(x, y, 0), cost, cost == 255, PathingUtils.Passability.clear);
				}
			}
		}
	}
    LinkedList<System.Tuple<IntVector3, IntVector3>> FindPathWaypoints(TileMap_PathFinder tileMap, IntVector3 startPos, IntVector3 endPos)
	{
		return tileMap.FindPathWaypointsWithShafts(startPos, endPos);
	}
}

public class TileMap_PathFinder
{
	GameMap tiles;
	//std::map<int, std::map<int, MapTile>> tiles; // sorted by z
	Dictionary<int, PathNode> pathNodes;
	List<Stairs> stairs;
	List<Elevator> elevators;
	IntVector3 minExtent, maxExtent;
	LRUCache<int, HistoricalSearches> lruHistoricalSearches;

	//float GetPathCost(IntVector3 start, IntVector3 end);

	float PathImpassible = float.MaxValue;
	float doorObstructed;
	float wallImpassible = 100;

	//-----------------------------------------------------------
	public TileMap_PathFinder()
	{
		lruHistoricalSearches = new LRUCache<int, HistoricalSearches>(12);
		doorObstructed = 4;
	}

	bool ArePositionsValid(IntVector3 startPos, IntVector3 endPos)
	{
		var startHash = PathingUtils.CalcHash(startPos);
		if (tiles.ContainsKey(startPos.z) == false ||
			(tiles[startPos.z].ContainsKey(startHash) == false))
		{
			return false;
		}

		var endHash = PathingUtils.CalcHash(endPos);
		if (tiles.ContainsKey(endPos.z) == false ||
			(tiles[endPos.z].ContainsKey(endHash) == false))
		{
			return false;
		}
		return true;
	}

	public bool IsOnStairsOrElevator(IntVector3 startPos, IntVector3 endPos)
	{
		if (startPos.x != endPos.x || startPos.y != endPos.y)
			return false;

		for (int i = 0; i < stairs.Count - 1; i++)
		{
			var stair = stairs[i];
			if (stair.Contains(startPos, endPos))
			{
				for (int j = i + 1; j < stairs.Count; j++)
				{
					var stairEnd = stairs[j];
					if (stairEnd.Contains(startPos, endPos))
					{
						return true;
					}
				}
				//return false;
			}
		}
		foreach (var elevator in elevators)
		{
			if (elevator.Contains(startPos, endPos))
			{
				return true;
			}
		}

		return false;
	}

	public void WipeMap()
	{
		tiles.Clear();
	}

	public void ClearHistory()
	{
		foreach (var node in pathNodes)
		{
			node.Value.ClearPath();
		}
	}

	void ReorderStairEndpoints()
	{
		foreach (var stair in stairs)
		{
			if (stair.start.z > stair.end.z)
			{
				var end = stair.end;
				stair.end = stair.start;
				stair.start = end;
			}
		}
	}

	List<int> GrabAllStairsSharingSameXY(int startingIndex)
	{
		List<int> contiguousIndices = new List<int>();
		contiguousIndices.Add(startingIndex);
		var basePos = stairs[startingIndex];

		for (int i = startingIndex + 1; i < stairs.Count; i++)
		{
			if (stairs[i].start.x == basePos.start.x &&
				stairs[i].start.y == basePos.start.y)
			{
				int baseZ = basePos.start.z;
				int numFloorsBetween = stairs[i].start.z - baseZ;
				if (numFloorsBetween > 1)
				{
					return contiguousIndices;
				}
				contiguousIndices.Add(i);
				basePos = stairs[i];// advance to next floor
			}
		}

		return contiguousIndices;
	}

	void CoalesceStairwells()
	{
		ReorderStairEndpoints();

		stairs.Sort((a, b) => b.start.z - a.start.z);

		/*  auto hasNeighborOnSameLevel = [](IntVector3 & pos, std::map<int, std::map<int, MapTile>>& tiles)
              {
                  int level = pos.z;
                  if (tiles.find(level) == tiles.end() ||
                      (tiles[level].Count == 1))// no tiles or only the stairs
                  {
                      return false; // no tiles on that layer
                  }
                  for (auto& tile : tiles[level])
                  {
                      if (tile.second.pos == pos)
                          continue;
                      if (GetCostTo(tile.second.pos, pos) <= 1)// we want a really close tile
                          return true;
                  }
                  return false;
              };*/

		// do not, in fact, coalesce the stairs - as per Doru - Mickey
		/*for (int i = 0; i < stairs.Count; i++)
		{
			auto stair = stairs[i];
			bool hasNeighborOnHigherLevel = hasNeighborOnSameLevel(stair.end, tiles);
			bool hasNeighborSameLevel = hasNeighborOnSameLevel(stair.start, tiles);
			if (!hasNeighborOnHigherLevel && hasNeighborSameLevel)
			{
				auto listOfStairIndices = GrabAllStairsSharingSameXY(i); // ordered
				if (listOfStairIndices.Count > 1)
				{
					int finalIndex = listOfStairIndices.Count - 1;
					stairs[i].end = stairs[listOfStairIndices[finalIndex]].end;
					for (int j = finalIndex; j > 0; j--)// careful on the indices
					{
						stairs.erase(stairs.begin() + listOfStairIndices[j]);
					}
				}
			}
		}*/
	}

	public bool AddToMap(IntVector3 pos, float cost, bool blocked, PathingUtils.Passability passable)
	{
		bool alreadyExists = false;
		MapTile tile = new MapTile(pos, (U8)cost, blocked, passable);
		var posHash = PathingUtils.CalcHash(pos);
		tiles[pos.z][posHash] = tile;

		PathNode pathNode = new PathNode(tile);

		if (tiles[pos.z].ContainsKey(posHash) == false)
		{
			alreadyExists = true;
			pathNodes[posHash] = pathNode;
		}
		else
		{
			pathNodes.Add(posHash, pathNode);
		}

		UpdateExtents(pos);

		if (alreadyExists)
			return false;
		return true;
	}

	public void ClearStairs()
	{
		stairs.Clear();
	}

	public void ClearElevators()
	{
		elevators.Clear();
	}

	public bool AddStairs(IntVector3 p1, IntVector3 p2, float cost)
	{
		Debug.Assert(p1.z != p2.z);
		Debug.Assert(cost > 0);

		// verify no duplicates
		for (int i = 0; i < stairs.Count; i++)
		{
			var pair = stairs[i];
			if ((pair.start == p1 || pair.end == p1) &&
				(pair.start == p2 || pair.end == p2))
			{
				return false;
			}
		}

		int newIndex = stairs.Count;
		stairs.Add(new Stairs(p1, p2, cost, 0));
		return true;
	}

	public bool AddElevator(Elevator config)
	{
		Debug.Assert(config.costMovingAway > 0);
		Debug.Assert(config.costMovingToward > 0);
		Debug.Assert(config.floors.Count > 0);
		Debug.Assert(config.elevatorPos.Count > 0);
		Debug.Assert(config.costPerFloorDistanceConfig.Count > 0);

		elevators.Add(config);
		return true;
	}

	public void SetBlocked(IntVector3 tilePosition, bool blocked)
	{
		var hash = PathingUtils.CalcHash(tilePosition);
		var tile = tiles[tilePosition.z][hash];
		if (tile == tiles[tilePosition.z].Last().Value)
			return;

		tile.blocked = blocked;
	}

	public void SetCost(IntVector3 tilePosition, float cost)
	{
		var hash = PathingUtils.CalcHash(tilePosition);
		var tile = tiles[tilePosition.z][hash];
		if (tile == tiles[tilePosition.z].Last().Value)
			return;

		tile.cost = (U8)cost;
	}

	public void SetPassability(IntVector3 tilePosition, PathingUtils.Passability passability)
	{
		var hash = PathingUtils.CalcHash(tilePosition);
		var tile = tiles[tilePosition.z][hash];
		if (tile == tiles[tilePosition.z].Last().Value)
			return;

		tile.passability = passability;
	}

	public bool IsValidMapPosition(IntVector3 pos)
	{
		return tiles[pos.z].ContainsKey(PathingUtils.CalcHash(pos)) == true;
	}

	public void PrintMap(IntVector3 minExtent, IntVector3 maxExtent)
	{
		for (int z = minExtent.z; z < maxExtent.z; z++)
		{
			Console.WriteLine("     ");
			for (int y = minExtent.y; y < maxExtent.y; y++)
			{
				Console.WriteLine("{0}  ", y);
			}

			Console.WriteLine("\n");
			for (int y = minExtent.y; y < maxExtent.y; y++)
			{
				string costs = "";// = new string();
				string passables = "";// = new string();

				for (int x = minExtent.x; x < maxExtent.x; x++)
				{
					var pos = new IntVector3(x, y, z);
					if (IsValidMapPosition(pos))
					{
						var tile = GetTile(pos);
						char printableCost = (char)(tile.cost + '0');
						if (tile.blocked == true)
							printableCost = '.';

						costs += printableCost;
						costs += "  ";
						char printablePassable = ((char)tile.passability);
						passables += printablePassable;
						passables += " ";
					}
					else
					{
						costs += "  ";
						passables += "  ";
					}
				}

				Console.WriteLine("  {0}   {1} --- {2}", y, costs, passables);
			}
			Console.WriteLine("\n");
		}
	}

	public GameMap GetTiles()
	{
		return tiles;
	}

	public bool IsInSet(List<PathNode> closedSet, PathNode needle)
	{
		foreach (var node in closedSet)
		{
			if (node.pos == needle.pos && node.referenceTile == needle.referenceTile)
				return true;
		}
		return false;
	}

	public PathData FindPathOnSingleLayer(IntVector3 startPos, IntVector3 endPos, List<PathNode> openSet, List<PathNode> closedSet)
	{
		var begin = GetPathNode(startPos);
		var end = GetPathNode(endPos);
		if (begin == null || end == null)
		{
			return new PathData();
		}
		openSet.Add(begin);
		while (openSet.Count > 0)
		{
			int selectedIndex = 0;
			var currentNode = openSet[0]; // search me baby

			for (int i = 1; i < openSet.Count; i++)
			{
				var testNode = openSet[i];
				if (testNode.fCost() < currentNode.fCost() || (testNode.fCost() == currentNode.fCost() && testNode.hCost < currentNode.hCost))
				{
					currentNode = testNode;
					selectedIndex = i;
				}
			}

			closedSet.Add(currentNode);
			openSet.RemoveAt(selectedIndex);

			if (currentNode.pos == endPos)
			{
				return new PathData(currentNode);
			}
			var neighborList = GetNeighbors(currentNode.pos);

			foreach (var neighbor in neighborList)
			{
				if (neighbor.referenceTile.blocked || IsInSet(closedSet, neighbor))
					continue;
				float wallCost = GetNeighborPassabilityCost(currentNode, neighbor);
				if (wallCost == wallImpassible)
					continue;

				int tileCost = (int)(currentNode.referenceTile.cost * wallCost);
				int currentGCost = currentNode.gCost;
				var gCost = tileCost * (currentGCost + PathingUtils.GetCostTo(currentNode.pos, neighbor.pos));
				if (gCost < neighbor.gCost || !IsInSet(openSet, neighbor))
				{
					neighbor.cameFrom = currentNode;
					neighbor.gCost = (int)gCost;
					neighbor.hCost = (int)(tileCost * PathingUtils.GetCostTo(neighbor.pos, endPos));

					if (!IsInSet(openSet, neighbor))
					{
						openSet.Add(neighbor);
					}
				}
			}
		}

		return new PathData();
	}

	public LinkedList<System.Tuple<IntVector3, IntVector3>> FindPathWaypoints2D(IntVector3 startPos, IntVector3 endPos)
	{
		ClearHistory();
		Dictionary<int, bool> visited = new Dictionary<int, bool>();
		List<int> shaftNodes = new List<int>();
		var outPath = Stairs_DepthFirstSearch(startPos, endPos, visited, shaftNodes);
		if (outPath == false)
			return new LinkedList<Tuple<IntVector3, IntVector3>>();


		LinkedList<System.Tuple<IntVector3, IntVector3>> output = new LinkedList<Tuple<IntVector3, IntVector3>>();
		// start node to first shaft
		output.AddLast(new Tuple<IntVector3, IntVector3>(startPos, stairs[shaftNodes[0]].start));
		if (shaftNodes.Count > 1)
		{
			//std::cout << shaftNodes.Count << std::endl;
			// always ignore the first node
			for (int i = 0; i < shaftNodes.Count - 1; i++)
			{
				int currentShaftIndex = shaftNodes[i];
				int nextShaftIndex = shaftNodes[i + 1];
				output.AddLast(new Tuple<IntVector3, IntVector3>(stairs[currentShaftIndex].end, stairs[nextShaftIndex].start));
			}
		}
		// last shaft to the 
		int lastShaftIndex = shaftNodes.LastOrDefault();

		output.AddLast(new Tuple<IntVector3, IntVector3>(stairs[lastShaftIndex].end, endPos));
		return output;
	}

	internal bool AttemptAssembleSingleLevelPath(IntVector3 startPos, IntVector3 endPos, LinkedList<Tuple<IntVector3, IntVector3>> outputPath, ref float PathCost)
	{
		List<PathNode> openSet = new List<PathNode>();
		List<PathNode> closedSet = new List<PathNode>();
		PathCost = float.MaxValue;
		var foundPath = FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);
		if (foundPath.path.Count != 0)
		{
			PathCost = foundPath.fcost;
			outputPath.AddLast(new LinkedListNode<Tuple<IntVector3, IntVector3>>(new Tuple<IntVector3, IntVector3>(startPos, endPos)));
			return true;
		}
		return false;
	}

	bool GetCachedResult(IntVector3 startPos, IntVector3 endPos, LinkedList<System.Tuple<IntVector3, IntVector3>> outputPath, float cost)
	{
		// first sort
		if (startPos.z < endPos.z)
		{
			// start should be on the bottom
		}
		if (startPos.y < endPos.y)
		{
			// left to right can overwrite the
		}
		return false;
	}

	public LinkedList<System.Tuple<IntVector3, IntVector3>> FindPathWaypointsWithShafts(IntVector3 startPos, IntVector3 endPos)
	{
		ClearHistory();
		if (endPos.z == startPos.z)// same horizon... likely a straight path,this code really isn't needed much anymore
		{
			float Cost = 0;
			LinkedList<System.Tuple<IntVector3, IntVector3>> outputPath = new LinkedList<Tuple<IntVector3, IntVector3>>();
			if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, ref Cost))
			{
				return outputPath;
			}
		}

		Dictionary<int, bool> visitedStairs = new Dictionary<int, bool>();
		Dictionary<int, bool> visitedElevators = new Dictionary<int, bool>();
		System.Tuple<float, List<ShaftEntry>> nodePath = new Tuple<float, List<ShaftEntry>>(0, new List<ShaftEntry>());

		bool outPath = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
		if (outPath == false)
			return new LinkedList<System.Tuple<IntVector3, IntVector3>>();


		LinkedList<System.Tuple<IntVector3, IntVector3>> output = new LinkedList<Tuple<IntVector3, IntVector3>>();
		// start node to first shaft
		output.AddLast(new System.Tuple<IntVector3, IntVector3>(startPos, nodePath.Item2[0].startLayer));
		if (nodePath.Item2.Count > 1)
		{
			for (int i = 0; i < nodePath.Item2.Count - 1; i++)
			{
				var shaft = nodePath.Item2[i];
				output.AddLast(new System.Tuple<IntVector3, IntVector3>(shaft.endLayer, nodePath.Item2[i + 1].startLayer));
			}
		}
		// last shaft to the end
		output.AddLast(new System.Tuple<IntVector3, IntVector3>(nodePath.Item2[nodePath.Item2.Count - 1].endLayer, endPos));
		return output;
	}


	void RemoveExtraShafts(List<ShaftEntry> nodePath)
	{
		if (nodePath.Count == 2)
		{
			if (nodePath[1].startLayer == nodePath[0].endLayer)
			{
				// we can always path one layer
				nodePath.Remove(nodePath[0]);
			}
		}
		else
		{
			// search for stairs that are 1 level apart and pick the first one only removing others
			for (int i = 0; i < nodePath.Count - 1; i++)// the -1 is important
			{
				var node = nodePath[i];
				if (node.IsShaft() == false)
					continue;
				// we do not need to advance the index, since we remove the current node
				// also, never remove the last node
				for (int j = i + 1; j < nodePath.Count - 1;)
				{
					var afternode = nodePath[j];
					if (!afternode.IsShaft())
						break;
					if (afternode.startLayer == node.endLayer)
					{
						node.endLayer = afternode.endLayer;
						nodePath.RemoveAt(j);

					}
					else
					{
						break;
					}
				}
			}
		}
		// we can always remove the last node
		var lastNode = nodePath[nodePath.Count - 1];
		if (lastNode.IsShaft())
		{
			nodePath.Clear();
		}
	}


	public LinkedList<System.Tuple<IntVector3, IntVector3>> FindPathWaypointsWithShaftsCleanedUp(IntVector3 startPos, IntVector3 endPos)
	{
		LinkedList<System.Tuple<IntVector3, IntVector3>> outputPath = new LinkedList<Tuple<IntVector3, IntVector3>>();
		float pathCost = 0;

		if (GetCachedResult(startPos, endPos, outputPath, pathCost))
		{
			return outputPath;
		}

		ClearHistory();

		if (endPos.z == startPos.z)// same horizon... likely a straight path, this code really isn't needed much anymore
		{
			float Cost = 0;
			if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, ref Cost))
			{
				return outputPath;
			}
		}
		if (IsOnStairsOrElevator(startPos, endPos))
		{
			outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(startPos, endPos));
			return outputPath;
		}

		Dictionary<int, bool> visitedStairs = new Dictionary<int, bool>();
		Dictionary<int, bool> visitedElevators = new Dictionary<int, bool>();
		System.Tuple<float, List<ShaftEntry>> nodePath = new Tuple<float, List<ShaftEntry>>(0, new List<ShaftEntry>());

		bool didWeFindAPathToDest = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
		if (didWeFindAPathToDest == false)
			return outputPath;

		RemoveExtraShafts(nodePath.Item2);

		if (nodePath.Item2.Count == 0) // can be true for VERY close nodes
		{
			outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(startPos, endPos));
			return outputPath;
		}

		// start node to first shaft
		outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(startPos, nodePath.Item2[0].startLayer));
		if (nodePath.Item2.Count > 1)
		{
			for (int i = 0; i < nodePath.Item2.Count - 1; i++)
			{
				var shaft = nodePath.Item2[i];
				outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(shaft.endLayer, nodePath.Item2[i + 1].startLayer));
			}
		}
		// last shaft to the end
		outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(nodePath.Item2[nodePath.Item2.Count - 1].endLayer, endPos));
		return outputPath;
	}

	float FindPathWaypointsCost(IntVector3 startPos, IntVector3 endPos)
	{
		ClearHistory();

		LinkedList<System.Tuple<IntVector3, IntVector3>> outputPath = new LinkedList<Tuple<IntVector3, IntVector3>>();
		if (endPos.z == startPos.z)// same horizon... likely a straight path, this code really isn't needed much anymore
		{
			float Cost = 0;
			if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, ref Cost))
			{
				return Cost;
			}
		}
		if (IsOnStairsOrElevator(startPos, endPos))
		{
			outputPath.AddLast(new System.Tuple<IntVector3, IntVector3>(startPos, endPos));
			return float.MaxValue;
		}

		Dictionary<int, bool> visitedStairs = new Dictionary<int, bool>();
		Dictionary<int, bool> visitedElevators = new Dictionary<int, bool>();
		System.Tuple<float, List<ShaftEntry>> nodePath = new Tuple<float, List<ShaftEntry>>(0, new List<ShaftEntry>());


		bool didWeFindAPathToDest = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
		if (didWeFindAPathToDest == false)
			return float.MaxValue;

		return nodePath.Item1;
	}

	LinkedList<System.Tuple<IntVector3, float>> FindClosestLocation(IntVector3 startPos, List<IntVector3> targetLocations)
	{
		LinkedList<System.Tuple<IntVector3, float>> prioritizedNodes = new LinkedList<Tuple<IntVector3, float>>();
		foreach (var target in targetLocations)
		{
			Dictionary<int, bool> visitedStairs = new Dictionary<int, bool>();
			Dictionary<int, bool> visitedElevators = new Dictionary<int, bool>();
			System.Tuple<float, List<ShaftEntry>> nodePath = new Tuple<float, List<ShaftEntry>>(0, new List<ShaftEntry>());

			bool outPath = StairsAndElevators_DepthFirstSearch(startPos, target, visitedStairs, visitedElevators, nodePath);
			if (outPath)
			{
				prioritizedNodes.AddLast(new System.Tuple<IntVector3, float>(target, nodePath.Item1));
			}
		}

		prioritizedNodes.OrderBy(a => a.Item2);
		return prioritizedNodes;
	}

	public void SetMaxExtents(IntVector3 min, IntVector3 max)
	{
		minExtent = min;
		maxExtent = max;
	}

	public void UpdateExtents(IntVector3 pos)
	{
		if (minExtent.x > pos.x)
			minExtent.x = pos.x;
		if (minExtent.y > pos.y)
			minExtent.y = pos.y;
		if (minExtent.z > pos.z)
			minExtent.z = pos.z;

		if (maxExtent.x <= pos.x)
			maxExtent.x = pos.x + 1;
		if (maxExtent.y <= pos.y)
			maxExtent.y = pos.y + 1;
		if (maxExtent.z <= pos.z)
			maxExtent.z = pos.z + 1;
	}

	List<ShaftEntry> GetAllStairsAndElevatorsOnLayer(IntVector3 startPos, IntVector3 endPos)
	{
		List<ShaftEntry> shaftIndices = new List<ShaftEntry>();
		AggregateStairs(startPos, endPos, shaftIndices);
		AggregateElevators(startPos, endPos, shaftIndices);

		// sort this list by the least costly paths first
		shaftIndices.Sort(delegate (ShaftEntry a, ShaftEntry b)
		{
			return (int)(b.fCost() - a.fCost());
		});

		return shaftIndices;
	}

	void AggregateElevators(IntVector3 startLayer, IntVector3 endLayer, List<ShaftEntry> shaftIndices)
	{
		for (int elevatorIndex = 0; elevatorIndex < elevators.Count; elevatorIndex++)
		{
			var elevator = elevators[elevatorIndex];
			int NoFloorFound = -10000;
			int foundIndex = NoFloorFound;
			for (int f = 0; f < elevator.floors.Count; f++)
			{
				if (elevator.floors[f] == startLayer.z)
				{
					foundIndex = f;
					break;
				}
			}
			if (foundIndex != NoFloorFound)
			{
				var bottomPos = elevator.elevatorPos[foundIndex];
				float distCalc = PathingUtils.GetCostTo(startLayer, bottomPos);
				for (int floorIndex = 0; floorIndex < elevator.floors.Count; floorIndex++) // store the cost the every floor
				{
					if (floorIndex == foundIndex)
						continue;
					if (endLayer.z != elevator.floors[floorIndex])
						continue;

					float cost = elevator.GetCostToFloor(foundIndex, elevator.floors[floorIndex]);
					float gDist = PathingUtils.GetCostTo(elevator.elevatorPos[floorIndex], endLayer);
					ShaftEntry shaft = new ShaftEntry(bottomPos, elevator.elevatorPos[floorIndex], -1, elevatorIndex, distCalc, gDist, cost);
					shaftIndices.Add(shaft);
				}
			}
		}
	}

	void AggregateStairs(IntVector3 startPos, IntVector3 endPos, List<ShaftEntry> shaftIndices)
	{
		for (int shaftIndex = 0; shaftIndex < stairs.Count; shaftIndex++)// all stairs are in there twice
		{
			var pair = stairs[shaftIndex];
			if (pair.start.z == startPos.z)
			{
				pair.distCalc = PathingUtils.GetCostTo(startPos, pair.end);
				float endDistCalc = PathingUtils.GetCostTo(endPos, pair.start);
				ShaftEntry shaft = new ShaftEntry(pair.start, pair.end, shaftIndex, -1, pair.distCalc, endDistCalc, pair.cost);

				shaftIndices.Add(shaft);
			}
			else if (pair.end.z == startPos.z)
			{
				pair.distCalc = PathingUtils.GetCostTo(startPos, pair.start);
				float endDistCalc = PathingUtils.GetCostTo(endPos, pair.end);
				ShaftEntry shaft = new ShaftEntry(pair.end, pair.start, shaftIndex, -1, pair.distCalc, endDistCalc, pair.cost);
				shaftIndices.Add(shaft);
			}
		}
	}

	void MarkShaftVisited(Dictionary<int, bool> visitedStairs, Dictionary<int, bool> visitedElevators, ShaftEntry entry)
	{
		if (entry.IsShaft())
		{
			visitedStairs[entry.shaftIndex] = true;
		}

		else
		{
			visitedElevators[entry.elevatorIndex] = true;
		}
	}

	bool StairsAndElevators_DepthFirstSearch(IntVector3 startPos, IntVector3 endPos, Dictionary<int, bool> visitedStairs, Dictionary<int, bool> visitedElevators, System.Tuple<float, List<ShaftEntry>> nodePath)
	{
        var listOfShafts = nodePath.Item2;
        var allStairsAndElevatorsOnLayer = GetAllStairsAndElevatorsOnLayer(startPos, endPos);
		foreach (var shaftEntry in allStairsAndElevatorsOnLayer)
		{
			if (shaftEntry.IsShaft())
			{
				if (visitedStairs[shaftEntry.shaftIndex])
					continue;
			}
			else
			{
				if (visitedElevators[shaftEntry.elevatorIndex])
					continue;
			}
			if (shaftEntry.startLayer.z == startPos.z)
			{
				float layerPathCost = GetPathCost(startPos, shaftEntry.startLayer) * shaftEntry.costToUse;
				float PathToDestinationCost = GetPathCost(shaftEntry.endLayer, endPos) * shaftEntry.costToUse;
				if (PathToDestinationCost > 0 && PathToDestinationCost < PathImpassible)// can path to end
				{
					if (layerPathCost > 0 && layerPathCost < PathImpassible)
					{
						listOfShafts.Add(shaftEntry);
                        nodePath = new Tuple<float, List<ShaftEntry>>(nodePath.Item1 + PathToDestinationCost, listOfShafts);
						/*nodePath.Item1 += PathToDestinationCost;
						nodePath.Item2.AddLast(shaftEntry);*/
						return true;
					}
				}

				if (layerPathCost > 0 && layerPathCost < PathImpassible)
				{
					MarkShaftVisited(visitedStairs, visitedElevators, shaftEntry);
                   
                    listOfShafts.Add(shaftEntry);
                    nodePath = new Tuple<float, List<ShaftEntry>>(nodePath.Item1 + layerPathCost, listOfShafts);

                  /*  nodePath.Item1 += layerPathCost;
					nodePath.Item2.AddLast(shaftEntry);*/
					if (StairsAndElevators_DepthFirstSearch(shaftEntry.endLayer, endPos, visitedStairs, visitedElevators, nodePath) == true)
						return true;

					// remove unsuccessful node and try next node in list
					/*nodePath.Item1 -= layerPathCost;
					nodePath.Item2.pop_back();*/
					listOfShafts = nodePath.Item2;
					listOfShafts.RemoveAt(listOfShafts.Count - 1);
					nodePath = new Tuple<float, List<ShaftEntry>>(nodePath.Item1 - layerPathCost, listOfShafts);
                }
			}
		}
		return false;
	}

	float SortStairs (int a, int b)
    {
        if (Mathf.Abs(stairs[a].distCalc - stairs[b].distCalc) < 5)// when they are close, choose the cost of the stairs
            return stairs[b].cost - stairs[a].cost;

        return stairs[b].distCalc - stairs[a].distCalc;
    }

    List<int> GetAllStairsOnLayer(IntVector3 startLayer)
	{
		List<int> shaftIndices = new List< int>();
		for (int i = 0; i < stairs.Count; i++)
		{
			var pair = stairs[i];
			if (pair.start.z == startLayer.z)
			{
				pair.distCalc = PathingUtils.GetCostTo(startLayer, pair.end);
				shaftIndices.Add(i);
			}
			else if (pair.end.z == startLayer.z)
			{
				pair.distCalc = PathingUtils.GetCostTo(startLayer, pair.start);
				shaftIndices.Add(i);
			}
		}

        shaftIndices.Sort((x,y) => (int) SortStairs(x, y) );
        // sort this list by the least costly paths first

		/*std::sort(shaftIndices.begin(), shaftIndices.end(), [&](int a, int b)
			{
				if (abs(stairs[a].distCalc - stairs[b].distCalc) < 5)// when they are close, choose the cost of the stairs
					return	stairs[a].cost < stairs[b].cost;

				return stairs[a].distCalc < stairs[b].distCalc;
			});*/
		return shaftIndices;
	}


float GetPathCost(IntVector3 startPos, IntVector3 endPos)
{
	if (startPos.z != endPos.z)
		return 0;

	ClearHistory();
	List<PathNode> openSet = new List<PathNode>();
	List<PathNode> closedSet = new List<PathNode>();
	PathData path = FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);

	if (path.path.Count != 0)
		return path.fcost;

	return PathImpassible;
}

bool Stairs_DepthFirstSearch(IntVector3 startPos, IntVector3 endPos, Dictionary<int, bool> visitedStairs, List<int> nodePath)
{
	var allStairsOnLayer = GetAllStairsOnLayer(startPos);
	foreach (var shaftIndex in allStairsOnLayer)
	{
		if (visitedStairs[shaftIndex])
			continue;

		var shaft = stairs[shaftIndex];

		if (shaft.start.z == startPos.z) // eval the cost of all paths that can connect here
		{
			float layerPathCost = GetPathCost(startPos, shaft.start) * shaft.cost;
			float PathToDestinationCost = GetPathCost(shaft.end, endPos) * shaft.cost;
			if (PathToDestinationCost > 0 && PathToDestinationCost < PathImpassible)
			{
				if (layerPathCost > 0 && layerPathCost < PathImpassible)
				{
					nodePath.Add(shaftIndex);
					return true;
				}
			}

			if (layerPathCost > 0 && layerPathCost < PathImpassible)
			{
				MarkStairVisited(visitedStairs, shaftIndex);
				nodePath.Add(shaftIndex);
				if (Stairs_DepthFirstSearch(shaft.end, endPos, visitedStairs, nodePath) == true)
					return true;

				// remove unsuccessful node and try next node in list
				nodePath.Remove(nodePath.Count-1);
			}
		}
	}
	return false;
}

void MarkStairVisited(Dictionary<int, bool> visited, int index)
{
	int shaftIndex = index % 2 == 1 ? index - 1 : index;
	visited[shaftIndex] = true;
	visited[shaftIndex + 1] = true;
}

public MapTile GetTile(IntVector3 pos)
{
    Debug.Assert(IsValidMapPosition(pos));

    int hash = PathingUtils.CalcHash(pos);
    return tiles[pos.z][hash];
}

public PathNode GetPathNode(IntVector3 pos)
{
	if (!IsValidMapPosition(pos))
		return null;

	int hash = PathingUtils.CalcHash(pos);
	return pathNodes[hash];
}

public List<PathNode> GetNeighbors(IntVector3 root)
{
	var possibleMoveDirs = new List<IntVector3>
	{
		//{-1,-1,0}, {1,-1,0}, {1,1,0}, {-1,1,0},		// diagonals
		new IntVector3(0,-1,0),// cardinal directions
        new IntVector3(1,0,0),
		new IntVector3(0,1,0),
		new IntVector3(-1,0,0)
	};

	List<PathNode> nodes = new List<PathNode>();

	foreach (var dir in possibleMoveDirs)
	{
		var pos = dir + root;
		int hash = PathingUtils.CalcHash(pos);
		if (tiles[pos.z].ContainsKey(hash) == false)
		{
			nodes.Add(pathNodes[hash]);
		}
	}
	return nodes;
}

	float GetNeighborPassabilityCost(PathNode start, PathNode neighbor)
	{
		var startPassability = start.referenceTile.passability; // caching
		var neighborPassability = neighbor.referenceTile.passability;
		if (startPassability == PathingUtils.Passability.clear && neighborPassability == PathingUtils.Passability.clear)
			return 1;
		var startPos = start.pos;
		var neighborPos = neighbor.pos;

		if ((startPassability & PathingUtils.Passability.blocked) != 0)
		{
			if ((startPassability & PathingUtils.Passability.ydir_pos_blocked) != 0 && neighborPos.y > startPos.y)
				return wallImpassible;

			if ((startPassability & PathingUtils.Passability.xdir_pos_blocked) != 0 && neighborPos.x < startPos.x)
				return wallImpassible;

			if ((startPassability & PathingUtils.Passability.ydir_neg_blocked) != 0 && neighborPos.y < startPos.y)
				return wallImpassible;

			if ((startPassability & PathingUtils.Passability.xdir_neg_blocked) != 0 && neighborPos.x > startPos.x)
				return wallImpassible;
		}
		if ((neighborPassability & PathingUtils.Passability.blocked) != 0)
    {
            if ((neighborPassability & PathingUtils.Passability.ydir_pos_blocked) != 0 && neighborPos.y < startPos.y)
			return wallImpassible;

		if ((neighborPassability & PathingUtils.Passability.xdir_pos_blocked) != 0 && neighborPos.x > startPos.x)
			return wallImpassible;

		if ((neighborPassability & PathingUtils.Passability.ydir_neg_blocked) != 0 && neighborPos.y > startPos.y)
			return wallImpassible;

		if ((neighborPassability & PathingUtils.Passability.xdir_neg_blocked) != 0 && neighborPos.x < startPos.x)
			return wallImpassible;
	}

	if ((neighborPassability & PathingUtils.Passability.partly_blocked) != 0)
			return doorObstructed;// doors, no matter the direction, are a partial obstruction

	return 1;
}
}