using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class UI_BuildingOptionsPanel : MonoBehaviour
{
    void Start()
    {
        GameUnitSelector.OnGameObjectClicked += SelectUnit;
        transform.gameObject.SetActive(false);
    }


    void SelectUnit(GameObject obj)
    {
        if(obj == null)
        {
            transform.gameObject.SetActive(false);
        }
        else
        {
            var isoUnit = obj.GetComponent<IsoUnit>();
            if(isoUnit != null)
            {
                if(isoUnit.Data.GetAbility(UnitAbilities.Construction) != 0)
                {
                    var constructables = isoUnit.playerOwner.GetComponent<Construction>().Constructables;
                    foreach( var constructable in constructables ) 
                    {
                        Debug.Log(constructable.name);
                    }
                    transform.gameObject.SetActive(true);
                }
            }
        }
    }
}
