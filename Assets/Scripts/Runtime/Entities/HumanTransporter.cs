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

    public HumanTransporterState? state { get; set; }

    public void AddPath(List<Vector2Int> path) {
        movingPath.Clear();

        var isFirst = true;
        foreach (var tile in path) {
            if (isFirst) {
                isFirst = false;

                if (tile != (movingTo ?? pos)) {
                    movingPath.Add(tile);
                }

                continue;
            }

            movingPath.Add(tile);
        }

        if (movingTo == null) {
            PopMovingTo();
        }
    }

    public void PopMovingTo() {
        if (movingPath.Count == 0) {
            movingElapsed = 0;
            movingTo = null;
        }
        else {
            movingTo = movingPath[0];
            movingPath.RemoveAt(0);
        }
    }

    #region HumanTransporter_MovingInTheWorld_Controller

    public HumanTransporter_MovingInTheWorld_Controller.State? stateMovingInTheWorld { get; set; }

    #endregion

    #region HumanTransporter_MovingInsideSegment_Controller

    public HumanTransporter_MovingInsideSegment_Controller.State? stateMovingInsideSegment {
        get;
        set;
    }

    #endregion

    #region HumanTransporter_MovingItem_Controller

    public HumanTransporter_MovingItem_Controller.State? stateMovingItem;
    public bool justStartedMovingItem;

    public float stateMovingItem_pickingUpResourceElapsed;
    public float stateMovingItem_pickingUpResourceNormalized;
    public float stateMovingItem_placingResourceElapsed;
    public float stateMovingItem_placingResourceNormalized;

    public MapResource? targetedResource = null;

    #endregion
}
}
