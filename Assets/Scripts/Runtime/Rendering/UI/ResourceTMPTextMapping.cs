using System;
using TMPro;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering.UI {
[Serializable]
public class ResourceTMPTextMapping {
    [FormerlySerializedAs("Resource")]
    public ScriptableResource resource;

    [FormerlySerializedAs("Text")]
    public TMP_Text text;
}
}
