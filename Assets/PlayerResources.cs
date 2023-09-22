using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    public Dictionary<ResourceType, float> resource;  // type quantity.. 
    public PlayerResources()
    {  
        resource = new Dictionary<ResourceType, float>();
        resource[ResourceType.Wood] = 0; 
        resource[ResourceType.Metal] = 0; 
        resource[ResourceType.Prestige] = 0;
    }
    public event Action<int, int, int> OnResourcesModifiedEvent;
    public void OnResourcesModified(ResourceType type, int quantity)
    {
        resource[type] = Mathf.Clamp(resource[type] + quantity, 0, 1000000);
        OnResourcesModifiedEvent((int)resource[ResourceType.Wood], (int)resource[ResourceType.Metal], (int)resource[ResourceType.Prestige]);
    }
}
