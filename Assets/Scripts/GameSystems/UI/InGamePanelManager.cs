using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.UI;

public class InGamePanelManager : MonoBehaviour
{
    enum Panels { Tile, Unit, Building, Resource };

    [SerializeField]
    private EnumMap<Panels, GameObject[]> panelMap;

    public Camera renderCamera;

    public void OnGUI()
    {
        foreach (var array in panelMap)
        {
            // Do something with the whole array of game objects
            // in each enum value slot.
        }
    }
    void OnValidate()
    {
        panelMap.TryRevise();
    }

    void Reset()
    {
        panelMap.TryRevise();
    }

    // Start is called before the first frame update
    void Start()
    {
        ShowAllPanels(false);
        //OnItemSelected += OnItemSelectedMethod;
        GameModeManager.OnGameObjectClicked += OnItemSelectedMethod;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ShowAllPanels(bool isShowing)
    {
        foreach (var arrayOfItems in panelMap)
        {
            foreach (var item in arrayOfItems)
            {
                item.SetActive(isShowing);
            }
        }
    }

    void ShowPanels(Panels panels, string textToShow)
    {
        var selectedPanel = panelMap[panels];
        {
            foreach (var item in selectedPanel)
            {
                item.SetActive(true);
                var text = item.GetComponentInChildren<TMPro.TMP_Text>();
                if (text == null)
                    continue;
                text.SetText(textToShow);
            }
        }
        foreach (var panel in panelMap)
        {
            if (panel == selectedPanel)
                continue;

            foreach (var item in panel)
            {
                item.SetActive(false);
            }
        }
    }
    void RenderThumbnailPanels(Panels panels, Texture2D texture)
    {
        if (texture == null)
            return;

        var arrayOfItems = panelMap[panels];
        {

            foreach (var item in arrayOfItems)
            {
                foreach (Transform child in item.transform)
                {
                    var image = child.GetComponentInChildren<Image>();
                    if (image == null)
                        continue;
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    image.sprite = sprite;
                    
                }
            }
        }
    }

   /* private (Vector3 center, float size) CalculateOrthoSize()
    {
        var bounds 
    }*/
    Sprite IconMaker(GameObject target)
    {
        //var camera = GetComponent<Camera>();
        var camera = renderCamera;
        camera.orthographicSize = target.GetComponent<Renderer>().bounds.extents.y + 0.1f; // create CSS-like padding

        int resX = camera.pixelWidth;
        int resY = camera.pixelHeight;

        int clipX = 0, clipY = 0;

        if(resX > resY) { clipX = resX - resY; }
        if (resY > resX) { clipY = resY - resX; }

        Texture2D texture = new Texture2D(resX - clipX, resY - clipY, TextureFormat.RGBA32, false);
        RenderTexture renderTexture = new RenderTexture(resX, resY, 24);

        camera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;

        camera.Render();// grab icon
        texture.ReadPixels(new Rect(0, 0, resX, resY), 0, 0);
        texture.Apply();

        //var sprite = 

        camera.targetTexture = null;
        RenderTexture.active = null;

        Destroy(renderTexture);

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    void OnItemSelectedMethod(GameObject sender)
    {
        if(sender == null)
        {
            ShowAllPanels(false);
            return;
        }
        var tile = sender.GetComponent<TileBehavior>();
        if (tile)
        {
            ShowPanels(Panels.Tile, sender.name);

            var texture = AssetPreview.GetAssetPreview(sender);
            RenderThumbnailPanels(Panels.Tile, texture);
           // RenderThumbnailPanels(Panels.Tile, IconMaker(sender));
        }
    }
}
