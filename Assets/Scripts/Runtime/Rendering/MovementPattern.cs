using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering {
[Serializable]
public class MovementPattern {
    public List<MovementFeedback> feedbacks = new();
}
}
