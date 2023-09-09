using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnit : MonoBehaviour
{
    public IsoUnitData data;
    public GameObject dataDisplay;
    public GameObject gameModel;    

    bool isScaled = false;

    void Start()
    {
        data.isoUnit = this.transform.gameObject;
        if (gameModel == null)
        {
            gameModel = this.transform.gameObject;
        }
    }

    void Update()
    {
        
    }

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

    void ScaleSelected(bool scaleUp)
    {
        if (scaleUp == false)
        {
            if (isScaled)
            {
                isScaled = false;
                gameModel.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else //if (scaleUp == true)
        {
            if (isScaled == false)
            {
                isScaled = true;
                gameModel.transform.localScale = new Vector3(2, 2, 2);
            }
        }
    }

    public bool Move(PlayerControlledUnit.MoveDir dir)
    {
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

            return true;
        }
        return false;
    }
}

