using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class ResourceCollector : MonoBehaviour
{
    float wood, metal, prestige;
    float woodMultiplier = 1, metalMultiplier = 1, prestigeMultiplier = 1;

    public event Action<int, int, int> OnResourcesModified;

    public void AddResources(EnvironmentCollector environmentCollector, int w, int m, int p)
    {
        wood += w * woodMultiplier; 
        metal += m * metalMultiplier; 
        prestige += p * prestigeMultiplier;
        //OnResourcesModified?.Invoke((int)wood, (int)metal, (int)prestige);
        environmentCollector.Collect(transform.position, ResourceType.Wood, (int) wood, OnResourcesModified);
        environmentCollector.Collect(transform.position, ResourceType.Metal, (int) metal, OnResourcesModified);
        environmentCollector.Collect(transform.position, ResourceType.Prestige, (int) prestige, OnResourcesModified);
    }
    public void UseResources(int w, int m, int p)
    {
        wood = Mathf.Clamp(wood - w, 0, 500);
        metal = Mathf.Clamp(metal - m, 0, 500);
        prestige = Mathf.Clamp(prestige - p, 0, 500);
        OnResourcesModified?.Invoke((int)wood, (int)metal, (int)prestige);
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            var playerUnitController = GetComponent<PlayerUnitController>(); 
            var playerTurnTaker = GetComponent<PlayerTurnTaker>();
            if (playerTurnTaker.IsHuman)
            {

                if (Keyboard.current.cKey.isPressed)
                {
                    AddResources(1, 3, 5);
                }
                else if (Keyboard.current.rKey.isPressed)
                {
                    UseResources(2, 2, 2);
                }
                else if (Keyboard.current.tKey.isPressed)
                {
                    GameObject.FindAnyObjectByType<EnvironmentCollector>().Collect(this.transform.position, ResourceType.Wood);
                }
            }
        }
    }*/

    public void ControlledUpdate()
    {
        //var buildingsToUpdate = GetComponent<PlayerTurnTaker>().BuildingsIOwn;
    }

    public bool AmIDoneCollecting() { return true; }
}
