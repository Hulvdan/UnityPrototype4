using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public struct PathFindResult {
    public bool success;

    public List<Vector2Int> value;

    public PathFindResult(bool success_, List<Vector2Int> value_) {
        success = success_;
        value = value_;
    }
}
}
