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

    //GameModeManager gameModeManager;
    public static event Action<GameObject> OnGameObjectClicked;

    private void Start()
    {
        currentCamera = Camera.main;
        ResetCamera = currentCamera.transform.position;

        //gameModeManager = transform.parent.GetComponent<GameModeManager>();
        
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
                    //Our custom method. 
                    //CurrentClickedGameObject(raycastHit.transform.gameObject);
                    
                    OnGameObjectClicked?.Invoke(raycastHit.transform.gameObject);
                }
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

        //Vector3 pos = sphere.position;

        currentCamera.orthographicSize -= Input.mouseScrollDelta.y;
        if (currentCamera.orthographicSize < 1)
            currentCamera.orthographicSize = 1;
        if (currentCamera.orthographicSize > 10)
            currentCamera.orthographicSize = 10;
        //Debug.Log(currentCamera.orthographicSize);
       //sphere.position = pos;

            /* if (Input.GetMouseButton(1))
                 currentCamera.transform.position = ResetCamera;*/

    }
}
