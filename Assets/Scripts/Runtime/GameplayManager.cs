using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BFG.Runtime {
public class Resource {
    public int Amount;
    public string Codename;
}

public record ResourceChanged {
    public string Codename;
    public int NewAmount;
    public int OldAmount;
}

public class GameplayManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    Map _map;

    [Header("Setup")]
    [SerializeField]
    public UnityEvent<ResourceChanged> OnResourceChanged;

    readonly List<Resource> _resources = new();

    void Start() {
        _resources.Add(new Resource { Amount = 0, Codename = "wood" });
        _resources.Add(new Resource { Amount = 0, Codename = "stone" });
        _resources.Add(new Resource { Amount = 0, Codename = "food" });

        // OnResourceChanged?.Invoke(
        //     new ResourceChanged
        //         { NewAmount = 100, OldAmount = 0, Codename = _resources[0].Codename }
        // );
    }

    public void GiveResource(string codename, int amount) {
        var resource = _resources.Find(x => x.Codename == codename);
        resource.Amount += amount;
        OnResourceChanged?.Invoke(
            new ResourceChanged {
                NewAmount = resource.Amount,
                OldAmount = resource.Amount - amount,
                Codename = resource.Codename
            }
        );
    }
}
}
