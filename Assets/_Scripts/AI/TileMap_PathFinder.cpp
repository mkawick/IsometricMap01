#include "RSG_TileMap_PathFinder.h"
#include "RSG_PathingUtils.h"
#include <iostream>

PRAGMA_DISABLE_OPTIMIZATION
namespace TileMapMainInteraction
{
	void CreateMap(RSG_TileMap_PathFinder& tileMap, U8* mapCosts[], int numLayers, const Vector3& dimensions)
	{
		int bigInt = 100000;
		int smallInt = -bigInt;
		Vector3 minExtent = { bigInt , bigInt, bigInt }, maxExtent = { smallInt , smallInt , smallInt };
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
					float cost = mapCosts[z][y * dimensions.x + x];
					tileMap.AddToMap(Vector3(x, y, 0), cost, cost == 255, Passability::clear);
				}
			}
		}
	}
	std::deque<std::pair<Vector3, Vector3>> FindPathWaypoints(RSG_TileMap_PathFinder& tileMap, const Vector3& startPos, const Vector3& endPos)
	{
		return tileMap.FindPathWaypointsWithShafts(startPos, endPos);
	}
}

RSG_TileMap_PathFinder::RSG_TileMap_PathFinder() : lruHistoricalSearches(12), 
	doorObstructed(4)	
{
}

bool RSG_TileMap_PathFinder::ArePositionsValid(const Vector3& startPos, const Vector3& endPos)
{
	if(tiles.find(startPos.z) == tiles.end() || 
		(tiles[startPos.z].find(startPos.Hash()) == tiles[startPos.z].end()))
		{
			return false;
		}
	if (tiles.find(endPos.z) == tiles.end() || 
		(tiles[endPos.z].find(endPos.Hash()) == tiles[endPos.z].end()))
		{
			return false;
		}
	return true;
}


bool RSG_TileMap_PathFinder::IsOnStairsOrElevator(const Vector3& startPos, const Vector3& endPos)
{
	if(startPos.x != endPos.x || startPos.y != endPos.y)
		return false;

	for (int i=0; i<stairs.size()-1; i++)
	{
		auto const& stair = stairs[i];
		if (stair.Contains(startPos, endPos))
		{
			for (int j = i + 1; j < stairs.size(); j++)
			{
				auto const& stairEnd = stairs[j];
				if (stairEnd.Contains(startPos, endPos))
				{
					return true;
				}
			}
			//return false;
		}
	}
	for (auto& elevator : elevators)
	{
		if(elevator.Contains(startPos, endPos))
		{
			return true;
		}
	}

	return false;
}

void RSG_TileMap_PathFinder::WipeMap()
{
	tiles.clear();
}

void RSG_TileMap_PathFinder::ClearHistory()
{
	for (auto& node : pathNodes)
	{
		node.second.ClearPath();
	}
}

//UE_DISABLE_OPTIMIZATION
void RSG_TileMap_PathFinder::ReorderStairEndpoints()
{
	for (auto& stair : stairs)
	{
		if (stair.start.z > stair.end.z)
		{
			auto end = stair.end;
			stair.end = stair.start;
			stair.start = end;
		}
	}
}

std::vector<int> RSG_TileMap_PathFinder::GrabAllStairsSharingSameXY(int startingIndex) const
{
	std::vector<int> contiguousIndices;
	contiguousIndices.push_back(startingIndex);
	auto base = stairs[startingIndex];
	for (int i = startingIndex + 1; i < stairs.size(); i++)
	{
		if (stairs[i].start.x == base.start.x &&
			stairs[i].start.y == base.start.y)
		{
			int baseZ = base.start.z;
			int numFloorsBetween = stairs[i].start.z - baseZ;
			if (numFloorsBetween > 1)
			{
				return contiguousIndices;
			}
			contiguousIndices.push_back(i);
			base = stairs[i];// advance to next floor
		}
	}

	return contiguousIndices;
}

void RSG_TileMap_PathFinder::CoalesceStairwells()
{
	ReorderStairEndpoints();
	std::sort(stairs.begin(), stairs.end(), [](const Stairs& a, const Stairs& b)
		{
			return a.start.z < b.start.z;
		});

	auto hasNeighborOnSameLevel = [](const Vector3& pos, std::map<int, std::map<int, MapTile>>& tiles)
		{
			int level = pos.z;
			if (tiles.find(level) == tiles.end() ||
				(tiles[level].size() == 1))// no tiles or only the stairs
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
		};

	// do not, in fact, coalesce the stairs - as per Doru - Mickey
	/*for (int i = 0; i < stairs.size(); i++)
	{
		auto stair = stairs[i];
		bool hasNeighborOnHigherLevel = hasNeighborOnSameLevel(stair.end, tiles);
		bool hasNeighborSameLevel = hasNeighborOnSameLevel(stair.start, tiles);
		if (!hasNeighborOnHigherLevel && hasNeighborSameLevel)
		{
			auto listOfStairIndices = GrabAllStairsSharingSameXY(i); // ordered
			if (listOfStairIndices.size() > 1)
			{
				int finalIndex = listOfStairIndices.size() - 1;
				stairs[i].end = stairs[listOfStairIndices[finalIndex]].end;
				for (int j = finalIndex; j > 0; j--)// careful on the indices
				{
					stairs.erase(stairs.begin() + listOfStairIndices[j]);
				}
			}
		}
	}*/
}

bool RSG_TileMap_PathFinder::AddToMap(const Vector3& pos, float cost, bool blocked, Passability passable)
{
	bool alreadyExists = false;
	MapTile tile{ pos, cost, (U8)blocked, passable };
	auto hash = pos.Hash();
	tiles[pos.z][hash] = tile;

	PathNode pathNode(&tiles[pos.z][hash]);
	if (tiles[pos.z].find(pos.Hash()) != tiles[pos.z].end())
	{
		alreadyExists = true;
		pathNodes[hash] = pathNode;
	}
	else
	{
		pathNodes.insert(std::make_pair<>(hash, pathNode));
	}

	UpdateExtents(pos);

	if (alreadyExists)
		return false;
	return true;
}

void RSG_TileMap_PathFinder::ClearStairs()
{
	stairs.clear();
}

void RSG_TileMap_PathFinder::ClearElevators()
{
	elevators.clear();
}

bool RSG_TileMap_PathFinder::AddStairs(const Vector3& p1, const Vector3& p2, float cost)
{
	assert(p1.z != p2.z);
	assert(cost > 0);

	// verify no duplicates
	for (int i = 0; i < stairs.size(); i++)
	{
		auto& pair = stairs.at(i);
		if ((pair.start == p1 || pair.end == p1) &&
			(pair.start == p2 || pair.end == p2))
		{
			return false;
		}
	}

	int newIndex = stairs.size();
	stairs.push_back({ p1, p2, cost, 0 });
	return true;
}

bool RSG_TileMap_PathFinder::AddElevator(Elevator& config)
{
	assert(config.costMovingAway > 0);
	assert(config.costMovingToward > 0);
	assert(config.floors.size() > 0);
	assert(config.elevatorPos.size() > 0);
	assert(config.costPerFloorDistanceConfig.size() > 0);

	elevators.push_back(config);
	return true;
}

void RSG_TileMap_PathFinder::SetBlocked(const Vector3& tilePosition, bool blocked)
{
	auto hash = tilePosition.Hash();
	auto tile = tiles[tilePosition.z].find(hash);
	if (tile == tiles[tilePosition.z].end())
		return;

	(*tile).second.blocked = blocked;
}

void RSG_TileMap_PathFinder::SetCost(const Vector3& tilePosition, float cost)
{
	auto hash = tilePosition.Hash();
	auto tile = tiles[tilePosition.z].find(hash);
	if (tile == tiles[tilePosition.z].end())
		return;

	(*tile).second.cost = cost;
}

void RSG_TileMap_PathFinder::SetPassability(const Vector3& tilePosition, Passability passability)
{
	auto hash = tilePosition.Hash();
	auto tile = tiles[tilePosition.z].find(hash);
	if (tile == tiles[tilePosition.z].end())
		return;

	(*tile).second.passability = passability;
}

bool RSG_TileMap_PathFinder::IsValidMapPosition(const Vector3& pos)
{
	return tiles[pos.z].find(pos.Hash()) != tiles[pos.z].end();
}

void RSG_TileMap_PathFinder::PrintMap()
{
	for (int z = minExtent.z; z < maxExtent.z; z++)
	{
		std::cout << "     ";
		for (int y = minExtent.y; y < maxExtent.y; y++)
		{
			std::cout << std::setw(2) << std::to_string(y) + "  ";
		}
		std::cout << std::endl;
		for (int y = minExtent.y; y < maxExtent.y; y++)
		{
			std::string costs, passables;

			for (int x = minExtent.x; x < maxExtent.x; x++)
			{
				auto pos = Vector3(x, y, z);
				if (IsValidMapPosition(pos))
				{
					auto tile = GetTile(pos);
					char printableCost = ((char)tile->cost + '0');
					if (tile->blocked)
						printableCost = '.';

					costs += printableCost;
					costs += "  ";
					char printablePassable = ((char)tile->passability);
					passables += printablePassable;
					passables += " ";
				}
				else
				{
					costs += "  ";
					passables += "  ";
				}
			}

			std::cout << std::setw(2) << y << "   " << costs << " --- " << passables << std::endl;
		}
		std::cout << std::endl;
	}
}

const std::map<int, std::map<int, MapTile>>& RSG_TileMap_PathFinder::GetTiles() const
{
	return tiles;
}

bool IsInSet(const std::vector<PathNode*>& closedSet, PathNode* needle)
{
	for (const auto node : closedSet)
	{
		if (node->pos == needle->pos && node->referenceTile == needle->referenceTile)
			return true;
	}
	return false;
}

PathData RSG_TileMap_PathFinder::FindPathOnSingleLayer(const Vector3& startPos, const Vector3& endPos, std::vector<PathNode*>& openSet, std::vector<PathNode*>& closedSet)
{
	auto begin = GetPathNode(startPos);
	auto end = GetPathNode(endPos);
	if (begin == nullptr || end == nullptr)
	{
		return PathData();
	}
	openSet.push_back(begin);
	while (openSet.size() > 0)
	{
		int selectedIndex = 0;
		auto currentNode = openSet[0]; // search me baby

		for (int i = 1; i < (int)openSet.size(); i++)
		{
			auto& testNode = openSet[i];
			if (testNode->fCost() < currentNode->fCost() || (testNode->fCost() == currentNode->fCost() && testNode->hCost < currentNode->hCost))
			{
				currentNode = testNode;
				selectedIndex = i;
			}
		}

		closedSet.push_back(currentNode);
		openSet.erase(openSet.begin() + selectedIndex);

		if (currentNode->pos == endPos)
		{
			return PathData(currentNode);
		}
		auto neighborList = GetNeighbors(currentNode->pos);

		for (auto neighbor : neighborList)
		{
			if (neighbor->referenceTile->blocked || IsInSet(closedSet, neighbor))
				continue;
			float wallCost = GetNeighborPassabilityCost(currentNode, neighbor);
			if (wallCost == wallImpassible)
				continue;

			int tileCost = currentNode->referenceTile->cost * wallCost;
			int currentGCost = currentNode->gCost;
			auto gCost = tileCost * (currentGCost + GetCostTo(currentNode->pos, neighbor->pos));
			if (gCost < neighbor->gCost || !IsInSet(openSet, neighbor))
			{
				neighbor->cameFrom = currentNode;
				neighbor->gCost = (int)gCost;
				neighbor->hCost = (int)(tileCost * GetCostTo(neighbor->pos, endPos));

				if (!IsInSet(openSet, neighbor))
				{
					openSet.push_back(neighbor);
				}
			}
		}
	}

	return PathData();
}

std::deque<std::pair<Vector3, Vector3>> RSG_TileMap_PathFinder::FindPathWaypoints2D(const Vector3& startPos, const Vector3& endPos)
{
	ClearHistory();
	std::map<int, bool> visited;
	std::vector<int> shaftNodes;
	auto outPath = Stairs_DepthFirstSearch(startPos, endPos, visited, shaftNodes);
	if (outPath == false)
		return std::deque <std::pair<Vector3, Vector3>>();


	std::deque < std::pair<Vector3, Vector3>> output;
	// start node to first shaft
	output.push_back(std::make_pair<>(startPos, stairs[shaftNodes[0]].start));
	if (shaftNodes.size() > 1)
	{
		std::cout << shaftNodes.size() << std::endl;
		// always ignore the first node
		for (int i = 0; i < shaftNodes.size() - 1; i++)
		{
			int currentShaftIndex = shaftNodes[i];
			int nextShaftIndex = shaftNodes[i + 1];
			output.push_back(std::make_pair<>(stairs[currentShaftIndex].end, stairs[nextShaftIndex].start));
		}
	}
	// last shaft to the end
	output.push_back(std::make_pair<>(stairs[*(shaftNodes.end() - 1)].end, endPos));
	return output;
}

bool RSG_TileMap_PathFinder::AttemptAssembleSingleLevelPath(const Vector3& startPos, const Vector3& endPos, std::deque<std::pair<Vector3, Vector3>>& outputPath, float& PathCost)
{
	std::vector<PathNode*> openSet;
	std::vector<PathNode*> closedSet;
	PathCost = std::numeric_limits<float>::max();
	auto outPath = FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);
	if (outPath.path.size())
	{
		PathCost = outPath.fcost;
		outputPath.push_back(std::make_pair<>(startPos, endPos));
		return true;
	}
	return false;
}

bool RSG_TileMap_PathFinder::GetCachedResult(Vector3 startPos, Vector3 endPos, std::deque<std::pair<Vector3, Vector3>>& outputPath, float& cost)
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

std::deque<std::pair<Vector3, Vector3>> RSG_TileMap_PathFinder::FindPathWaypointsWithShafts(const Vector3& startPos, const Vector3& endPos)
{
	ClearHistory();
	if (endPos.z == startPos.z)// same horizon... likely a straight path,this code really isn't needed much anymore
	{
		float Cost;
		std::deque < std::pair<Vector3, Vector3>> outputPath;
		if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, Cost))
		{
			return outputPath;
		}
	}

	std::map<int, bool> visitedStairs;
	std::map<int, bool> visitedElevators;
	std::pair<float, std::vector<ShaftEntry >> nodePath; nodePath.first = 0;

	bool outPath = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
	if (outPath == false)
		return std::deque <std::pair<Vector3, Vector3>>();


	std::deque < std::pair<Vector3, Vector3>> output;
	// start node to first shaft
	output.push_back(std::make_pair<>(startPos, nodePath.second[0].startLayer));
	if (nodePath.second.size() > 1)
	{
		for (int i = 0; i < nodePath.second.size() - 1; i++)
		{
			auto& shaft = nodePath.second[i];
			output.push_back(std::make_pair<>(shaft.endLayer, nodePath.second[i + 1].startLayer));
		}
	}
	// last shaft to the end
	output.push_back(std::make_pair<>(nodePath.second[nodePath.second.size() - 1].endLayer, endPos));
	return output;
}


void RemoveExtraShafts(std::vector<ShaftEntry>& nodePath)
{
	if (nodePath.size() == 2)
	{
		if (nodePath[1].startLayer == nodePath[0].endLayer)
		{
			// we cam always path one layer
			nodePath.erase(nodePath.begin());
		}
	}
	else 
	{
		// search for stairs that are 1 level apart and pick the first one only removing others
		for (int i = 0; i < nodePath.size() - 1; i++)// the -1 is important
		{
			auto& node = nodePath[i];
			if (node.IsShaft() == false)
				continue;
			// we do not need to advance the index, since we remove the current node
			// also, never remove the last node
			for (int j = i + 1; j < nodePath.size()-1; )
			{
				auto const& afternode = nodePath[j];
				if (!afternode.IsShaft())
					break;
				if (afternode.startLayer == node.endLayer)
				{
					node.endLayer = afternode.endLayer;
					nodePath.erase(nodePath.begin() + j);
					
				}
				else
				{
					break;
				}
			}
		}
	}
	// we can always remove the last node
	auto const& node = nodePath[nodePath.size() - 1];
	if (node.IsShaft())
	{
		nodePath.erase(nodePath.begin() + (nodePath.size() - 1));
	}
}


std::deque<std::pair<Vector3, Vector3>> RSG_TileMap_PathFinder::FindPathWaypointsWithShaftsCleanedUp(const Vector3& startPos, const Vector3& endPos)
{
	std::deque < std::pair<Vector3, Vector3>> outputPath;
	float pathCost = 0;

	if (GetCachedResult(startPos, endPos, outputPath, pathCost))
	{
		return outputPath;
	}
	
	ClearHistory();

	if (endPos.z == startPos.z)// same horizon... likely a straight path, this code really isn't needed much anymore
	{
		float Cost;
		if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, Cost))
		{
			return outputPath;
		}
	}
	if (IsOnStairsOrElevator(startPos, endPos))
	{
		outputPath.push_back(std::make_pair<>(startPos, endPos));
		return outputPath;
	}

	std::map<int, bool> visitedStairs;
	std::map<int, bool> visitedElevators;
	std::pair<float, std::vector<ShaftEntry >> nodePath; 
	nodePath.first = 0;

	bool didWeFindAPathToDest = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
	if (didWeFindAPathToDest == false)
		return outputPath;

	RemoveExtraShafts(nodePath.second);

	if (nodePath.second.size() == 0) // can be true for VERY close nodes
	{
		outputPath.push_back(std::make_pair<>(startPos, endPos));
		return outputPath;
	}

	// start node to first shaft
	outputPath.push_back(std::make_pair<>(startPos, nodePath.second[0].startLayer));
	if (nodePath.second.size() > 1)
	{
		for (int i = 0; i < nodePath.second.size() - 1; i++)
		{
			auto& shaft = nodePath.second[i];
			outputPath.push_back(std::make_pair<>(shaft.endLayer, nodePath.second[i + 1].startLayer));
		}
	}
	// last shaft to the end
	outputPath.push_back(std::make_pair<>(nodePath.second[nodePath.second.size() - 1].endLayer, endPos));
	return outputPath;
}

float RSG_TileMap_PathFinder::FindPathWaypointsCost(const Vector3& startPos, const Vector3& endPos)
{
	ClearHistory();

	std::deque < std::pair<Vector3, Vector3>> outputPath;
	if (endPos.z == startPos.z)// same horizon... likely a straight path, this code really isn't needed much anymore
	{
		float Cost;
		if (AttemptAssembleSingleLevelPath(startPos, endPos, outputPath, Cost))
		{
			return Cost;
		}
	}
	if (IsOnStairsOrElevator(startPos, endPos))
	{
		outputPath.push_back(std::make_pair<>(startPos, endPos));
		return std::numeric_limits<float>::max();
	}

	std::map<int, bool> visitedStairs;
	std::map<int, bool> visitedElevators;
	std::pair<float, std::vector<ShaftEntry >> nodePath;
	nodePath.first = 0;

	bool didWeFindAPathToDest = StairsAndElevators_DepthFirstSearch(startPos, endPos, visitedStairs, visitedElevators, nodePath);
	if (didWeFindAPathToDest == false)
		return std::numeric_limits<float>::max();

	return nodePath.first;
}

std::deque<std::pair<Vector3, float>> RSG_TileMap_PathFinder::FindClosestLocation(const Vector3& startPos, const std::vector<Vector3>& targetLocations)
{
	std::deque <std::pair<Vector3, float>> prioritizedNodes;
	for (auto target : targetLocations)
	{
		std::map<int, bool> visitedStairs;
		std::map<int, bool> visitedElevators;
		std::pair<float, std::vector<ShaftEntry>> nodePath; 
		nodePath.first = 0;

		bool outPath = StairsAndElevators_DepthFirstSearch(startPos, target, visitedStairs, visitedElevators, nodePath);
		if (outPath)
		{
			prioritizedNodes.push_back(std::pair<Vector3, float>( target, nodePath.first ));
		}
	}

	std::sort(prioritizedNodes.begin(), prioritizedNodes.end(), [&](const std::pair<Vector3, float>& a, const std::pair<Vector3, float>& b)
		{
			return a.second < b.second;
		});
	return prioritizedNodes;
}

void RSG_TileMap_PathFinder::SetMaxExtents(const Vector3& min, const Vector3& max)
{
	minExtent = min;
	maxExtent = max;
}

void RSG_TileMap_PathFinder::UpdateExtents(const Vector3& pos)
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

std::vector<ShaftEntry> RSG_TileMap_PathFinder::GetAllStairsAndElevatorsOnLayer(const Vector3& startPos, const Vector3& endPos)
{
	std::vector<ShaftEntry> shaftIndices;
	AggregateStairs(startPos, endPos, shaftIndices);
	AggregateElevators(startPos, endPos, shaftIndices);

	// sort this list by the least costly paths first
	std::sort(shaftIndices.begin(), shaftIndices.end(), [&](const ShaftEntry& a, const ShaftEntry& b)
		{
			/*if (a.endLayer.z == endLayer)
				return true;
			if (b.endLayer.z == endLayer)
				return false;*/

			// give preference to those paths whose end points are on a nearer floor
			/*auto floorDiffA = abs(endLayer - a.endLayer.z);
			auto floorDiffB = abs(endLayer - b.endLayer.z);
			if (floorDiffA != floorDiffB)
				return floorDiffA < floorDiffB;*/
			
			/*int kMinDistThresholdToPreferDesignCost = 5;
			if (abs(a.fCost() - b.fCost()) < kMinDistThresholdToPreferDesignCost)// when they are close, choose the cost of the stairs
				return	a.costToUse < b.costToUse;*/
			// lastly, try closer shafts first
			return a.fCost() < b.fCost();
		});
	return shaftIndices;
}

void RSG_TileMap_PathFinder::AggregateElevators(const Vector3& startLayer, const Vector3& endLayer, std::vector<ShaftEntry>& shaftIndices)
{
	for (int elevatorIndex = 0; elevatorIndex < elevators.size(); elevatorIndex++)
	{
		auto& elevator = elevators.at(elevatorIndex);
		const int NoFloorFound = -10000;
		int foundIndex = NoFloorFound;
		for (int f = 0; f < elevator.floors.size(); f++)
		{
			if (elevator.floors[f] == startLayer.z)
			{
				foundIndex = f;
				break;
			}
		}
		if (foundIndex != NoFloorFound)
		{
			auto& bottomPos = elevator.elevatorPos[foundIndex];
			float distCalc = GetCostTo(startLayer, bottomPos);
			for (int floorIndex = 0; floorIndex < elevator.floors.size(); floorIndex++) // store the cost the every floor
			{
				if (floorIndex == foundIndex)
					continue;
				if (endLayer.z != elevator.floors[floorIndex])
					continue;

				float cost = elevator.GetCostToFloor(foundIndex, elevator.floors[floorIndex]);
				float gDist = GetCostTo(elevator.elevatorPos[floorIndex], endLayer);
				ShaftEntry shaft{ bottomPos, elevator.elevatorPos[floorIndex], -1, elevatorIndex, distCalc, gDist, cost };
				shaftIndices.push_back(shaft);
			}
		}
	}
}

void RSG_TileMap_PathFinder::AggregateStairs(const Vector3& startPos, const Vector3& endPos, std::vector<ShaftEntry>& shaftIndices)
{
	for (int shaftIndex = 0; shaftIndex < stairs.size(); shaftIndex++)// all stairs are in there twice
	{
		auto& pair = stairs.at(shaftIndex);
		if (pair.start.z == startPos.z)
		{
			pair.distCalc = GetCostTo(startPos, pair.end);
			float endDistCalc = GetCostTo(endPos, pair.start);
			ShaftEntry shaft{ pair.start, pair.end, shaftIndex, -1, pair.distCalc, endDistCalc, pair.cost };

			shaftIndices.push_back(shaft);
		}
		else if (pair.end.z == startPos.z)
		{
			pair.distCalc = GetCostTo(startPos, pair.start);
			float endDistCalc = GetCostTo(endPos, pair.end);
			ShaftEntry shaft{ pair.end, pair.start, shaftIndex, -1, pair.distCalc, endDistCalc, pair.cost };
			shaftIndices.push_back(shaft);
		}
	}
}

void RSG_TileMap_PathFinder::MarkShaftVisited(std::map<int, bool>& visitedStairs, std::map<int, bool>& visitedElevators, const ShaftEntry& entry) const
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

bool RSG_TileMap_PathFinder::StairsAndElevators_DepthFirstSearch(const Vector3& startPos, const Vector3& endPos, std::map<int, bool>& visitedStairs, std::map<int, bool>& visitedElevators, std::pair<float, std::vector<ShaftEntry>>& nodePath)
{
	auto allStairsAndElevatorsOnLayer = GetAllStairsAndElevatorsOnLayer(startPos, endPos);
	for (auto& shaftEntry : allStairsAndElevatorsOnLayer)
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
					nodePath.first += PathToDestinationCost;
					nodePath.second.push_back(shaftEntry);
					return true;
				}
			}

			if (layerPathCost > 0 && layerPathCost < PathImpassible)
			{
				MarkShaftVisited(visitedStairs, visitedElevators, shaftEntry);
				nodePath.first += layerPathCost;
				nodePath.second.push_back(shaftEntry);
				if (StairsAndElevators_DepthFirstSearch(shaftEntry.endLayer, endPos, visitedStairs, visitedElevators, nodePath) == true)
					return true;

				// remove unsuccessful node and try next node in list
				nodePath.first -= layerPathCost;
				nodePath.second.pop_back();
			}
		}
	}
	return false;
}

std::vector<int> RSG_TileMap_PathFinder::GetAllStairsOnLayer(const Vector3& startLayer)
{
	std::vector<int> shaftIndices;
	for (int i = 0; i < stairs.size(); i++)
	{
		auto& pair = stairs.at(i);
		if (pair.start.z == startLayer.z)
		{
			pair.distCalc = GetCostTo(startLayer, pair.end);
			shaftIndices.push_back(i);
		}
		else if (pair.end.z == startLayer.z)
		{
			pair.distCalc = GetCostTo(startLayer, pair.start);
			shaftIndices.push_back(i);
		}
	}

	// sort this list by the least costly paths first
	std::sort(shaftIndices.begin(), shaftIndices.end(), [&](int a, int b)
		{
			if (abs(stairs[a].distCalc - stairs[b].distCalc) < 5)// when they are close, choose the cost of the stairs
				return	stairs[a].cost < stairs[b].cost;

			return stairs[a].distCalc < stairs[b].distCalc;
		});
	return shaftIndices;
}


float RSG_TileMap_PathFinder::GetPathCost(const Vector3& startPos, const Vector3& endPos)
{
	if (startPos.z != endPos.z)
		return false;

	ClearHistory();
	std::vector<PathNode*> openSet;
	std::vector<PathNode*> closedSet;
	PathData path = FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);

	if (path.path.size())
		return path.fcost;

	return PathImpassible;
}

bool RSG_TileMap_PathFinder::Stairs_DepthFirstSearch(const Vector3& startPos, const Vector3& endPos, std::map<int, bool>& visitedStairs, std::vector<int>& nodePath)
{
	auto allStairsOnLayer = GetAllStairsOnLayer(startPos);
	for (auto shaftIndex : allStairsOnLayer)
	{
		if (visitedStairs[shaftIndex])
			continue;

		auto& shaft = stairs[shaftIndex];

		if (shaft.start.z == startPos.z) // eval the cost of all paths that can connect here
		{
			float layerPathCost = GetPathCost(startPos, shaft.start) * shaft.cost;
			float PathToDestinationCost = GetPathCost(shaft.end, endPos) * shaft.cost;
			if (PathToDestinationCost > 0 && PathToDestinationCost < PathImpassible)
			{
				if (layerPathCost > 0 && layerPathCost < PathImpassible)
				{
					nodePath.push_back(shaftIndex);
					return true;
				}
			}

			if (layerPathCost > 0 && layerPathCost < PathImpassible)
			{
				MarkStairVisited(visitedStairs, shaftIndex);
				nodePath.push_back(shaftIndex);
				if (Stairs_DepthFirstSearch(shaft.end, endPos, visitedStairs, nodePath) == true)
					return true;

				// remove unsuccessful node and try next node in list
				nodePath.pop_back();
			}
		}
	}
	return false;
}

//UE_ENABLE_OPTIMIZATION
void RSG_TileMap_PathFinder::MarkStairVisited(std::map<int, bool>& visited, int index)
{
	int shaftIndex = index % 2 == 1 ? index - 1 : index;
	visited[shaftIndex] = true;
	visited[shaftIndex + 1] = true;
}

MapTile* RSG_TileMap_PathFinder::GetTile(const Vector3& pos)
{
	if (!IsValidMapPosition(pos))
		return nullptr;

	int hash = pos.Hash();
	return &tiles[pos.z][hash];
}

PathNode* RSG_TileMap_PathFinder::GetPathNode(const Vector3& pos)
{
	if (!IsValidMapPosition(pos))
		return nullptr;

	int hash = pos.Hash();
	return &pathNodes[hash];
}

std::vector<PathNode*> RSG_TileMap_PathFinder::GetNeighbors(const Vector3& root)
{
	static std::vector<Vector3> possibleMoveDirs =
	{
		//{-1,-1,0}, {1,-1,0}, {1,1,0}, {-1,1,0},		// diagonals
		{0,-1,0}, {1,0,0}, {0,1,0}, {-1,0,0}		// cardinal directions
	};

	std::vector<PathNode*> nodes;

	for (auto& dir : possibleMoveDirs)
	{
		auto pos = dir + root;
		int hash = pos.Hash();
		if (tiles[pos.z].find(hash) != tiles[pos.z].end())
		{
			nodes.push_back(&pathNodes[hash]);
		}
	}
	return nodes;
}

float RSG_TileMap_PathFinder::GetNeighborPassabilityCost(const PathNode* start, const PathNode* neighbor)
{
	auto startPassability = start->referenceTile->passability; // caching
	auto neighborPassability = neighbor->referenceTile->passability;
	if (startPassability == Passability::clear && neighborPassability == Passability::clear)
		return 1;
	auto& startPos = start->pos;
	auto& neighborPos = neighbor->pos;

	if (startPassability & Passability::blocked)
	{
		if ((startPassability & Passability::ydir_pos_blocked) && neighborPos.y > startPos.y)
			return wallImpassible;

		if ((startPassability & Passability::xdir_pos_blocked) && neighborPos.x < startPos.x)
			return wallImpassible;

		if ((startPassability & Passability::ydir_neg_blocked) && neighborPos.y < startPos.y)
			return wallImpassible;

		if ((startPassability & Passability::xdir_neg_blocked) && neighborPos.x > startPos.x)
			return wallImpassible;
	}
	if (neighborPassability & Passability::blocked)
	{
		if ((neighborPassability & Passability::ydir_pos_blocked) && neighborPos.y < startPos.y)
			return wallImpassible;

		if ((neighborPassability & Passability::xdir_pos_blocked) && neighborPos.x > startPos.x)
			return wallImpassible;

		if ((neighborPassability & Passability::ydir_neg_blocked) && neighborPos.y > startPos.y)
			return wallImpassible;

		if ((neighborPassability & Passability::xdir_neg_blocked) && neighborPos.x < startPos.x)
			return wallImpassible;
	}

	if (neighborPassability & Passability::partly_blocked)
		return doorObstructed;// doors, no matter the direction, are a partial obstruction

	return 1;
}
PRAGMA_ENABLE_OPTIMIZATION