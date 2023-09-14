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
        IosUnitMovementController iosUnitMovementController = new IosUnitMovementController();
        iosUnitMovementController.mapGenerator = mapGenerator;
        iosUnitMovementController.ResetMoves(currentlySelectedUnit.GetComponent<IsoUnit>().MovesRemaining);
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            //var isoUnit = currentlySelectedUnit.GetComponent<IsoUnit>();
            if (currentlySelectedUnit != null)
            {
                var originalPosition = currentlySelectedUnit.transform.position;
                bool didMove = false;
                if (Keyboard.current.downArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.South, mapGenerator);
                }
                if (Keyboard.current.upArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.North, mapGenerator);
                }
                if (Keyboard.current.rightArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.East, mapGenerator);
                }
                if (Keyboard.current.leftArrowKey.isPressed)
                {
                    didMove = iosUnitMovementController.Move(currentlySelectedUnit, MoveDir.West, mapGenerator);
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
                    //this.transform.position += new Vector3Int(offset.x, 0, offset.y);
                    bool didMove = iosUnitMovementController.Move(currentlySelectedUnit, (MoveDir)dir, mapGenerator);
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
