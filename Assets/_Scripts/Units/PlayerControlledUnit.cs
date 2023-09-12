using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlledUnit : MonoBehaviour
{
    public GameObject currentlySelectedUnit;
    public MapGenerator mapGenerator;
    public static float MoveDist { get { return 1; } }
    public enum MoveDir { North, East, South, West};
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

    public bool IsUnitOutOfActions()
    {
        if (currentlySelectedUnit == null)
            return true;

        return currentlySelectedUnit.GetComponent<IsoUnit>().IsOutOfActions();
    }

    public void ControlledUpdate()
    {
        if (currentlySelectedUnit == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            var isoUnit = currentlySelectedUnit.GetComponent<IsoUnit>();
            if(isoUnit != null)
            {
                var originalPosition = isoUnit.transform.position;
                bool didMove = false;
                if (Keyboard.current.downArrowKey.isPressed)
                {
                    didMove = isoUnit.Move(MoveDir.South);
                }
                if (Keyboard.current.upArrowKey.isPressed)
                {
                    didMove = isoUnit.Move(MoveDir.North);
                }
                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    didMove = isoUnit.Move(MoveDir.East);
                }
                if (Keyboard.current.leftArrowKey.isPressed)
                {
                    didMove = isoUnit.Move(MoveDir.West);
                }
                if (didMove)
                {
                    if (mapGenerator != null)
                    {
                        var oldTile = mapGenerator.GetTile(originalPosition);
                        if (oldTile)
                        {
                            oldTile.GetComponent<TileBehavior>().unitOnTop = null;
                        }
                        UpdateUnitOnTile();
                    }
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
