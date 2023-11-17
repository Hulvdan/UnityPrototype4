using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class GraphSegment {
    public List<GraphVertex> Vertexes;
    public List<Vector2Int> MovementTiles;

    public GraphSegment(List<GraphVertex> vertexes, List<Vector2Int> movementTiles) {
        Vertexes = vertexes;
        MovementTiles = movementTiles;
    }
}
}
