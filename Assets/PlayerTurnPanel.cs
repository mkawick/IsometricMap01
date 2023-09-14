using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnPanel : MonoBehaviour
{
    public Image [] panelsToControl;
    public float xOffsetDeselected = 58;
    public float imageWidthDeselected = 100;
    public float xOffsetSelected = 88;
    public float imageWidthSelected = 160;
    int numPlayers;

    public void SetNumPlayers(int num)
    {
        numPlayers = num;
        for (int i = 0; i < num; i++)
        {
            panelsToControl[i].gameObject.SetActive(true);
        }
        for (int i = num; i < panelsToControl.Length; i++)
        {
            panelsToControl[i].gameObject.SetActive(false);
        }
        DeselectAllPlayers();
    }
    void DeselectAllPlayers()
    {
        for (int i = 0; i < numPlayers; i++)
        {
            var pos = panelsToControl[i].rectTransform.anchoredPosition;
            pos.x = xOffsetDeselected;
            panelsToControl[i].rectTransform.anchoredPosition = pos;
            var dims = panelsToControl[i].rectTransform.sizeDelta;
            panelsToControl[i].rectTransform.sizeDelta = new Vector2(imageWidthDeselected, dims.y);
        }
    }
    public void SetActivePlayer(int index)
    {
        if(index >= numPlayers)
        {
            return;
        }
        DeselectAllPlayers();
        var pos = panelsToControl[index].rectTransform.anchoredPosition;
        pos.x = xOffsetSelected;
        panelsToControl[index].rectTransform.anchoredPosition = pos;
        var dims = panelsToControl[index].rectTransform.sizeDelta;
        panelsToControl[index].rectTransform.sizeDelta = new Vector2(imageWidthSelected, dims.y);
    }

    public void SetPlayerName(string name, int whichPlayer)
    {
        var text = panelsToControl[whichPlayer].gameObject.GetComponentInChildren<TMPro.TMP_Text>();
        text.text = name;
    }
    /* // Start is called before the first frame update
     void Start()
     {

     }

     // Update is called once per frame
     void Update()
     {

     }*/
}
