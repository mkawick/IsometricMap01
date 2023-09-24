using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class UI_BuildingOptionsPanel : MonoBehaviour
{
    [SerializeField]
    private GameObject purchaseOptionPrefab;
    [SerializeField]
    private GameObject optionsParent;

    private List<GameObject> purchaseOptions;

    //public event Action<IsoBuilding> OnClick;

    void Start()
    {
        Debug.Assert(purchaseOptionPrefab != null);
        Debug.Assert(optionsParent != null);
        //Debug.Assert(purchaseOptions != null);
        purchaseOptions = new List<GameObject>();

        GameUnitSelector.OnGameObjectClicked += SelectUnit;
        transform.gameObject.SetActive(false);
        purchaseOptions = new List<GameObject>();
    }

    public void PurchasePressed(IsoBuildingData buildingData)
    {
        Debug.Log(buildingData.unitName);
        // validate position
        // validate resources

        foreach(var purchaseOption in purchaseOptions)
        {
            // update all options based on current player resources
        }
        // 
    }

    void SelectUnit(GameObject obj)
    {
        if(obj == null)
        {
            transform.gameObject.SetActive(false);
            foreach(var opt in purchaseOptions)
            {
                Destroy(opt.gameObject);
            }
            purchaseOptions.Clear();
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
                        var option = Instantiate(purchaseOptionPrefab);
                        option.transform.SetParent(optionsParent.transform, false);
                        option.gameObject.SetActive(true);

                        BuildingButton_UI buttonDeets = option.GetComponent<BuildingButton_UI>();

                        var isoBuilding = constructable.GetComponent<IsoBuilding>();
                        var buildingData = isoBuilding.Data;
                        buttonDeets.BuildingName.text = buildingData.unitName;

                        buttonDeets.BuildingDescription.text = buildingData.description;
                        buttonDeets.SetWood(buildingData.Cost(ResourceType.Wood));
                        buttonDeets.SetMetal(buildingData.Cost(ResourceType.Metal));
                        buttonDeets.SetPrestige(buildingData.Cost(ResourceType.Prestige));
                        buttonDeets.BuildingData = buildingData;
                        buttonDeets.Init(this, isoUnit.playerOwner.GetComponent<PlayerResources>(), isoBuilding.modelToRender);
                        purchaseOptions.Add(option);

                        //Debug.Log(constructable.name);
                    }
                    transform.gameObject.SetActive(true);
                }
            }
        }
    }
}
