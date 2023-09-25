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

    private PlayerTurnTaker currentPurchasePlayer;

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
        
        PlayerResources resources = currentPurchasePlayer.GetComponent<PlayerResources>();
        if(ValidatePlayerHasEnoughResources(resources, buildingData) == false)
        {
            Debug.LogError("not enough resources");
            return;
        }
        if(currentPurchasePlayer.GetComponent<Construction>().Build(buildingData) == false)
        {
            Debug.LogError("failed to build");
            return;
        }

        foreach (var purchaseOption in purchaseOptions)
        {
            // update all options based on current player resources
            purchaseOption.GetComponent<BuildingButton_UI>().UpdateButtonState(resources);
        }
        // 
    }

    bool ValidatePlayerHasEnoughResources(PlayerResources resources, IsoBuildingData buildingData)
    {
        if (resources.Resource[ResourceType.Wood] < buildingData.Cost(ResourceType.Wood))
        {
            return false;
        }
        if (resources.Resource[ResourceType.Metal] < buildingData.Cost(ResourceType.Metal))
        {
            return false;
        }
        if (resources.Resource[ResourceType.Prestige] < buildingData.Cost(ResourceType.Prestige))
        {
            return false;
        }
        return true;
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
            currentPurchasePlayer = null;
        }
        else
        {
            var isoUnit = obj.GetComponent<IsoUnit>();
            if(isoUnit != null)
            {
                if(isoUnit.Data.GetAbility(UnitAbilities.Construction) != 0)
                {
                    currentPurchasePlayer = isoUnit.playerOwner;
                    var constructables = currentPurchasePlayer.GetComponent<Construction>().Constructables;
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
