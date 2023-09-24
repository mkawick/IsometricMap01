using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum ResourceType
{
    Wood, Metal, Prestige, NumResources
}

[System.Serializable]
public class ResourceTypeEntry // for costs and entries in classes
{
    public ResourceType type;
    public int cost;
}

public class ResourceData : MonoBehaviour
{
    public ResourceType type;
    public int quantity;
    public Action<ResourceType, int> OnResourcesFinishedFlying;
}

public class EnvironmentCollector : MonoBehaviour
{
    public const int kNoQuantityChange = -1;

    [SerializeField]
    Sprite [] collectionIcons = new Sprite[(int)ResourceType.NumResources];

    [SerializeField]
    Image[] destinationIcons;

    [SerializeField]
    GameObject imagePrefab;

    [SerializeField]
    GameObject storage;


    [SerializeField, Range(0.1f, 5)]
    float iconFlyingTime;

    public float Collect(Vector3 worldLocation, ResourceType type, int quantity, Action<ResourceType, int> OnResourcesFinishedFlying)
    {
        var screenLocation = Camera.main.WorldToScreenPoint(worldLocation);

        int imageIndex = (int)type;
        GameObject flyingSymbol = Instantiate(imagePrefab, storage.transform, false);        
        flyingSymbol.transform.position = screenLocation;
        Image flyingSymbolImage = flyingSymbol.GetComponent<Image>();
        flyingSymbolImage.sprite = collectionIcons[imageIndex];

        var resources = flyingSymbol.AddComponent<ResourceData>();
        resources.type = type;
        resources.quantity = quantity;
        resources.OnResourcesFinishedFlying = OnResourcesFinishedFlying;

        var pointToTravel = destinationIcons[imageIndex].GetComponent<Image>().rectTransform.localPosition;
        flyingSymbol.transform.LeanMoveLocal(pointToTravel, iconFlyingTime).setEaseOutExpo();

        var call = DestroyImage(flyingSymbol);
        StartCoroutine(call);

        return iconFlyingTime;
    }

    IEnumerator DestroyImage(GameObject flyingSymbol)
    {
        yield return new WaitForSeconds(iconFlyingTime);

        var resources = flyingSymbol.GetComponent<ResourceData>();
        resources.OnResourcesFinishedFlying?.Invoke(resources.type, resources.quantity);

       /* int wood = kNoQuantityChange, metal = kNoQuantityChange, prestige = kNoQuantityChange;
        switch(resources.type)
        {    
        case ResourceType.Wood:
            {
                wood = resources.quantity;
            }
            break;
        case ResourceType.Metal:
            {
                metal = resources.quantity;
            }
            break;
        case ResourceType.Prestige:
            {
                prestige = resources.quantity;
            }
            break;
        }
        resources.OnResourcesModified?.Invoke(wood, metal, prestige);*/
        Destroy(flyingSymbol);
    }
}
