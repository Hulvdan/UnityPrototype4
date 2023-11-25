using System;
using UnityEngine;

namespace BFG.Runtime {
[Serializable]
public class RequiredResourceToBuild {
    [Min(1)]
    public int Number;

    public ScriptableResource Scriptable;
}
}
