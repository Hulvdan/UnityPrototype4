#nullable enable
using System;
using System.Collections.Generic;
using BFG.Runtime.Controllers.HumanTransporter;
using BFG.Runtime.Graphs;
using UnityEngine;

namespace BFG.Runtime.Entities {
public class HumanTransporter {
    public HumanTransporter(Guid id, GraphSegment segment, Vector2Int currentPos) {
        ID = id;
        this.segment = segment;
        pos = currentPos;
        movingFrom = currentPos;
    }

    public readonly Guid ID;

    public GraphSegment? segment { get; set; }

    public Vector2Int pos { get; set; }
    public float movingElapsed { get; set; }
    public float movingProgress { get; set; }
    public Vector2 movingFrom { get; set; }
    public Vector2Int? movingTo { get; set; }

    public readonly List<Vector2Int> movingPath = new();

    public MainState? state { get; set; }

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

    public MovingInTheWorld.State? stateMovingInTheWorld { get; set; }

    #endregion

    #region HumanTransporter_MovingItem_Controller

    public MovingResources.State? stateMovingResource;

    public float stateMovingResource_pickingUpResourceElapsed;
    public float stateMovingResource_pickingUpResourceProgress;
    public float stateMovingResource_placingResourceElapsed;
    public float stateMovingResource_placingResourceProgress;

    public MapResource? stateMovingResource_targetedResource = null;

    #endregion
}
}
