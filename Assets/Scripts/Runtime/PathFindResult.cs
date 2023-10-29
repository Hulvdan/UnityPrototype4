using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public struct PathFindResult {
    public bool Success;
    public List<Vector2Int> Path;

    public PathFindResult(bool success, List<Vector2Int> path) {
        Success = success;
        Path = path;
    }
}
}
