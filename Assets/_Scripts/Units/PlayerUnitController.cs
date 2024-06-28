using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUnitController : MonoBehaviour
{
    public GameObject currentlySelectedUnit;
    public MapGenerator mapGenerator;
    public static float MoveDist { get { return 1; } }
    public enum MoveDir { North, East, South, West};
    void Start()
    {
        GameModeManager.OnGameModeChanged += OnGameGameModeChanged;
    }

    void OnGameGameModeChanged(GameModeManager.GameMode mode, bool regularGame)
    {
        if (mode == GameModeManager.GameMode.StartSinglePlayerGame)
            UpdateUnitOnTile();
    }

    public bool IsUnitOutOfActions()
    {
        if (currentlySelectedUnit == null)
            return true;

        return currentlySelectedUnit.GetComponent<IsoUnit>().IsOutOfActions();
    }

    public bool ConsumeActions(int numActions)
    {
        if(currentlySelectedUnit == null || currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining < numActions)
            return false;

        currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining -= numActions;
        return true;
    }

    public int GetNumRemainingActions()
    {
        if (currentlySelectedUnit == null)
            return 0;
        return currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining;
    }

    public void SetNewUnitToControl(GameObject unit)
    {
        var cam = Camera.main.GetComponent<IsoCameraController>();
        if (unit != null)
        {
            cam.SetTarget(unit);            
        }
        currentlySelectedUnit = unit;
    }

    public void ControlledUpdate(bool isHuman)
    {
        if (currentlySelectedUnit == null)
            return;

        if (isHuman)
        {
            InputControl();
        }
        else
        {
            RandomWalker();
        }
    }

    void InputControl()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            IosUnitMovementController iosUnitMovementController = new IosUnitMovementController();
            iosUnitMovementController.mapGenerator = mapGenerator;
            iosUnitMovementController.ResetMoves(currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining);
            if (currentlySelectedUnit != null)
            {
                bool didMove = false;
                if (Keyboard.current.downArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.South);
                }
                if (Keyboard.current.upArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.North);
                }
                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.East);
                }
                if (Keyboard.current.leftArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.West);
                }
                if(didMove)
                {
                    currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining = iosUnitMovementController.MovesRemaining;
                    currentlySelectedUnit.GetComponent<IsoUnit>().PostMoveCleanup();
                }
            }

        }
    }

    float nextMoveTime;
    void RandomWalker()
    {
        if (nextMoveTime < Time.time)
        {
            IosUnitMovementController iosUnitMovementController = new IosUnitMovementController();
            iosUnitMovementController.mapGenerator = mapGenerator;
            iosUnitMovementController.ResetMoves(currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining);

            nextMoveTime = Time.time + Random.Range(1, 3);
            var test2dPos = MapUtils.ConvertScreenPositionToMap(this.transform.position);

            int dir = Random.Range(0, 3);
            for (int i = 0; i < 4; i++, dir++)
            {// 4 should be part of the unit config

                var offset = MapUtils.Dir4Lookup(dir);
                if (MapUtils.IsDirValid(test2dPos + offset))
                {
                    bool didMove = iosUnitMovementController.Move(currentlySelectedUnit, (MoveDir)dir);
                    if (didMove)
                    {
                        currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining = iosUnitMovementController.MovesRemaining;
                        currentlySelectedUnit.GetComponent<IsoUnit>().PostMoveCleanup();
                    }
                    break;
                }
            }
        }
    }

    void UpdateUnitOnTile()
    {
        if (currentlySelectedUnit == null)
            return;

        var pos = currentlySelectedUnit.transform.position;
        var tile = mapGenerator.GetTile(pos);
        if (tile)
        {
            tile.GetComponent<TileBehavior>().unitOnTop = currentlySelectedUnit;
        }
    }
}
