using System;
using TMPro;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering.UI {
[Serializable]
public class ResourceTMPTextMapping {
    public ScriptableResource resource;

    public TMP_Text text;
}
}
