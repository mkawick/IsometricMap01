using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum BuildingAbilities
{
    CreateCombatUnits, 
    CreateDefend,
    PopulationGrowth,
    CutWood,
    ForgeMetal,
    GeneratePrestige,
    EnhanceDefenceInArea,

}

[System.Serializable]
public class BuildingAbilitiesEntry // for costs and entries in classes
{
    public BuildingAbilities ability;
    public float strength;
}


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/IsoBuildingData", order = 2)]
public class IsoBuildingData : ScriptableObject
{
    public string unitName;
    [TextArea]
    public string description;

    [Tooltip("do not assign; assigned at runtime")]
    public GameObject isoBuilding;
    [Tooltip("do not assign; autogenerated")]
    public Sprite image;

    public ResourceTypeEntry[] costs;
    public BuildingAbilitiesEntry[] abilitiesConfig;

    public int Cost(ResourceType type)
    {
        if (costs == null)
            return 0;
        foreach (ResourceTypeEntry entry in costs)
        {
            if (entry.type == type)
            {
                return entry.cost;
            }
        }
        return 0;
    }
    public float Ability(BuildingAbilities type)
    {
        if (abilitiesConfig == null)
            return 0;
        foreach (var entry in abilitiesConfig)
        {
            if (entry.ability == type)
            {
                return entry.strength;
            }
        }
        return 0;
    }
    /*  public void Init(GameObject owner)
      {
          isoUnit = owner;
          // todo setup a sprite
      }



      */
}