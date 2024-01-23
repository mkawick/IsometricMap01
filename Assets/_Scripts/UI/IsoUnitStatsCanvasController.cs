using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//class UnityEditor.UIElements;


public class IsoUnitStatsCanvasController : MonoBehaviour
{
    public Camera mainCamera;
    [SerializeField]
    private TMP_Text Name;
    [SerializeField]
    private TMP_Text Description;
    [SerializeField]
    private Image Image;

    void Start()
    {
        if(mainCamera == null)
            mainCamera = Camera.main;
        // set the initial camera lookat
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);

        this.gameObject.SetActive(false);
    }

    public void SetPosition(Vector3 worldPos)
    {
        transform.localPosition = worldPos + new Vector3(-0.9f, 0.5f, 0f);
    }

    public void Set(IsoUnitData data, GameObject isoObj)
    {
        this.gameObject.SetActive(true);
        Name.text = data.name;
        Description.text = data.description;

        var oldLayer = isoObj.transform.gameObject.layer;
        int defaultLayer = LayerMask.NameToLayer("Default");
        isoObj.transform.gameObject.layer = defaultLayer;

        var texture2d = RuntimePreviewGenerator.GenerateModelPreview(isoObj.transform, 128, 128, true, true);
        var sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f), 100.0f);
        Image.sprite = sprite;
        isoObj.transform.gameObject.layer = oldLayer;
    }

    public void Set(IsoBuildingData data, GameObject isoObj)
    {
        this.gameObject.SetActive(true);
        Name.text = data.name;
        Description.text = data.description;

        var oldLayer = isoObj.transform.gameObject.layer;
        int defaultLayer = LayerMask.NameToLayer("Default");
        isoObj.transform.gameObject.layer = defaultLayer;

        var texture2d = RuntimePreviewGenerator.GenerateModelPreview(isoObj.transform, 128, 128, true, true);
        var sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f), 100.0f);
        Image.sprite = sprite;
        isoObj.transform.gameObject.layer = oldLayer;
    }

    public void Clear()
    {
        this.gameObject.SetActive(false);
    }
}
