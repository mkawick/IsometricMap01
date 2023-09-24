using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsoBuilding : MonoBehaviour
{
    [SerializeField]
    private IsoBuildingData data;

    [HideInInspector]
    public PlayerTurnTaker playerOwner;

    [SerializeField]
    public GameObject modelToRender;

    public IsoBuildingData Data { get => data; set => data = value; }
}
