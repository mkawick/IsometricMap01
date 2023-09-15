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
    void DeselectAllPlayers(int avoidIndex = -1)
    {
        for (int i = 0; i < numPlayers; i++)
        {
            if (i == avoidIndex)
                continue;
            
            var pos = panelsToControl[i].rectTransform.anchoredPosition;
            pos.x = xOffsetDeselected;
            var dims = panelsToControl[i].rectTransform.sizeDelta;
            panelsToControl[i].GetComponent<UI_SlidingPanel>().SlideOut(pos, new Vector2(imageWidthDeselected, 1));
        }
    }
    public void SetActivePlayer(int index)
    {
        if(index >= numPlayers)
        {
            return;
        }
        DeselectAllPlayers(index);
        var pos = panelsToControl[index].rectTransform.anchoredPosition;
        pos.x = xOffsetSelected;
        var dims = panelsToControl[index].rectTransform.sizeDelta;
        panelsToControl[index].GetComponent<UI_SlidingPanel>().SlideIn(pos, new Vector2(imageWidthSelected, 1));
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
