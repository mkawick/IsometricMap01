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
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            if(TryMouseHitIsoUnits() == false)
            {
                if(TryMouseHitIsoBuildings() == false)
                {
                    gameUnitSelector.SelectUnit(null);
                }
            }
           /* RaycastHit raycastHit;
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    var obj = raycastHit.transform.gameObject;
                    gameUnitSelector.SelectUnit(obj);
                    return;
                }
                else
                {
                    gameUnitSelector.SelectUnit(null);
                }
            }
            else
            {
                gameUnitSelector.SelectUnit(null);
            }*/
        }
    }

    bool TryMouseHitIsoUnits()
    {
        RaycastHit raycastHit;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        int mask = (1 << LayerMask.NameToLayer("IsoUnit"));
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, mask))
        {
            if (raycastHit.transform != null)
            {
                var obj = raycastHit.transform.gameObject;
                gameUnitSelector.SelectUnit(obj);
                return true;
            }
            else
            {
                //gameUnitSelector.SelectUnit(null);
            }
        }
        else
        {
            //gameUnitSelector.SelectUnit(null);
        }
        return false;
    }

    bool TryMouseHitIsoBuildings()
    {
        RaycastHit raycastHit;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        int mask = (1 << LayerMask.NameToLayer("IsoBuilding"));
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, mask))
        {
            if (raycastHit.transform != null)
            {
                var obj = raycastHit.transform.gameObject;
                gameUnitSelector.SelectUnit(obj);
                return true;
            }
            else
            {
               // gameUnitSelector.SelectUnit(null);
            }
        }
        else
        {
            //gameUnitSelector.SelectUnit(null);
        }
        return false;
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
