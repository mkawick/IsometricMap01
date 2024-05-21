#pragma once

#include <algorithm>
#include <cassert>
#include <chrono>
#include <deque>
#include <functional>
#include <iomanip>
#include <iostream>
#include <limits>
#include <map>
#include <queue>
#include <string>
#include <unordered_set>
#include <vector>

#include "PathingUtils.h"

class RSG_TileMap_PathFinder;
struct MapTile;

struct PathNode
{
	Vector3 pos;
	const MapTile* referenceTile;
	PathNode* cameFrom;
	bool walkable;

	int gCost, hCost;

	PathNode() : referenceTile(nullptr), cameFrom(nullptr), walkable(true), gCost((int)1e6), hCost((int)1e6) {
		pos.x = 0; 
		pos.y = 0; 
		pos.z = 0;
	}
	PathNode(const Vector3& _pos) : PathNode()
	{
		pos = _pos;
	}
	PathNode(const MapTile* mapTile) : pos(mapTile->pos), referenceTile(mapTile), cameFrom(nullptr), walkable(!mapTile->blocked), gCost((int)1e6), hCost((int)1e6)
	{}

	bool operator == (const PathNode& node) const
	{
		return node.pos == pos && node.referenceTile == referenceTile;
	}

	int fCost()
	{
		return gCost + hCost;
	}

	void ClearPath()
	{
		cameFrom = nullptr;
		gCost = (int) 1e6;
		hCost = (int) 1e6;
	}
};

struct PathData
{
	std::vector<PathNode*> path;
	float fcost, gcost, hcost;

	PathData() :fcost(0), gcost(0), hcost(0) {}
	PathData(PathNode* lastNodeInPath) : PathData() 
	{
		CleanUpPath(lastNodeInPath);
	}

	void CleanUpPath(PathNode* end)
	{
		PathNode* current = end;

		while (current != nullptr)
		{
			hcost += current->hCost;
			gcost += current->gCost;
			fcost += current->fCost();

			path.push_back(current);
			current = current->cameFrom;
		}

		for (auto first = path.begin(), last = path.end() - 1; first < last; first++, last--)
		{
			auto node = *first;
			*first = *last;
			*last = node;
		}
	}
};

std::vector<PathNode*> FindPath(const Vector3& startPos, const Vector3& endPos, RSG_TileMap_PathFinder& originalMap);

class Stairs
{
public:
	Vector3 start, end;
	float cost;
	float distCalc;

	bool Contains(const Vector3& p1, const Vector3& p2) const
	{
		if((start == p1) || (start == p2) || (end == p1) || (end == p2))
			return true;
		return false;
	}
};

class Shaft
{
public:
	Vector3 start, end;
	float cost;
	float distCalc;
};

class Elevator
{
public:
	std::vector<int> floors;
	std::vector<Vector3> elevatorPos;

	float costMovingAway;
	float costMovingToward;

	int currentDirection;
	int currentFloor, nextFloor;
	bool isMoving;

	std::vector<float> costPerFloorDistanceConfig;

	float GetCostToFloor(int from, int to) const;
	enum MovingDir { stationary, up, down };

	bool Contains(const Vector3& p1, const Vector3& p2) const
	{
		for (auto& pos : elevatorPos)
		{
			if ((pos == p1) || (pos == p2))
				return true;
		}
		return false;
	}
};