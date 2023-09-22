using System;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceCollector : MonoBehaviour
{
    float woodMultiplier = 1, metalMultiplier = 1, prestigeMultiplier = 1;

    public event Action<ResourceType, int> NotifyResourcesUpdated;

    public void Register(PlayerResources playerResources)
    {
        NotifyResourcesUpdated += playerResources.OnResourcesModified;
    }

    public void AddResources(EnvironmentCollector environmentCollector, int w, int m, int p)
    {
        float wood = (float)(w * woodMultiplier);
        float metal = (float)(m * metalMultiplier);
        float prestige = (float)(p * prestigeMultiplier);
        if (environmentCollector != null)
        {
            environmentCollector.Collect(transform.position, ResourceType.Wood, (int)wood, NotifyResourcesUpdated);
            environmentCollector.Collect(transform.position, ResourceType.Metal, (int)metal, NotifyResourcesUpdated);
            environmentCollector.Collect(transform.position, ResourceType.Prestige, (int)prestige, NotifyResourcesUpdated);
        }
    }
    public void UseResources(int w, int m, int p)
    {
        NotifyResourcesUpdated(ResourceType.Wood, -w);
        NotifyResourcesUpdated(ResourceType.Metal, -m);
        NotifyResourcesUpdated(ResourceType.Prestige, -p);
    }

    public void ControlledUpdate()
    {
        //var buildingsToUpdate = GetComponent<PlayerTurnTaker>().BuildingsIOwn;
    }

    public bool AmIDoneCollecting() { return true; }
}
