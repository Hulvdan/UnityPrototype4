using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class RequiredResourceToBuild {
    [Min(1)]
    public int number;

    public ScriptableResource scriptable;
}
}
