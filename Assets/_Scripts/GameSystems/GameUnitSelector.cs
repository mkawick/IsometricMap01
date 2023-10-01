using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUnitSelector : MonoBehaviour
{
    private IsoUnit oldIsoUnit = null;
    private IsoBuilding oldIsoBuilding = null;

    public static event Action<GameObject> OnGameObjectClicked;

    public void SelectUnit(GameObject obj)
    {
        if (obj == null)
        {
            if (oldIsoUnit != null)
            {
                oldIsoUnit.Selected(false);
                oldIsoUnit = null;
            }
            if(oldIsoBuilding != null)
            {
                oldIsoBuilding.Selected(false);
                oldIsoBuilding = null;
            }
            OnGameObjectClicked?.Invoke(null);
        }
        else
        {
            var newIsoUnit = obj.GetComponent<IsoUnit>();
            if(newIsoUnit)
            {                
                if (oldIsoUnit == newIsoUnit)// already selected
                {
                    return;
                }
                newIsoUnit.Selected(true);
                oldIsoUnit?.Selected(false);
                OnGameObjectClicked?.Invoke(obj);
                oldIsoUnit = newIsoUnit;
                return;
            }

            var newIsoBuilding = obj.GetComponent<IsoBuilding>();
            if (newIsoBuilding)
            {
                if (oldIsoBuilding == newIsoBuilding)// already selected
                {
                    return;
                }
                newIsoBuilding.Selected(true); // todo
                oldIsoBuilding?.Selected(false);
                OnGameObjectClicked?.Invoke(obj);
                oldIsoBuilding = newIsoBuilding;
                return;
            }

            if(oldIsoUnit != null)
            {  
                oldIsoUnit.Selected(false);
                oldIsoUnit = null;
            }
            if (oldIsoBuilding != null)
            {
                oldIsoBuilding.Selected(false);
                oldIsoBuilding = null;
            }
            OnGameObjectClicked?.Invoke(null);
        }
    }
    /*
    var obj = raycastHit.transform.gameObject;
    var newIsoUnit = obj.GetComponent<IsoUnit>();
    if(oldIsoUnit != newIsoUnit)
    {
        if (oldIsoUnit != null)
        {
            oldIsoUnit.Selected(false);
            oldIsoUnit = null;
        }

        if (newIsoUnit != null)
        {
            newIsoUnit.Selected(true);
            oldIsoUnit = newIsoUnit;
        }
        else
        {
            OnGameObjectClicked?.Invoke(obj);
        }
    }
     * */
}
