using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnit : MonoBehaviour
{
    [SerializeField]
    private IsoUnitData data;    
    [Tooltip("Main GameObject that will be clicked on and scaled")]
    public GameObject gameModel;
    Vector3 originalScale;

    [HideInInspector]
    public PlayerTurnTaker playerOwner;
    bool isScaled = false;
    int movesRemaining;

    public int MovesRemaining { get { return movesRemaining; } set { movesRemaining = value;  } }
    public IsoUnitData Data { get => data;  }
    public IsoUnitStatsCanvasController dataDisplay;

    void Start()
    {
        originalScale = gameModel.transform.localScale;
        if(originalScale.magnitude < 0.1f ) { originalScale = Vector3.one; }
        Data.isoUnit = this.transform.gameObject;
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
        var display = dataDisplay?.gameObject.GetComponent<IsoUnitStatsCanvasController>();
        
        if (isSelected == false)
        {
            display.Clear();
            ScaleSelected(false);
        }
        if (isSelected == true)
        {
            display.SetPosition(transform.localPosition);
            display.Set(Data, this.transform.gameObject);
            ScaleSelected(true);
        }
    }

    public void TurnReset()
    {
        movesRemaining = Data.numActionsPerTurn;
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
                gameModel.transform.localScale = originalScale;
                RemoveOutline();
            }
        }
        else //if (scaleUp == true)
        {
            if (isScaled == false)
            {
                isScaled = true;
                gameModel.transform.localScale = originalScale * 3;
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

