using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class UIManager : MonoBehaviour {
    readonly List<IDisposable> _dependencyHooks = new();

    IMap _map;

    public void InitDependencies(IMap map) {
        _map = map;
        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();
    }
}
}
