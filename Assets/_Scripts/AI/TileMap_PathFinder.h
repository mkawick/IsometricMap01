#pragma once

#include "Pathing.h"
#include "LRUCache.h"

struct FRSG_Tile;

namespace TileMapMainInteraction
{
	void CreateMap(RSG_TileMap_PathFinder& tileMap, U8* mapCosts[], int numLayers, const Vector3& dimensions);
	//std::deque<std::pair<Vector3, Vector3>> FindPathWaypoints(TileMap& tileMap, const Vector3& startPos, const Vector3& endPos);

	std::deque<std::pair<FRSG_Tile, FRSG_Tile>> FindPathWaypoints(RSG_TileMap_PathFinder& tileMap, const FRSG_Tile& startPos, const FRSG_Tile& endPos);
}

//----------------------------------------------------------------
//----------------------------------------------------------------

struct ShaftEntry
{
	Vector3 startLayer, endLayer;
	int shaftIndex;
	int elevatorIndex;
	float hCost, gCost;
	float fCost() const {return gCost + hCost;}
	float costToUse;

	bool IsShaft() const { return shaftIndex != -1; }
};

//----------------------------------------------------------------
//----------------------------------------------------------------

struct HistoricalSearches
{
	bool GetMatch(const Vector3& p1, const Vector3& p2, std::deque<std::pair<Vector3, Vector3>>& orderedPoints);
	std::deque<std::pair<Vector3, Vector3>> storedSearch;
	float cost;
};

//----------------------------------------------------------------
//----------------------------------------------------------------

class RSG_TileMap_PathFinder
{
public:
	RSG_TileMap_PathFinder();

	bool AddStairs(const Vector3& p1, const Vector3& p2, float cost = 1.0f);
	bool AddElevator(Elevator& config);

	void SetBlocked(const Vector3& tilePosition, bool blocked);
	void SetCost(const Vector3& tilePosition, float cost);
	void SetPassability(const Vector3& tilePosition, Passability passable);

	//-----------------------------------------------
	bool AddToMap(const Vector3& pos, float cost, bool blocked = false, Passability passable = Passability::clear);
	std::deque<std::pair<Vector3, Vector3>> FindPathWaypointsWithShafts(const Vector3& startPos, const Vector3& endPos);

	std::deque<std::pair<Vector3, Vector3>> FindPathWaypointsWithShaftsCleanedUp(const Vector3& startPos, const Vector3& endPos);
	float FindPathWaypointsCost(const Vector3& startPos, const Vector3& endPos);

	std::deque<std::pair<Vector3, float>> FindClosestLocation(const Vector3& startPos, const std::vector<Vector3>& targetLocations);
	//-----------------------------------------------

	bool ArePositionsValid(const Vector3& startPos, const Vector3& endPos);
	bool IsOnStairsOrElevator(const Vector3& startPos, const Vector3& endPos);
	void WipeMap();
	void ClearStairs();
	void ClearElevators();
	void ClearHistory();
	void CoalesceStairwells();

	void SetDoorObstructedModifier(float obstruction) { doorObstructed = obstruction; }

	bool IsValidMapPosition(const Vector3& pos);
	const std::map<int, std::map<int, MapTile>>& GetTiles() const;
	const std::vector<Stairs>& GetStairs() const { return stairs; }
	PathData FindPathOnSingleLayer(const Vector3& startPos, const Vector3& endPos, std::vector<PathNode*>& openSet, std::vector<PathNode*>& closedSet);
	std::deque<std::pair<Vector3, Vector3>> FindPathWaypoints2D(const Vector3& startPos, const Vector3& endPos);


	void SetMaxExtents(const Vector3& min, const Vector3& max);
	Vector3 GetMinExtents() const { return minExtent; }
	Vector3 GetMaxExtents() const { return maxExtent; }
	void PrintMap();

private:
	void UpdateExtents(const Vector3& pos);
	std::vector<int> GetAllStairsOnLayer(const Vector3& startLayer);
	bool Stairs_DepthFirstSearch(const Vector3& startPos, const Vector3& endPos, std::map<int, bool>& visited, std::vector<int>& shaftNodes);
	void MarkStairVisited(std::map<int, bool>& visited, int index);

	MapTile* GetTile(const Vector3& pos);// tried to make it const, but the map would not allow it
	PathNode* GetPathNode(const Vector3& pos);
	std::vector<PathNode*> GetNeighbors(const Vector3& root);
	float GetNeighborPassabilityCost(const PathNode* start, const PathNode* neighbor);

	//----------------------------------
	std::vector<ShaftEntry> GetAllStairsAndElevatorsOnLayer(const Vector3& startPos, const Vector3& endPos);
	void AggregateStairs(const Vector3& startPos, const Vector3& endPos, std::vector<ShaftEntry>& shaftIndices);
	void AggregateElevators(const Vector3& startLayer, const Vector3& endPos, std::vector<ShaftEntry>& shaftIndices);
	bool StairsAndElevators_DepthFirstSearch(const Vector3& startPos, const Vector3& endPos, std::map<int, bool>& visitedStairs, std::map<int, bool>& visitedElevators, std::pair<float, std::vector<ShaftEntry>>& nodePath);
	void MarkShaftVisited(std::map<int, bool>& visitedStairs, std::map<int, bool>& visitedElevators, const ShaftEntry& entry) const;
	void ReorderStairEndpoints();
	std::vector<int> GrabAllStairsSharingSameXY(int startingIndex) const;
	bool AttemptAssembleSingleLevelPath(const Vector3& startPos, const Vector3& endPos, std::deque<std::pair<Vector3, Vector3>>& outputPath, float& PathCost);

	bool GetCachedResult(Vector3 startPos, Vector3 endPos, std::deque<std::pair<Vector3, Vector3>>& outputPath, float& cost);
	//----------------------------------

	std::map<int, std::map<int, MapTile>> tiles; // sorted by z
	std::map<int, PathNode> pathNodes;
	std::vector<Stairs> stairs;
	std::vector<Elevator> elevators;
	Vector3 minExtent, maxExtent;
	LRUCache<int, HistoricalSearches> lruHistoricalSearches;

	float GetPathCost(const Vector3& start, const Vector3& end);

	const float PathImpassible = 1E9;
	float doorObstructed;
	float wallImpassible = 100;
};
