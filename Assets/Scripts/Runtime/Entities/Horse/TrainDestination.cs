using System;
using UnityEngine;

namespace BFG.Runtime {
[Serializable]
public struct TrainDestination {
    public TrainDestinationType Type;
    public Vector2Int Pos;
}
}
