using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TechTreeNode", order = 1)]
public class TechTreeNode : ScriptableObject
{
    public string tt_name;
    public string tt_description;
    public GameObject objToRender;
    public TechTreeNode[] dependecies;
    public bool unlocked;

    // bonuses
    public float foodProduction;
    public float bronzeProduction;
    public float constructionSpeed;
    public float constructionMaterialsEfficiency;
    public float moveSpeed;

    public enum Metals { None, Copper, Bronze, Brass, Iron };
    public Metals metallurgy;
    public enum ConstructionTypes { Straw, Wood, Clay, Rock, Mortar };
    public ConstructionTypes constructionTypes;
    // things that I can build
    // new troops
    // new weapon
}