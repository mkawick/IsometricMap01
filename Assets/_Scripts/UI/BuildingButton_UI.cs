using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton_UI : MonoBehaviour
{
    [SerializeField]
    Image thumbnail;
    [SerializeField]
    TMP_Text buildingName;
    [SerializeField]
    TMP_Text buildingDescription;
    [SerializeField]
    TMP_Text woodText;
    [SerializeField]
    TMP_Text metalText;
    [SerializeField]
    TMP_Text prestigeText;

    [SerializeField]
    GameObject player;

    IsoBuildingData buildingData;
    UI_BuildingOptionsPanel uI_BuildingOptionsPanel;

    public IsoBuildingData BuildingData { get => buildingData; set => buildingData = value; }
    public TMP_Text BuildingName { get => buildingName; set => buildingName = value; }
    public TMP_Text BuildingDescription { get => buildingDescription; set => buildingDescription = value; }
    public void SetWood(int wood) { woodText.text = wood.ToString(); }
    public void SetMetal(int metal) { metalText.text = metal.ToString(); }
    public void SetPrestige(int prestige) { prestigeText.text = prestige.ToString(); }

    public void Init(UI_BuildingOptionsPanel panel, PlayerResources resources, GameObject gameObject)
    {
        uI_BuildingOptionsPanel = panel;
        UpdateButtonState(resources);

       /* var oldLayer = gameObject.layer;
        int defaultLayer = LayerMask.NameToLayer("Default");
        gameObject.layer = defaultLayer;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = defaultLayer;
        }*/

        var texture2d = RuntimePreviewGenerator.GenerateModelPreview(gameObject.transform, 128, 128, true, true);
        var sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f), 100.0f);
        thumbnail.sprite = sprite;

      /*  gameObject.layer = oldLayer;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = oldLayer;
        }*/
    }

    public void UpdateButtonState(PlayerResources resources)
    {
        bool isValid = true;
        if (resources.Resource[ResourceType.Wood] < buildingData.Cost(ResourceType.Wood))
        {
            isValid = false;
            woodText.color = Color.red;
        }
        else
        {
            woodText.color = Color.black;
        }
        if (resources.Resource[ResourceType.Metal] < buildingData.Cost(ResourceType.Metal))
        {
            isValid = false;
            metalText.color = Color.red;
        }
        else
        {
            metalText.color = Color.black;
        }
        if (resources.Resource[ResourceType.Prestige] < buildingData.Cost(ResourceType.Prestige))
        {
            isValid = false;
            prestigeText.color = Color.red;
        }
        else
        {
            prestigeText.color = Color.black;
        }
        //GetComponent<Image>().color = isValid ? Color.white : Color.gray;
    }

    public void Purchase()
    {
        var rc = player.GetComponent<PlayerResources>();
        var wood = rc.Resource[ResourceType.Wood];
        var metal = rc.Resource[ResourceType.Metal];
        var prestige = rc.Resource[ResourceType.Prestige];

        uI_BuildingOptionsPanel.PurchasePressed(buildingData);
    }

}
