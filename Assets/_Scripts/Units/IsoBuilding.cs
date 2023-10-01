using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoBuilding : MonoBehaviour
{
    [SerializeField]
    private IsoBuildingData data;
    [Tooltip("Main GameObject that will be clicked on and scaled")]
    public GameObject gameModel;
    Vector3 originalScale;

    [HideInInspector]
    public PlayerTurnTaker playerOwner;

    bool isScaled = false;
    public IsoUnitStatsCanvasController dataDisplay;

    public IsoBuildingData Data { get => data; set => data = value; }

    public void Start()
    {
        originalScale = gameModel.transform.localScale;
    }
    public void Selected(bool isSelected)
    {
        dataDisplay?.gameObject.SetActive(isSelected);
        if (isSelected == false)
        {
            ScaleSelected(false);
        }
        if (isSelected == true)
        {
            var display = dataDisplay?.gameObject.GetComponent<IsoUnitStatsCanvasController>();
            display.SetPosition(transform.localPosition);
            display.Set(Data, this.transform.gameObject);
            ScaleSelected(true);
        }
    }

    void ScaleSelected(bool scaleUp)
    {
        if (scaleUp == false)
        {
            if (isScaled)
            {
                isScaled = false;
                gameModel.transform.localScale = originalScale;
                RemoveOutline();
            }
        }
        else //if (scaleUp == true)
        {
            if (isScaled == false)
            {
                isScaled = true;
                gameModel.transform.localScale = originalScale * 2;// new Vector3(2, 2, 2);
                AddOutline();
            }
        }
    }
    void AddOutline()
    {
        if (gameObject.GetComponent<Outline>() == null)
        {
            var outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 2f;
        }
    }
    void RemoveOutline()
    {
        var outline = gameObject.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }
}
