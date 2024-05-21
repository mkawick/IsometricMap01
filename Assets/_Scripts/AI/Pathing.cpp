
#include "RSG_Pathing.h"

#include <conio.h>

#include "RSG_TileMap_PathFinder.h"

std::vector<PathNode*> FindPath(const Vector3& startPos, const Vector3& endPos, RSG_TileMap_PathFinder& originalMap)
{
	originalMap.ClearHistory();	

	if (endPos.z == startPos.z)// same horizon... likely a straight path
	{
		std::vector<PathNode*> openSet;
		std::vector<PathNode*> closedSet;
		auto outPath = originalMap.FindPathOnSingleLayer(startPos, endPos, openSet, closedSet);
		if (outPath.path.size())
		{
			return outPath.path;
		}
	}
	
	// we either have different layers, or there is no direct path between the nodes 
	{
		auto queueOfNodes = originalMap.FindPathWaypointsWithShafts(startPos, endPos);
		std::vector<PathNode*> openSet;
		std::vector<PathNode*> closedSet;
		std::vector<PathNode*> totalPath;

		for (auto node : queueOfNodes)
		{
			auto outPath = originalMap.FindPathOnSingleLayer(node.first, node.second, openSet, closedSet);
			
			if (outPath.path.size())
			{
				totalPath.insert(totalPath.end(), outPath.path.begin(), outPath.path.end());
			}
		}

		return totalPath;
	}

	return std::vector<PathNode*>();
}

void PrintPath(const std::vector<PathNode*>& path, Vector3 min, Vector3 max)
{
	for (int z = min.z; z < max.z; z++)
	{
		std::cout << "     ";
		for (int y = min.y; y < max.y; y++)
		{
			std::cout << std::setw(2) << std::to_string(y) + "  ";
		}
		std::cout << std::endl;
		for (int y = min.y; y < max.y; y++)
		{
			std::string pathNodes;// = std::to_string(y) + "   ";

			for (int x = min.x; x < max.x; x++)
			{
				Vector3 v(x, y, 0);
				bool found = false;
				for (auto node : path)
				{ 
					if (node->pos == v)
					{
						found = true;
						break;
					}
				}
				if (found)
				{
					pathNodes += "*  ";
				}
				else
				{
					pathNodes += "   ";
				}
			}

			std::cout << std::setw(2) << y << "   " << pathNodes << std::endl;
		}
		std::cout << std::endl;
	}
}

void GenerateRandomMap(std::vector<U8>& map, Vector3 dimensions)
{
	int num = dimensions.x* dimensions.y;
	int lineStartPosition = 6 * dimensions.x;
	int lineEndPosition = lineStartPosition + 14;

	for (int i = 0; i < num; i++)
	{
		if (i >= lineStartPosition && i <= lineEndPosition)
		{
			map.push_back(-1);
			continue;
		}
		int oddsOfBlock = rand() % 10;
		if (oddsOfBlock == 0)
		{
			map.push_back(-1);
		}
		else
		{
			int passability = rand() % 30;

			if (passability == 1)
				map.push_back(1);
			if(passability>1 && passability<=3)
				map.push_back(2); 
			if (passability > 3 && passability <= 6)
				map.push_back(3); 
			if (passability > 6 && passability <= 10)
				map.push_back(4);
			if (passability > 10 && passability <= 21)
				map.push_back(5);
			if(passability>21 && passability<=24)
				map.push_back(6);
			if (passability > 24 && passability <= 26)
				map.push_back(7);
			if (passability > 26 && passability <= 29)
				map.push_back(8);
			if (passability == 0)
				map.push_back(9);
		}
	}

	
/*	int width = dimensions.x - 15;
	for (int x = 0; x <width; x++)
	{
		map[x + lineStartPosition] = -1;
	}*/
}

void Pathing()
{
	RSG_TileMap_PathFinder tileMap;
	//tileMap.CreateMap(mapCosts, 1);
	//auto path = FindPath({ 0, 0, 0 }, { 6, 2, 0}, tileMap);

	//tileMap.CreateMap(mapCosts, 2);

	//tileMap.AddTunnel(Vector3(6, 6, 0), Vector3(6, 6, 1));

	//auto path = FindPath({ 0, 0, 0 }, { 7, 6, 1}, tileMap);
	std::vector<U8> map;
	Vector3 dimensions({ 20, 20, 1 });
	GenerateRandomMap(map, dimensions);
	U8* madData [] = {map.data() , nullptr};
	TileMapMainInteraction::CreateMap(tileMap, madData, 1, dimensions);

	auto beginTime = std::chrono::high_resolution_clock::now();
	auto path = FindPath({ 0, 0, 0 }, { 4, 16, 0 }, tileMap);
	auto endTime = std::chrono::high_resolution_clock::now();

	auto duration = std::chrono::duration_cast<std::chrono::microseconds>(endTime - beginTime);

	tileMap.PrintMap();
	PrintPath(path, tileMap.GetMinExtents(), tileMap.GetMaxExtents());

	std::cout << "Elapsed Time: " << duration.count();

	//_getch();
}
 