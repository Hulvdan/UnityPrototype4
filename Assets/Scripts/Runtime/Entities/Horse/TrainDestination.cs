using System;
using UnityEngine;

namespace BFG.Runtime {
[Serializable]
public struct TrainDestination {
    public HorseDestinationType Type;
    public Vector2Int Pos;

    public override string ToString() {
        return $"TrainDestination({Pos}, {Type})";
    }
}
}
