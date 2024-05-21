#pragma once

typedef unsigned char U8;
typedef unsigned int U32;

#include <deque>
#include <vector>

enum class Passability : U8
{
	clear = 0,
	ydir_pos_blocked = 1 << 0, blocked_right = ydir_pos_blocked,//forward backward left right
	xdir_pos_blocked = 1 << 1, blocked_away_from_camera = xdir_pos_blocked,
	ydir_neg_blocked = 1 << 2, blocked_left = ydir_neg_blocked,
	xdir_neg_blocked = 1 << 3, blocked_toward_camera = xdir_neg_blocked,
	ydir_pos_partly_blocked = 1 << 4,
	xdir_pos_partly_blocked = 1 << 5,
	ydir_neg_partly_blocked = 1 << 6,
	xdir_neg_partly_blocked = 1 << 7,
	blocked = ydir_pos_blocked | xdir_pos_blocked | ydir_neg_blocked | xdir_neg_blocked,
	partly_blocked = ydir_pos_partly_blocked | xdir_pos_partly_blocked | ydir_neg_partly_blocked | xdir_neg_partly_blocked
};
bool operator & (Passability l, Passability r);

Passability& operator |=(Passability& a, Passability b);

struct Vector3
{
	int  x, y, z;

	Vector3() : x(0), y(0), z(0) {}
	Vector3(int _x, int _y, int _z) : x(_x), y(_y), z(_z) {}
	Vector3(const Vector3& v) : x(v.x), y(v.y), z(v.z) {}
	Vector3(const FVector& v) : x(v.X), y(v.Y), z(v.Z) {}

	// friends defined inside class body are inline and are hidden from non-ADL lookup
	friend Vector3 operator + (Vector3 lhs,        // passing lhs by value helps optimize chained a+b+c
		const Vector3& rhs) // otherwise, both parameters may be const references
	{
		lhs.x += rhs.x; // reuse compound assignment
		lhs.y += rhs.y;
		lhs.z += rhs.z;
		return lhs; // return the result by value (uses move constructor)
	}
	friend Vector3 operator - (Vector3 lhs,        // passing lhs by value helps optimize chained a+b+c
		const Vector3& rhs) // otherwise, both parameters may be const references
	{
		lhs.x -= rhs.x; // reuse compound assignment
		lhs.y -= rhs.y;
		lhs.z -= rhs.z;
		return lhs; // return the result by value (uses move constructor)
	}
	Vector3 operator = (const FVector& rhs) // otherwise, both parameters may be const references
	{
		x -= rhs.X; // reuse compound assignment
		y -= rhs.Y;
		z -= rhs.Z;
		return *this; // return the result by value (uses move constructor)
	}


	int Hash() const;
	int Hash2D() const;
	bool operator == (int hashValue) const;
	operator FVector () const { return FVector((float)x, (float)y, (float)z); }
};
bool operator==(const Vector3& lhs, const Vector3& rhs);


struct MapTile
{
	Vector3		pos;// just for lookup
	U8			cost;
	U8			blocked;
	Passability	passability;
};

void PrintMap(const std::vector<MapTile>& map, Vector3 mapDimensions);

bool IsValidMapPosition(const Vector3& pos, const Vector3& mapDimensions, const std::vector<MapTile>& map);

float GetCostTo(const Vector3& from, const Vector3& dest);
float GetCostTo(const std::vector<MapTile>& map, const Vector3& from, const Vector3& dest, const Vector3& mapDimensions);

int CalculateIndex(const Vector3& pos, const Vector3& mapDimensions);
int CalculateIndex(int x, int y, const Vector3& mapDimensions);
int CalcHash(int a, int b, int c);

struct PathingTracker
{
	FVector WorldStartPosition, WorldEndPosition;
	TArray<FVector> NodesToFollow;// a queue does not support Num or Count, so we use an array
	int currentIndex;
	bool isWorkingOnFinalNode;

	PathingTracker() { Clear(); }
	bool IsUnderway() const 
	{
		return (NodesToFollow.Num() != 0 && 
			(currentIndex < NodesToFollow.Num()) || 
			isWorkingOnFinalNode == true);	
	}
	bool IsPathComplete() 
	{
		return (currentIndex >= (NodesToFollow.Num() - 1)) && (isWorkingOnFinalNode == true); 
	}
	void Clear() { NodesToFollow.Empty(); currentIndex = 0; isWorkingOnFinalNode = false; }
	bool IsFinalNode() const { return currentIndex >= (NodesToFollow.Num() - 1); }
	int Index() const { return currentIndex; }
	FVector GetCurrent()
	{
		ensure(IsUnderway());
		return NodesToFollow[currentIndex];
	}
	FVector GetNext()
	{
		ensure(IsUnderway());
		FVector Pos = NodesToFollow[currentIndex];

		if (IsFinalNode() == true)
		{
			Pos = WorldEndPosition;
			isWorkingOnFinalNode = true;
		}
		else
		{
			currentIndex++;
		}
		return Pos;
	}

	void Set(const FVector& WorldStartPosition, const FVector& WorldEndPosition, const std::deque<std::pair<Vector3, Vector3>>& pathNodes);
	void Set(const FVector& WorldStartPosition, const FVector& WorldEndPosition);
};