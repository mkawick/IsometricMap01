using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResources : MonoBehaviour
{
    private Dictionary<ResourceType, float> resource;  // type quantity.. 
    public PlayerResources()
    {
        resource = new Dictionary<ResourceType, float>();
        resource[ResourceType.Wood] = 0;
        resource[ResourceType.Metal] = 0;
        resource[ResourceType.Prestige] = 0;
    }

    public Dictionary<ResourceType, float> Resource { get => resource; }

    public event Action<int, int, int> OnResourcesModifiedEvent;
    public void OnResourcesModified(ResourceType type, int quantity)
    {
        resource[type] = Mathf.Clamp(Resource[type] + quantity, 0, 1000000);
        OnResourcesModifiedEvent((int)Resource[ResourceType.Wood], (int)Resource[ResourceType.Metal], (int)Resource[ResourceType.Prestige]);
    }
}
