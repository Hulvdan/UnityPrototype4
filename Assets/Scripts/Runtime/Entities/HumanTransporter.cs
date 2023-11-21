using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class HumanTransporter {
    public HumanTransporter(Guid id, GraphSegment segment, Vector2Int currentPos) {
        ID = id;
        this.segment = segment;
        pos = currentPos;
        movingFrom = currentPos;
    }

    public readonly Guid ID;

    public GraphSegment segment { get; set; }

    public Vector2Int pos { get; set; }
    public float movingElapsed { get; set; }
    public float movingNormalized { get; set; }
    public Vector2 movingFrom { get; set; }
    public Vector2Int? movingTo { get; set; }

    public List<Vector2Int> movingPath = new();

    public HumanTransporterState state { get; set; } = HumanTransporterState.MovingToCenter;
}
}
