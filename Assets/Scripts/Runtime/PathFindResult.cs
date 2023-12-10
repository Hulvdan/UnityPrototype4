using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
public struct PathFindResult {
    public bool Success;

    [FormerlySerializedAs("Path")]
    public List<Vector2Int> Value;

    public PathFindResult(bool success, List<Vector2Int> value) {
        Success = success;
        Value = value;
    }
}
}
