using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerResources : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text woodQuantityText;
    [SerializeField]
    TMPro.TMP_Text metalQuantityText;
    [SerializeField]
    TMPro.TMP_Text prestigeQuantityText;
    // Start is called before the first frame update
    void Start()
    {
        woodQuantityText.text = "0";
        metalQuantityText.text = "0";
        prestigeQuantityText.text = "0";
    }

    public void SetupResourceCollector(PlayerResources playerResources)
    {
        playerResources.OnResourcesModifiedEvent += OnResourcesModified;
    }

    public void GameEnd(PlayerResources playerResources)
    {
        playerResources.OnResourcesModifiedEvent -= OnResourcesModified;
    }

    void OnResourcesModified(int wood, int metal, int prestige)
    {
        if(wood != EnvironmentCollector.kNoQuantityChange)
            woodQuantityText.text = wood.ToString();

        if (metal != EnvironmentCollector.kNoQuantityChange)
            metalQuantityText.text = metal.ToString();

        if (prestige != EnvironmentCollector.kNoQuantityChange)
            prestigeQuantityText.text = prestige.ToString();
    }

  /*  // Update is called once per frame
    void Update()
    {
        
    }
    */

}
