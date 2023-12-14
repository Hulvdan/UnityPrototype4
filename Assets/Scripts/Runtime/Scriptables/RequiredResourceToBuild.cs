using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class RequiredResourceToBuild {
    [FormerlySerializedAs("Number")]
    [Min(1)]
    public int number;

    [FormerlySerializedAs("Scriptable")]
    public ScriptableResource scriptable;
}
}
