using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [HideInInspector]
    private UI_PlayerResources playerResourcesUi;

    [SerializeField] private IsoUnitStatsCanvasController isoUnitPopupPrefab;
    IsoUnitStatsCanvasController isoUnitPopup;

    bool isMyTurn = false;
    bool isMyTurnFinished = false;
    EnvironmentCollector environmentCollector;

    public List<IsoUnit> unitsIOwn;
    [SerializeField]
    public List<GameObject> buildingsIOwn;

    public List<GameObject> objectsNeedingAnUpdate;
    public MapGenerator mapGenerator
    {
        set
        {
            GetComponent<Construction>().mapGenerator = value;
            GetComponent<PlayerUnitController>().mapGenerator = value;
        } 
        get { return GetComponent<Construction>().mapGenerator; }// not needed, but here for completeness
    }
    public bool IsHuman { get { return isHuman; } }
    public string PlayerName { get { return playerName; } }
    public List<GameObject> BuildingsIOwn {  get { return buildingsIOwn; } }
    public bool IsRegularGame { get; set; }

    #region Basics
    public UI_PlayerResources PlayerResourcesUi 
    { 
        get => playerResourcesUi; 
        set 
        { 
            playerResourcesUi = value; 
            playerResourcesUi.SetupResourceCollector(GetComponent<PlayerResources>()); 
        } 
    }

    public void AddBuilding(GameObject building) 
    { 
        buildingsIOwn.Add(building);

        //playerResourcesUi.SetupResourceCollector(building.GetComponent<ResourceCollector>());
    }
    void Start()
    {
        isoUnitPopup = Instantiate(isoUnitPopupPrefab.gameObject, this.transform).GetComponent<IsoUnitStatsCanvasController>();
        var startingUnit = Instantiate(startingIsoUnitPrefab.gameObject, this.transform).GetComponent<IsoUnit>();

        unitsIOwn.Add(startingUnit);
        startingUnit.SetDataDisplay(isoUnitPopup);
        startingUnit.playerOwner = this;

        environmentCollector = GameObject.FindAnyObjectByType<EnvironmentCollector>();
        // todo.. set initial location to spawn
    }

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

    #endregion Basics
        
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
            // same for buildings is needed
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
                    // todo, make decision about the state of this unit
                    var nextUnitToControl = objectsNeedingAnUpdate[0];
                    //1) find a good spot to build, set the location, start moving toward it
                    //2) set unit to collect
                    //3) set unit to build
                    //4) Make wall

                    playerUnitController.SetNewUnitToControl(nextUnitToControl);
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
        ResourceUpdate();
    }

    void ResourceUpdate()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            //var playerUnitController = GetComponent<PlayerUnitController>();
            var playerTurnTaker = GetComponent<PlayerTurnTaker>();
            if (playerTurnTaker.IsHuman)
            {
                if (Keyboard.current.cKey.isPressed)
                {
                    if (IsRegularGame)
                    {
                        foreach (var building in buildingsIOwn)
                        {
                            building.GetComponent<ResourceCollector>().AddResources(environmentCollector, 1, 3, 5);
                        }
                    }
                    else
                    {
                        GetComponent<PlayerResources>().OnResourcesModified(2, 2, 2);
                    }
                }
                else if (Keyboard.current.rKey.isPressed)
                {
                   /* foreach (var building in buildingsIOwn)
                    {
                        building.GetComponent<ResourceCollector>().UseResources(2, 2, 2);
                    }*/
                    GetComponent<PlayerResources>().OnResourcesModified(-2, -2, -2);
                }
               /* else if (Keyboard.current.tKey.isPressed)
                {
                    GameObject.FindAnyObjectByType<EnvironmentCollector>().Collect(this.transform.position, ResourceType.Wood);
                }**/
            }
        }
    }

    public bool AmIDoneWithMyTurn()
    {
        return isMyTurnFinished;
       // return true;
    }
    #endregion
}
