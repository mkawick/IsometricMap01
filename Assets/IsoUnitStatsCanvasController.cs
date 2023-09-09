using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnitStatsCanvasController : MonoBehaviour
{
    public Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.back, camera.transform.rotation * Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPosition(Vector3 worldPos)
    {
        transform.localPosition = worldPos + new Vector3(-0.9f, 0.5f, 0f);
    }
}
