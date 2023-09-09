using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/IsoUnitData", order = 2)]
public class IsoUnitData : ScriptableObject
{
    public string name;
    public GameObject isoUnit;
    //public int price;
    [TextArea]
    public string description;
    public Sprite image;

    public void Init(GameObject owner)
    {
        isoUnit = owner;
        // todo setup a sprite
    }
}