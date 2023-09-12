using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// most behaviours for a player should be contained here
public class PlayerTurnTaker : MonoBehaviour
{
    [SerializeField]
    private string playerName;
    readonly public int turnTakerId;
    [SerializeField]
    private bool isHuman;

    bool isMyTurn = false;
    bool isMyTurnFinished = false;

    public List<GameObject> unitsIOwn;
    public List<GameObject> buildingsIOwn;

    public List<GameObject> objectsNeedingAnUpdate;

    public void YourTurn()
    {
        isMyTurn = true;
        isMyTurnFinished = false;

        // move all units that need an update 
        foreach (GameObject go in unitsIOwn)
        {
            objectsNeedingAnUpdate.Add(go);
            var isoUnit = go.GetComponent<IsoUnit>();
            if (isoUnit)
            {
                isoUnit.TurnReset();
            }
            // same for buildings
        }
    }

    public void PlayEndTurnTransition(int nextPlayerId)
    {
        // play some screen
    }

    public void ControlledUnitIsDone(GameObject obj)
    {
        objectsNeedingAnUpdate.Remove(obj);
        if (objectsNeedingAnUpdate.Count == 0)
            isMyTurnFinished = true;
    }

    public void ControlledUpdate()
    {
        var playerControlledUnit = GetComponent<PlayerControlledUnit>();

        if (playerControlledUnit != null)
        {
            playerControlledUnit.ControlledUpdate();
            bool isDone = playerControlledUnit.IsUnitOutOfActions();
            if (isDone)
            {
                ControlledUnitIsDone(playerControlledUnit.currentlySelectedUnit);
            }
            if(objectsNeedingAnUpdate.Count > 0)
            {
                playerControlledUnit.currentlySelectedUnit = objectsNeedingAnUpdate[0];
                // todo pan camera to unit
            }
        }
        else
        {
            var aiPlayerControlledUnit = GetComponent<PlayerControlledUnit>();
            if (aiPlayerControlledUnit != null)
            {
                aiPlayerControlledUnit.ControlledUpdate();
            }
        }
    }

    public bool AmIDoneWithMyTurn()
    {
        // return isMyTurnFinished;
        return true;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isMyTurn)
        {

        }
    }

    public void TakeTurn()
    {
        // 
    }
}
