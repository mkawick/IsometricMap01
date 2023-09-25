using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour
{
    private Vector3 Origin;
    private Vector3 Difference;
    private Vector3 ResetCamera;

    public Camera currentCamera;

    [SerializeField]
    GameUnitSelector gameUnitSelector;

    private bool drag = false;

    private void Start()
    {
        currentCamera = Camera.main;
        ResetCamera = currentCamera.transform.position;        
    }

    private void Update()
    {
        //Check for mouse click 
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            //EventSystem.current.IsPointerOverGameObject()
           // EventSystem.current.
            //if (EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit raycastHit;
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out raycastHit, 100f))
                {
                   // if (raycastHit.transform.gameObject.layer == LayerMask.NameToLayer("UI"))
                   //     return;

                    //IsPointerOverUIElement(raycastHit.)
                    if (raycastHit.transform != null)
                    {
                        var obj = raycastHit.transform.gameObject;
                        gameUnitSelector.SelectUnit(obj);
                    }
                    else
                    {
                        gameUnitSelector.SelectUnit(null);
                    }
                }
                else
                {
                    gameUnitSelector.SelectUnit(null);
                }
            }
        }
    }
    public bool IsPointerOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            Difference = (currentCamera.ScreenToWorldPoint(Input.mousePosition)) - currentCamera.transform.position;
            if(drag == false)
            {
                drag = true;
                Origin = currentCamera.ScreenToWorldPoint(Input.mousePosition);
            }

        }
        else
        {
            drag = false;
        }

        if (drag)
        {
            currentCamera.transform.position = Origin - Difference;// * 0.5f;
        }

        currentCamera.orthographicSize -= Input.mouseScrollDelta.y;
        if (currentCamera.orthographicSize < 1)
            currentCamera.orthographicSize = 1;
        if (currentCamera.orthographicSize > 10)
            currentCamera.orthographicSize = 10;


    }
}
