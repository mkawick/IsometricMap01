using UnityEngine;
using UnityEngine.InputSystem;

public class Construction : MonoBehaviour
{
    [SerializeField]
    GameObject[] constructables;

    [SerializeField]
    MapGenerator mapGenerator;

    bool wasPressed = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            if(Keyboard.current.bKey.isPressed && wasPressed == false)
            {
                wasPressed = true;
                var pos = GetComponent<PlayerControlledUnit>().currentlySelectedUnit.transform.position;
                mapGenerator.AddDecorationsPrefab(pos, constructables[0]);
            }
            else
            {
                wasPressed = false;
            }
        }
    }
}
