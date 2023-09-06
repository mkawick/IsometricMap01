using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlledUnit : MonoBehaviour
{
    public GameObject currentlySelectedUnit;
    public MapGenerator mapGenerator;
    public float moveDist = 1;
    void Start()
    {
        // var gmm = GameObject.Find("GameModeManager");// todo.. this must be replaced
        GameModeManager.OnGameGameModeChanged += OnGameGameModeChanged;        
    }

    void OnGameGameModeChanged(GameModeManager.Mode mode)
    {
        if (mode == GameModeManager.Mode.StartSinglePlayerGame)
            UpdateUnitOnTile();
    }

    void Update()
    {
        if (currentlySelectedUnit == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            var pos = currentlySelectedUnit.transform.position;
            var oldPos = pos;
            var rot = currentlySelectedUnit.transform.rotation;
            if (Keyboard.current.downArrowKey.isPressed)
            {
                pos.z += moveDist;
                rot.eulerAngles = new Vector3(0,0,0);
            }
            if (Keyboard.current.upArrowKey.isPressed)
            {
                pos.z -= moveDist;
                rot.eulerAngles = new Vector3(0, 180, 0);
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                pos.x -= moveDist;
                rot.eulerAngles = new Vector3(0, 270, 0);
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                pos.x += moveDist;
                rot.eulerAngles = new Vector3(0, 90, 0);
            }
            if (oldPos != pos)
            {
                currentlySelectedUnit.transform.position = pos;
                currentlySelectedUnit.transform.rotation = rot;

                if (mapGenerator != null)
                {
                    var oldTile = mapGenerator.GetTile(oldPos);
                    if (oldTile)
                    {
                        oldTile.GetComponent<TileBehavior>().unitOnTop = null;
                    }
                    UpdateUnitOnTile();
                }
            }
        }
    }

    void UpdateUnitOnTile()
    {
        var pos = currentlySelectedUnit.transform.position;
        var tile = mapGenerator.GetTile(pos);
        if (tile)
        {
            tile.GetComponent<TileBehavior>().unitOnTop = currentlySelectedUnit;
        }
    }
}
