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
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if (wasPressed == false)// prevent double bounce
            {
                if (Keyboard.current.bKey.isPressed)
                {
                    wasPressed = true;
                    var pos = GetComponent<PlayerUnitController>().currentlySelectedUnit.transform.position;
                    mapGenerator.AddDecorationsPrefab(pos, constructables[0]);
                   // Debug.Log("Building made b!");
                }
                else if (Keyboard.current.xKey.isPressed)
                {
                    wasPressed = true;
                    var pos = GetComponent<PlayerUnitController>().currentlySelectedUnit.transform.position;
                    mapGenerator.AddDecorationsPrefab(pos, constructables[1]);
                  //  Debug.Log("Building made x!");
                }
            }
        }
        else
        {
            wasPressed = false;
        }
    }
}
