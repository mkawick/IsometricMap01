using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnit : MonoBehaviour
{
    public IsoUnitData data;
    public GameObject dataDisplay;
    public GameObject gameModel;    

    bool isScaled = false;

    int movesRemaining;

    void Start()
    {
        data.isoUnit = this.transform.gameObject;
        if (gameModel == null)
        {
            gameModel = this.transform.gameObject;
        }
        //movesRemaining = data.numActionsPerTurn;
    }

  /*  void Update()
    {
        
    }*/

    public void Selected(bool isSelected)
    {
        dataDisplay?.SetActive(isSelected);
        if(isSelected == false)
        {
            ScaleSelected(false);
        }
        if (isSelected == true)
        {
            var display = dataDisplay.GetComponent<IsoUnitStatsCanvasController>();
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
        return movesRemaining > 0;
    }

    void ScaleSelected(bool scaleUp)
    {
        if (scaleUp == false)
        {
            if (isScaled)
            {
                isScaled = false;
                gameModel.transform.localScale = new Vector3(1, 1, 1);
                var outline = gameObject.GetComponent<Outline>();

                if (outline != null)
                {
                    Destroy(outline);
                }
            }
        }
        else //if (scaleUp == true)
        {
            if (isScaled == false)
            {
                isScaled = true;
                gameModel.transform.localScale = new Vector3(2, 2, 2);
                if (gameObject.GetComponent<Outline>() == null)
                {
                    var outline = gameObject.AddComponent<Outline>();

                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineColor = Color.yellow;
                    outline.OutlineWidth = 2f;
                }
            }
        }
    }

    public bool Move(PlayerControlledUnit.MoveDir dir)
    {
        if (movesRemaining <= 0)
            return false;

        var pos = transform.position;
        var oldPos = pos;
        var rot = transform.rotation;

        switch (dir)
        {
            case PlayerControlledUnit.MoveDir.North:
                pos.z -= PlayerControlledUnit.MoveDist;
                rot.eulerAngles = new Vector3(0, 180, 0);
                break;
            case PlayerControlledUnit.MoveDir.South:
                pos.z += PlayerControlledUnit.MoveDist;
                rot.eulerAngles = new Vector3(0, 0, 0);
                break;
            case PlayerControlledUnit.MoveDir.East:
                pos.x -= PlayerControlledUnit.MoveDist;
                rot.eulerAngles = new Vector3(0, 270, 0);
                break;
            case PlayerControlledUnit.MoveDir.West:
                pos.x += PlayerControlledUnit.MoveDist;
                rot.eulerAngles = new Vector3(0, 90, 0);
                break;
        }
        if (oldPos != pos)
        {
            transform.position = pos;
            transform.rotation = rot;
            ScaleSelected(false);
            dataDisplay?.SetActive(false);
            movesRemaining--; // todo.. look at terrain and roads

            return true;
        }
        return false;
    }
}

