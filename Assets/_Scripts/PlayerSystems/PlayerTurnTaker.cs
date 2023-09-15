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
    [SerializeField]
    private IsoUnit startingIsoUnitPrefab;

    [SerializeField] private IsoUnitStatsCanvasController isoUnitPopupPrefab;
    IsoUnitStatsCanvasController isoUnitPopup;

    bool isMyTurn = false;
    bool isMyTurnFinished = false;

    public List<IsoUnit> unitsIOwn;
    public List<GameObject> buildingsIOwn;

    public List<GameObject> objectsNeedingAnUpdate;
    public MapGenerator mapGenerator
    {
        set
        {
            GetComponent<Construction>().mapGenerator = value;
            GetComponent<PlayerUnitController>().mapGenerator = value;
        } }
    public bool IsHuman { get { return isHuman; } }
    public string PlayerName { get { return playerName; } }

    #region TurnTaking
    public void YourTurn()
    {
        isMyTurn = true;
        isMyTurnFinished = false;

        // move all units that need an update 
        foreach (IsoUnit isoUnit in unitsIOwn)
        {
            objectsNeedingAnUpdate.Add(isoUnit.gameObject);

            isoUnit.TurnReset();
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
        var playerUnitController = GetComponent<PlayerUnitController>();

        if (playerUnitController != null)
        {
            playerUnitController.ControlledUpdate(isHuman);
            bool isDone = playerUnitController.IsUnitOutOfActions();
            if (isDone)
            {
                ControlledUnitIsDone(playerUnitController.currentlySelectedUnit);
                if (objectsNeedingAnUpdate.Count > 0)
                {
                    playerUnitController.SetNewUnitToControl( objectsNeedingAnUpdate[0]);
                    // todo pan camera to unit
                }
                else
                {
                    playerUnitController.SetNewUnitToControl(null);
                }
            }            
        }
        else
        {
            var aiPlayerUnitController = GetComponent<PlayerUnitController>();
            if (aiPlayerUnitController != null)
            {
                aiPlayerUnitController.ControlledUpdate(isHuman);
            }
        }
    }

    public bool AmIDoneWithMyTurn()
    {
        return isMyTurnFinished;
       // return true;
    }
    #endregion

    void Start()
    {
        isoUnitPopup = Instantiate(isoUnitPopupPrefab.gameObject, this.transform).GetComponent<IsoUnitStatsCanvasController>();
        var startingUnit = Instantiate(startingIsoUnitPrefab.gameObject, this.transform).GetComponent<IsoUnit>();

        unitsIOwn.Add(startingUnit);
        startingUnit.SetDataDisplay(isoUnitPopup);
        // todo.. set location to spawn
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
