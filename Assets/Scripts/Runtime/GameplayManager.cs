using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BFG.Runtime {
public struct Resource {
    public string Codename;
    public int Amount;
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

    List<Resource> _resources = new();

    void Awake() {
    }

    void Update() {
    }
}
}
