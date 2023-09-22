using UnityEngine;
using UnityEngine.InputSystem;

public class Construction : MonoBehaviour
{
    [SerializeField]
    GameObject[] constructables;

    //[SerializeField]
    public MapGenerator mapGenerator { get; set; }

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
                    var building = mapGenerator.AddDecorationsPrefab(pos, constructables[0]);
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
                    var building = mapGenerator.AddDecorationsPrefab(pos, constructables[1]);
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
