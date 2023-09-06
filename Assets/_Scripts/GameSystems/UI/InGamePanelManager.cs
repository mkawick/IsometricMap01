using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEngine.UI;

public class InGamePanelManager : MonoBehaviour
{
    enum Panels { Tile, Unit, Building, Resource };

    [SerializeField]
    private EnumMap<Panels, GameObject[]> panelMap;
    [SerializeField]
    MapGenerator mapGenerator;

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

    void Start()
    {
        ShowAllPanels(false);
        GameModeManager.OnGameObjectClicked += OnItemSelectedMethod;
    }

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
        DisplayMainParentPanel(isShowing);
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
        DisplayMainParentPanel(true);
    }

    void DisplayMainParentPanel(bool isShowing)
    {
        this.transform.gameObject.SetActive(isShowing);
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


    void OnItemSelectedMethod(GameObject sender)
    {
        if(sender == null)
        {
            ShowAllPanels(false);
            return;
        }

        List<GameObject> objs = mapGenerator.GetAllObjectsOnTile(sender);
        var tile = sender.GetComponent<TileBehavior>();
        if (tile)
        {
            GameObject objToRender = sender;
            if (objs.Count > 0)
            {
                objToRender = objs[0];
            }
            ShowPanels(Panels.Tile, sender.name);
            RuntimePreviewGenerator.OrthographicMode = true;
            RuntimePreviewGenerator.BackgroundColor = new Color(0,0,0,0);
            RenderThumbnailPanels(Panels.Tile, RuntimePreviewGenerator.GenerateModelPreview(objToRender.transform, 128, 128, true, true));
        }
    }
}
