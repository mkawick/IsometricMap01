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
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Purchase()
    {
        var rc = player.GetComponent<PlayerResources>();
        var wood = rc.resource[ResourceType.Wood];
        var metal = rc.resource[ResourceType.Metal];
        var prestige = rc.resource[ResourceType.Prestige];
    }

}
