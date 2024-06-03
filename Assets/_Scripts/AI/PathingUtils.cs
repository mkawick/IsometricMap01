#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS8321 // Local function is declared but never used
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using U8 = System.Byte;
using U32 = System.UInt32;
using static UnityEditor.PlayerSettings;
using GameMap = System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, MapTile>>;
using System;
using UnityEngine.Assertions;
using System.Linq;

public class IntVector3
{
    public int x, y, z;

    public IntVector3() { x = (0); y = (0); z = (0); }
    public IntVector3(int _x, int _y, int _z)  { x = (_x); y = (_y); z = (_z); }
    public IntVector3(IntVector3 v) { x = v.x; y = v.y; z = v.z; }

    static public IntVector3 operator + (IntVector3 lhs, IntVector3 rhs) 
	{
		lhs.x += rhs.x; 
		lhs.y += rhs.y;
		lhs.z += rhs.z;
		return lhs;
	}
    public static IntVector3 operator - (IntVector3 lhs, IntVector3 rhs)
    {
        lhs.x -= rhs.x; 
        lhs.y -= rhs.y;
        lhs.z -= rhs.z;
        return lhs;
    }
    public static bool operator ==(IntVector3 hashed, int hashValue) => PathingUtils.CalcHash(hashed.x, hashed.y, hashed.z) == hashValue;
    public static bool operator !=(IntVector3 hashed, int hashValue) => !(hashed == hashValue);
    static public bool operator == (IntVector3 lhs, IntVector3 rhs) => (lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z);
    public static bool operator !=(IntVector3 lhs, IntVector3 rhs) => !(lhs == rhs);

   /* public static implicit operator IntVector3(int _x, int _y, int _z)
    {
        return new IntVector3(_x, _y, _z);
    }*/
}

public class PathNode
{
    public IntVector3 pos;
    public MapTile referenceTile;
    public PathNode cameFrom;
    public bool walkable;

    public int gCost, hCost;

    public PathNode() => BasicInit();
    public PathNode(IntVector3 _pos)
    {
        BasicInit();
        pos = _pos;
    }
    public PathNode(MapTile mapTile)
    {
        pos = mapTile.pos;
        referenceTile = mapTile;
        cameFrom = null;
        walkable = !mapTile.blocked;
        gCost = (int)1e6;
        hCost = (int)1e6;
    }

    public void BasicInit()
    {
        //referenceTile = null;
        cameFrom = null;
        walkable = true;
        gCost = (int)1e6;
        hCost = (int)1e6;
        pos.x = 0;
        pos.y = 0;
        pos.z = 0;

    }

    public void ClearPath()
    {
        cameFrom = null;
        gCost = (int)1e6;
        hCost = (int)1e6;
    }

    public int fCost()
    {
        return gCost + hCost;
    }

    public static bool operator == (PathNode lhs, PathNode rhs)
	{
        if(UnityEngine.Object.ReferenceEquals(lhs, null)) return false;
        if (UnityEngine.Object.ReferenceEquals(rhs, null)) return false;
        //if (rhs is null) return false;
        //if(ReferenceEquals(lhs, rhs)) return true;
        return lhs.pos == rhs.pos && lhs.referenceTile == rhs.referenceTile;
	}
    public static bool operator !=(PathNode lhs, PathNode rhs) => !(lhs == rhs); 
}

public class PathData
{
    public List<PathNode> path;
    public float fcost, gcost, hcost;

    public PathData()
    {
        fcost = 0;
        gcost = 0;
        hcost = 0;
        path = new List<PathNode>();
    }
    public PathData(PathNode lastNodeInPath) : this()
    {
        CleanUpPath(lastNodeInPath);
    }

    public void CleanUpPath(PathNode end)
    {
        PathNode current = end;

        while (UnityEngine.Object.ReferenceEquals(current, null)) //(current != null)
        {
            hcost += current.hCost;
            gcost += current.gCost;
            fcost += current.fCost();

            path.Add(current);
            current = current.cameFrom;
        }

        for (int first = 0, last = path.Count - 1; first < last; first++, last--)
        {
            var node = path[first];
            path[first] = path[last];
            path[last] = node;
        }
    }
}

public class MapTile
{
    public IntVector3 pos;// just for lookup
    public U8 cost;
    public bool blocked;
    public PathingUtils.Passability passability;

    public MapTile(IntVector3 pos, U8 cost, bool blocked, PathingUtils.Passability passability)
    {
        this.pos = pos;
        this.cost = cost;
        this.blocked = blocked;
        this.passability = passability;
    }

    public static bool operator ==(MapTile lhs, MapTile rhs)
    {
        return lhs.pos == rhs.pos && lhs.passability == rhs.passability && lhs.cost == rhs.cost;
    }
    public static bool operator !=(MapTile lhs, MapTile rhs) => !(lhs == rhs);
}

public class PathingUtils
{
    GameMap tiles;

    GameMap GetTiles()
    {
	    return tiles;
    }

    MapTile GetTile(IntVector3 pos)
    {
        Assert.IsTrue(IsValidMapPosition(pos));

	    int hash = CalcHash(pos);
	    return tiles[pos.z][hash];
    }

    public void PrintMap(GameMap map, IntVector3 minExtent, IntVector3 maxExtent)
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
                    if (IsValidMapPosition(pos, map))
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
    public bool IsValidMapPosition(IntVector3 pos)
    {
        var hash = CalcHash(pos);
        return (tiles[pos.z].ContainsKey(hash));
    }

    public static bool IsValidMapPosition(IntVector3 pos, GameMap tiles)
    {
        var hash = CalcHash(pos);
        return (tiles[pos.z].ContainsKey(hash));
    }

    public static float GetCostTo(IntVector3 from, IntVector3 dest)
    {
        float distz = (float)dest.z - (float)from.z;
        float disty = (float)dest.y - (float)from.y;
        float distx = (float)dest.x - (float)from.x;

        float squareDist = distx * distx + disty * disty + distz * distz;
        float dist = Mathf.Sqrt(squareDist);

        return dist;
    }
   /* float GetCostTo(List<MapTile> map, IntVector3 from, IntVector3 dest, IntVector3 mapDimensions)
    {
        return pos.y * mapDimensions.x + pos.x;
    }*/

    public int CalculateIndex(IntVector3 pos, IntVector3 mapDimensions)
    {
        return pos.y * mapDimensions.x + pos.x;
    }

    public int CalculateIndex(int x, int y, IntVector3 mapDimensions)
    {
        return y * mapDimensions.x + x;
    }

    public static Passability CombineInstance(ref Passability a, Passability b) 
    { 
        a |= b; return a; 
    }

    public static int CalcHash(IntVector3 v)
    {
        return CalcHash(v.x, v.y, v.z);
    }

    public static int CalcHash(int a, int b, int c)
    {
        //https://en.wikipedia.org/wiki/List_of_prime_numbers
        int hash = (a ^ 0xF) * 4493 + (b ^ 0xB) * 83 + (c ^ 0xC);
        return hash;
    }


    public int Hash(IntVector3 v)
    {
	    return CalcHash(v.x, v.y, v.z);
    }

    public int Hash2D(IntVector3 v) 
    {
	    return CalcHash(v.x, v.y, 0);
    }

    public enum Passability : U8
    {
        clear = 0,
        ydir_pos_blocked = 1 << 0,
        xdir_pos_blocked = 1 << 1,
        ydir_neg_blocked = 1 << 2,
        xdir_neg_blocked = 1 << 3,
        ydir_pos_partly_blocked = 1 << 4,
        xdir_pos_partly_blocked = 1 << 5,
        ydir_neg_partly_blocked = 1 << 6,
        xdir_neg_partly_blocked = 1 << 7,
        blocked = ydir_pos_blocked | xdir_pos_blocked | ydir_neg_blocked | xdir_neg_blocked,
        partly_blocked = ydir_pos_partly_blocked | xdir_pos_partly_blocked | ydir_neg_partly_blocked | xdir_neg_partly_blocked
    }

    public static bool CheckPassability(Passability l, Passability r)
    {
        return ((U8)l & (U8)r) != 0;
    }
}

class PathingTracker
{
    Vector3 WorldStartPosition, WorldEndPosition;
    List<Vector3> NodesToFollow;
    int currentIndex;
    bool isWorkingOnFinalNode;

    public PathingTracker() { Clear(); }
    public bool IsUnderway()
	{
		return (NodesToFollow.Count != 0 && 
			(currentIndex<NodesToFollow.Count) || 
			isWorkingOnFinalNode == true);	
	}
    public bool IsPathComplete()
    {
        return (currentIndex >= (NodesToFollow.Count - 1)) && (isWorkingOnFinalNode == true);
    }
    public void Clear() { NodesToFollow.Clear(); currentIndex = 0; isWorkingOnFinalNode = false; }

    public Vector3 GetNext()
    {
        Debug.Assert(IsUnderway());
        Vector3 Pos = NodesToFollow[currentIndex];

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

    public bool IsFinalNode() { return currentIndex >= (NodesToFollow.Count - 1); }
    public int Index() { return currentIndex; }
	    Vector3 GetCurrent()
    {
        Debug.Assert(IsUnderway());
        return NodesToFollow[currentIndex];
    }
    void Set(Vector3 StartPosition, Vector3 EndPosition, List<Tuple<Vector3, Vector3>> pathNodes)
    {
        Clear();
        WorldStartPosition = StartPosition;
        WorldEndPosition = EndPosition;

        foreach (var pair in pathNodes)
        {
            var destination = pair.Item2;
            NodesToFollow.Add(destination);
        }
    }
    void Set(Vector3 StartPosition, Vector3 EndPosition)
    {
        Clear();
        isWorkingOnFinalNode = true;// only begin and end
        WorldStartPosition = StartPosition;
        WorldEndPosition = EndPosition;
    }
}

public class Stairs
{
	public IntVector3 start, end;
    public float cost;
    public float distCalc;

    public Stairs(IntVector3 start, IntVector3 end, float cost, float distCalc)
    {
        this.start = start;
        this.end = end;
        this.cost = cost;
        this.distCalc = distCalc;
    }

    public bool Contains(IntVector3 p1, IntVector3 p2) 
	{
		if((start == p1) || (start == p2) || (end == p1) || (end == p2))
			return true;
		return false;
	}
};

public class Shaft
{
    public Vector3 start, end;
    public float cost;
    public float distCalc;
};

public class Elevator
{
    public List<int> floors;
    public List<IntVector3> elevatorPos;

    public float costMovingAway;
    public float costMovingToward;

    public int currentDirection;
    public int currentFloor, nextFloor;
    public bool isMoving;

    public List<float> costPerFloorDistanceConfig;
    public enum MovingDir { stationary, up, down };

    public float GetCostToFloor(int from, int to) 
    {
	    if (currentDirection == (int) MovingDir.stationary)
	    {
		    return Mathf.Abs(to - from) * ((costMovingToward + costMovingAway)*0.5f);
	    }
        int totalCost = 0;
        if (from == currentFloor)
            return 0;

        int topFloor = floors.Max(); //*max_element(std::begin(floors), std::end(floors));
        int bottomFloor = floors.Min();// (std::begin(floors), std::end(floors));

        if (from < to)
        {
            if (from < currentFloor)// elevator above, we want to go up
            {
                if (currentDirection == (int) MovingDir.up)
                {
                    totalCost = (int)(Mathf.Abs(to - from) * costMovingToward + Mathf.Abs(topFloor - currentFloor) * costMovingAway);
                }
                else // down
                {
                    totalCost = (int)(Mathf.Abs(from - currentFloor) * costMovingToward);
                }
            }
            else // we are above the elevator, we want to go up
            {
                if (currentDirection == (int) MovingDir.up)
                {
                    totalCost = (int)(Mathf.Abs(from - currentFloor) * costMovingToward);
                }
                else
                {
                    totalCost = (int)(Mathf.Abs(to - from) * costMovingToward + Mathf.Abs(bottomFloor - currentFloor) * costMovingAway);
                }
            }
        }
        else // ... going down
        {
            if (from < currentFloor)// elevator above, we want to go down
            {
                if (currentDirection == (int) MovingDir.up)
                {
                    totalCost = (int)(Mathf.Abs(to - from) * costMovingToward + Mathf.Abs(topFloor - currentFloor) * costMovingAway);
                }
                else
                {
                    totalCost = (int)(Mathf.Abs(from - currentFloor) * costMovingToward);
                }
            }
            else // we are above the elevator, we want to go down
            {
                if (currentDirection == (int) MovingDir.up)
                {
                    totalCost = (int)(Mathf.Abs(from - currentFloor) * costMovingToward);
                }
                else
                {
                    totalCost = (int)(Mathf.Abs(to - from) * costMovingToward + Mathf.Abs(bottomFloor - currentFloor) * costMovingAway);
                }
            }
        }

        return totalCost;
    }
    

    public bool Contains(IntVector3 p1, IntVector3 p2)
	{
		foreach (var pos in elevatorPos)
		{
			if ((pos == p1) || (pos == p2))
				return true;
		}
        return false;
	}
};

#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning restore CS8321 // Local function is declared but never used
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()