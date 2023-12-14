using System;
using UnityEngine.Serialization;

namespace BFG.Runtime.Localization {
[Serializable]
public class GStringKey {
    [FormerlySerializedAs("Key")]
    public string key;
}
}
