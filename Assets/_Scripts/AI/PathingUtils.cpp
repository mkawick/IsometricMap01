#include "RSG_PathingUtils.h"

#include <chrono>
#include <iostream>
#include <conio.h>
#include <functional>
#include <algorithm>
#include <limits>
#include <unordered_set>
#include "RSG_Pathing.h"

bool operator & (Passability l, Passability r) 
{ 
	return (U8)(l) & (U8)(r); 
}


float Elevator::GetCostToFloor(int from, int to) const
{
	if (currentDirection == MovingDir::stationary)
	{
		return abs(to - from) * ((costMovingToward + costMovingAway)*0.5f);
	}
	int totalCost = 0;
	if (from == currentFloor)
		return 0;

	int topFloor = *max_element(std::begin(floors), std::end(floors)); 
	int bottomFloor = *min_element(std::begin(floors), std::end(floors));

	if (from < to)
	{
		if (from < currentFloor)// elevator above, we want to go up
		{
			if (currentDirection == MovingDir::up)
			{
				totalCost = abs(to - from) * costMovingToward + abs(topFloor - currentFloor) * costMovingAway;
			}
			else // down
			{
				totalCost = abs(from - currentFloor) * costMovingToward;
			}
		}
		else // we are above the elevator, we want to go up
		{
			if (currentDirection == MovingDir::up)
			{
				totalCost = abs(from - currentFloor) * costMovingToward;
			}
			else
			{
				totalCost = abs(to - from) * costMovingToward + abs(bottomFloor - currentFloor) * costMovingAway;
			}
		}
	}
	else // ... going down
	{
		if (from < currentFloor)// elevator above, we want to go down
		{
			if (currentDirection == MovingDir::up)
			{
				totalCost = abs(to - from) * costMovingToward + abs(topFloor - currentFloor) * costMovingAway;
			}
			else
			{
				totalCost = abs(from - currentFloor) * costMovingToward;
			}
		}
		else // we are above the elevator, we want to go down
		{
			if (currentDirection == MovingDir::up)
			{
				totalCost = abs(from - currentFloor) * costMovingToward;
			}
			else
			{
				totalCost = abs(to - from) * costMovingToward + abs(bottomFloor - currentFloor) * costMovingAway;
			}
		}
	}

	return totalCost;
}

bool IsValidMapPosition(const Vector3& pos, const Vector3& mapDimensions, const std::vector<MapTile>& map)
{
	if (pos.x < 0 || pos.y < 0)
		return false;
	if (pos.x >= mapDimensions.x || pos.y >= mapDimensions.y)
		return false;
	if (map[CalculateIndex(pos, mapDimensions)].blocked)
		return false;

	return true;
}

float GetCostTo(const Vector3& from, const Vector3& dest)
{
	float distz = (float)dest.z - (float)from.z;
	float disty = (float)dest.y - (float)from.y;
	float distx = (float)dest.x - (float)from.x;

	float squareDist = distx * distx + disty * disty + distz * distz;
	float dist = std::sqrt(squareDist);

	return dist;
}

int CalculateIndex(const Vector3& pos, const Vector3& mapDimensions)
{
	return pos.y * mapDimensions.x + pos.x;
}

int CalculateIndex(int x, int y, const Vector3& mapDimensions)
{
	return y * mapDimensions.x + x;
}

void PathingTracker::Set(const FVector& StartPosition, const FVector& EndPosition, const std::deque<std::pair<Vector3, Vector3>>& pathNodes)
{
	WorldStartPosition = StartPosition;
	WorldEndPosition = EndPosition;
	currentIndex = 0;
	isWorkingOnFinalNode = false;

	for (auto it : pathNodes)
	{
		FVector destination(it.second);
		NodesToFollow.Add(destination);
	}
}

void PathingTracker::Set(const FVector& StartPosition, const FVector& EndPosition)
{
	NodesToFollow.Empty();
	WorldStartPosition = StartPosition;
	WorldEndPosition = EndPosition;
	currentIndex = 0;
	isWorkingOnFinalNode = true;// only begin and end
}