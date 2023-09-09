using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private Vector3 Origin;
    private Vector3 Difference;
    private Vector3 ResetCamera;

    public Camera currentCamera;

    private bool drag = false;

    private IsoUnit oldIsoUnit = null;

    public static event Action<GameObject> OnGameObjectClicked;

    private void Start()
    {
        currentCamera = Camera.main;
        ResetCamera = currentCamera.transform.position;
        
    }

    private void Update()
    {
        //Check for mouse click 
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    if (oldIsoUnit != null)
                    {
                        oldIsoUnit.WriteData(false);
                        oldIsoUnit = null;
                    }

                    var obj = raycastHit.transform.gameObject;
                    var newIsoUnit = obj.GetComponent<IsoUnit>();
                    
                    if (newIsoUnit != null)
                    {
                        newIsoUnit.WriteData(true);
                        oldIsoUnit = newIsoUnit;
                    }
                    else 
                    {
                        OnGameObjectClicked?.Invoke(obj);
                    }
                }
                else
                {
                    OnGameObjectClicked?.Invoke(null);
                }
            }
            else
            {
                OnGameObjectClicked?.Invoke(null);
            }
        }
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
