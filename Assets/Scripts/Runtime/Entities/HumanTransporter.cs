using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class HumanTransporter {
    public HumanTransporter(Guid id, GraphSegment segment, Vector2Int currentPosition) {
        ID = id;
        this.segment = segment;
        position = currentPosition;
        movingFrom = currentPosition;
    }

    public readonly Guid ID;

    public GraphSegment segment { get; set; }

    public Vector2Int position { get; set; }
    public float movingElapsed { get; set; }
    public float movingNormalized { get; set; }
    public Vector2 movingFrom { get; set; }
    public Vector2Int? movingTo { get; set; }

    public Queue<Vector2Int> movingPath = new();

    public HumanTransporterState state { get; set; } = HumanTransporterState.MovingToCenter;
}
}
