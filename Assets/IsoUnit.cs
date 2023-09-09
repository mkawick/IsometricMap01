using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoUnit : MonoBehaviour
{
    public IsoUnitData data;
    public GameObject dataDisplay;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   /* private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit");
    }*/
    public void WriteData(bool show)
    {
        dataDisplay?.SetActive(show);
        if (show == true)
        {
            dataDisplay.GetComponent<IsoUnitStatsCanvasController>().SetPosition(transform.localPosition);
            //dataDisplay?.SetActive(false);
            // dataDisplay.transform.SetParent(this.transform);
           // dataDisplay.transform.localPosition = dataDisplay.transform.localPosition + new Vector3(-0.9f, 0.5f, 0f); // this offset seems great
        }
    }
}
