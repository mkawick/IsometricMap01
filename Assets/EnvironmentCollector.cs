using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public enum ResourceType
{
    Wood, Metal, Prestige, NumResources
}

public class EnvironmentCollector : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
       // destinationIcons[0].sprite = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Collect(Vector3 worldLocation, ResourceType type)
    {
        var screenLocation = Camera.main.WorldToScreenPoint(worldLocation);

        int imageIndex = (int)type;
        GameObject flyingSymbol = Instantiate(imagePrefab, storage.transform, false);
        flyingSymbol.transform.position = screenLocation;
        Image flyingSymbolImage = flyingSymbol.GetComponent<Image>();
        flyingSymbolImage.sprite = collectionIcons[imageIndex];

        var pointToTravel = destinationIcons[imageIndex].GetComponent<Image>().rectTransform.localPosition;
        flyingSymbol.transform.LeanMoveLocal(pointToTravel, iconFlyingTime).setEaseOutExpo();

        var call = DestroyImage(flyingSymbol);
        StartCoroutine(call);
    }

    IEnumerator DestroyImage(GameObject obj)
    {
        yield return new WaitForSeconds(iconFlyingTime);

        Destroy(obj);
    }
}
