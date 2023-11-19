using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitMovementController
{
    public MapGenerator mapGenerator { get; set; }

    protected int movesRemaining;
    public int MovesRemaining { get { return movesRemaining; } set { movesRemaining = value; } }

    public void ResetMoves(int numMoves)
    {
        movesRemaining = numMoves;
    }

    protected void UpdateUnitOnTile(GameObject controlled, Vector3 oldPos)
    {
        if (controlled == null)
            return;
        var oldTile = mapGenerator.GetTile(oldPos);
        if (oldTile)
        {
            oldTile.GetComponent<TileBehavior>().unitOnTop = null;
        }

        var pos = controlled.transform.position;
        var tile = mapGenerator.GetTile(pos);
        if (tile)
        {
            tile.GetComponent<TileBehavior>().unitOnTop = controlled;
        }
    }

    public abstract bool Move(GameObject controlled, PlayerUnitController.MoveDir dir);
}

/// <summary>
/// /////////////////////////////////////////////////////////////////
/// </summary>
public class IosUnitMovementController : UnitMovementController
{
    public override bool Move(GameObject controlled, PlayerUnitController.MoveDir dir)
    {
        if (movesRemaining <= 0)
            return false;

        var pos = controlled.transform.position;
        var oldPos = pos;
        var rot = controlled.transform.rotation;

        switch (dir)
        {
            case PlayerUnitController.MoveDir.North:
                pos.z -= PlayerUnitController.MoveDist;
                rot.eulerAngles = new Vector3(0, 180, 0);
                break;
            case PlayerUnitController.MoveDir.South:
                pos.z += PlayerUnitController.MoveDist;
                rot.eulerAngles = new Vector3(0, 0, 0);
                break;
            case PlayerUnitController.MoveDir.East:
                pos.x -= PlayerUnitController.MoveDist;
                rot.eulerAngles = new Vector3(0, 270, 0);
                break;
            case PlayerUnitController.MoveDir.West:
                pos.x += PlayerUnitController.MoveDist;
                rot.eulerAngles = new Vector3(0, 90, 0);
                break;
        }
        if (oldPos != pos)
        {
            var constrainedPos = MapUtils.Constrain(new Vector2Int((int)pos.x, (int)pos.z));
            controlled.transform.position = new Vector3(constrainedPos.x, pos.y, constrainedPos.y);
            controlled.transform.rotation = rot;

            UpdateUnitOnTile(controlled, oldPos);

            //ScaleSelected(false);
            //dataDisplay?.gameObject.SetActive(false);
            movesRemaining--; // todo.. look at terrain and roads

            return true;
        }
        return false;
    }
    
}
