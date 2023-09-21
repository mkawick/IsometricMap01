using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnit : MonoBehaviour
{
    public IsoUnitData data;
    public IsoUnitStatsCanvasController dataDisplay;
    [Tooltip("GameObject")]
    public GameObject gameModel;

    bool isScaled = false;

    int movesRemaining;
    public int MovesRemaining { get { return movesRemaining; } set { movesRemaining = value;  } }

    void Start()
    {
        data.isoUnit = this.transform.gameObject;
        if (gameModel == null)
        {
            gameModel = this.transform.gameObject;
        }
        //movesRemaining = data.numActionsPerTurn;
    }

    public void SetDataDisplay(IsoUnitStatsCanvasController popup)
    {
        dataDisplay = popup;
    }

    public void Selected(bool isSelected)
    {
        dataDisplay?.gameObject.SetActive(isSelected);
        if(isSelected == false)
        {
            ScaleSelected(false);
        }
        if (isSelected == true)
        {
            var display = dataDisplay?.gameObject.GetComponent<IsoUnitStatsCanvasController>();
            display.SetPosition(transform.localPosition);
            display.Set(data, this.transform.gameObject);
            ScaleSelected(true);
        }
    }

    public void TurnReset()
    {
        movesRemaining = data.numActionsPerTurn;
    }

    public bool IsOutOfActions()
    { 
        return movesRemaining <= 0;
    }

    void ScaleSelected(bool scaleUp)
    {
        if (scaleUp == false)
        {
            if (isScaled)
            {
                isScaled = false;
                gameModel.transform.localScale = new Vector3(1, 1, 1);
                RemoveOutline();
            }
        }
        else //if (scaleUp == true)
        {
            if (isScaled == false)
            {
                isScaled = true;
                gameModel.transform.localScale = new Vector3(2, 2, 2);
                AddOutline();
            }
        }
    }

    void AddOutline()
    {
        if (gameObject.GetComponent<Outline>() == null)
        {
            var outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 2f;
        }
    }
    void RemoveOutline()
    {
        var outline = gameObject.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }

    public bool Move(PlayerUnitController.MoveDir dir)
    {
        if (movesRemaining <= 0)
            return false;

        var pos = transform.position;
        var oldPos = pos;
        var rot = transform.rotation;

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
            transform.position = pos;
            transform.rotation = rot;
            PostMoveCleanup();
            movesRemaining--; // todo.. look at terrain and roads

            return true;
        }
        return false;
    }

    public void PostMoveCleanup()
    {
        ScaleSelected(false);
        dataDisplay?.gameObject.SetActive(false);
    }
}

