using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering {
[Serializable]
public class MovementPattern {
    [FormerlySerializedAs("Feedbacks")]
    public List<MovementFeedback> feedbacks = new();
}
}
