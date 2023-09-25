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
    public void OnResourcesModified(float wood, float metal, float prestige)
    {
        resource[ResourceType.Wood] = Mathf.Clamp(Resource[ResourceType.Wood] + wood, 0, 1000000);
        resource[ResourceType.Metal] = Mathf.Clamp(Resource[ResourceType.Metal] + wood, 0, 1000000);
        resource[ResourceType.Prestige] = Mathf.Clamp(Resource[ResourceType.Prestige] + wood, 0, 1000000);
        OnResourcesModifiedEvent((int)Resource[ResourceType.Wood], (int)Resource[ResourceType.Metal], (int)Resource[ResourceType.Prestige]);
    }
}
