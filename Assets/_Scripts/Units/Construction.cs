using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class Construction : MonoBehaviour
{
    [SerializeField]
    GameObject[] constructables;

    //[SerializeField]
    public MapGenerator mapGenerator { get; set; }
    public GameObject[] Constructables { get => constructables; set => constructables = value; }

    bool wasPressed = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // todo - convert to resource checks for construction
        // todo - convert to local player check
        if (GetComponent<PlayerTurnTaker>().IsHuman)
        {
            RunLocalPlayerConstruction();
        }
    }

    public bool Build(IsoBuildingData buildingChoice)
    {
        var player = GetComponent<PlayerTurnTaker>();
        if(player == null)
            return false;
        var pos = GetComponent<PlayerUnitController>().currentlySelectedUnit.transform.position;
        var building = mapGenerator.AddDecorationsPrefab(pos, buildingChoice.isoBuilding);
        if (building == null)
            return false;
        if (player)
        {
            player.AddBuilding(building);
            building.GetComponent<ResourceCollector>().Register(player.GetComponent<PlayerResources>());
            var playerResources = GetComponent<PlayerResources>();
           /* playerResources.Resource[ResourceType.Wood] -= buildingChoice.Cost(ResourceType.Wood);
            playerResources.Resource[ResourceType.Metal] -= buildingChoice.Cost(ResourceType.Metal);
            playerResources.Resource[ResourceType.Prestige] -= buildingChoice.Cost(ResourceType.Prestige);*/

            if(buildingChoice.Cost(ResourceType.Wood) != 0)
                playerResources.OnResourcesModified(ResourceType.Wood, -buildingChoice.Cost(ResourceType.Wood));
            if (buildingChoice.Cost(ResourceType.Metal) != 0)
                playerResources.OnResourcesModified(ResourceType.Metal, -buildingChoice.Cost(ResourceType.Metal));
            if (buildingChoice.Cost(ResourceType.Prestige) != 0)
                playerResources.OnResourcesModified(ResourceType.Prestige, -buildingChoice.Cost(ResourceType.Prestige));
            return true;
        }

        return false;
    }

    void RunLocalPlayerConstruction()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            var playerUnitController = GetComponent<PlayerUnitController>();
            bool canConstruct = playerUnitController.GetNumRemainingActions() > 0;
            var player = GetComponent<PlayerTurnTaker>();

            if (wasPressed == false && canConstruct)// prevent double bounce
            {
                if (Keyboard.current.bKey.isPressed)
                {
                    wasPressed = true;
                    var pos = GetComponent<PlayerUnitController>().currentlySelectedUnit.transform.position;
                    var building = mapGenerator.AddDecorationsPrefab(pos, Constructables[0]);
                    if (player)
                    {
                        player.AddBuilding(building);
                        building.GetComponent<ResourceCollector>().Register(player.GetComponent<PlayerResources>());
                    }
                    // Debug.Log("Building made b!");
                }
                else if (Keyboard.current.xKey.isPressed)
                {
                    wasPressed = true;
                    var pos = GetComponent<PlayerUnitController>().currentlySelectedUnit.transform.position;
                    var building = mapGenerator.AddDecorationsPrefab(pos, Constructables[1]);
                    if (player)
                    {
                        player.AddBuilding(building);
                        building.GetComponent<ResourceCollector>().Register(player.GetComponent<PlayerResources>());
                    }
                    //  Debug.Log("Building made x!");
                }
                if(wasPressed)
                {
                    playerUnitController.ConsumeActions(1);
                }
            }
        }
        else
        {
            wasPressed = false;
        }
    }
}
