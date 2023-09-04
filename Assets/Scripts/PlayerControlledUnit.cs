using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlledUnit : MonoBehaviour
{
    public GameObject currentlySelectedUnit;
    public float moveDist = 1;
    //int key = Keyboard.current.
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlySelectedUnit == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            var pos = currentlySelectedUnit.transform.position;
            if (Keyboard.current.downArrowKey.isPressed)
            {
                pos.z += moveDist;
            }
            if (Keyboard.current.upArrowKey.isPressed)
            {
                pos.z -= moveDist;
            }
            if (Keyboard.current.rightArrowKey.isPressed)
            {
                pos.x -= moveDist;
            }
            if (Keyboard.current.leftArrowKey.isPressed)
            {
                pos.x += moveDist;
            }
            currentlySelectedUnit.transform.position = pos;
            //Debug.Log("A key was pressed");
        }
      /*  if (Gamepad.current.aButton.wasPressedThisFrame)
        {
            Debug.Log("A button was pressed");
        }*/
    }
}
