using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class GraphVertex {
    public List<ResourceObj> Resources;
    public Vector2Int Pos;

    public GraphVertex(List<ResourceObj> resources, Vector2Int pos) {
        Resources = resources;
        Pos = pos;
    }
}
}
