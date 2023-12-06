#nullable enable
using System;
using System.Collections.Generic;
using BFG.Runtime.Controllers.HumanTransporter;
using BFG.Runtime.Graphs;
using UnityEngine;

namespace BFG.Runtime.Entities {
public class HumanBuilder {
    readonly Guid ID;

    public HumanBuilder(Guid id, Building building, Vector2Int currentPos) {
        ID = id;
    }
}

public class HumanMovingComponent {
    public Vector2Int pos { get; set; }
    public float elapsed { get; set; }
    public float progress { get; set; }
    public Vector2 from { get; set; }
    public Vector2Int? to { get; set; }

    public readonly List<Vector2Int> path = new();

    public HumanMovingComponent(Vector2Int initialPosition) {
        pos = initialPosition;
        from = initialPosition;
    }

    public void AddPath(List<Vector2Int> path) {
        this.path.Clear();

        var isFirst = true;
        foreach (var tile in path) {
            if (isFirst) {
                isFirst = false;

                if (tile != (to ?? pos)) {
                    this.path.Add(tile);
                }

                continue;
            }

            this.path.Add(tile);
        }

        if (to == null) {
            PopMovingTo();
        }
    }

    public void PopMovingTo() {
        if (path.Count == 0) {
            elapsed = 0;
            to = null;
        }
        else {
            to = path[0];
            path.RemoveAt(0);
        }
    }
}

public class HumanTransporter {
    public HumanTransporter(Guid id, GraphSegment segment, Vector2Int currentPos) {
        ID = id;
        this.segment = segment;

        moving = new(currentPos);
    }

    public readonly Guid ID;

    public GraphSegment? segment { get; set; }

    public HumanMovingComponent moving { get; }

    public MainState? state { get; set; }

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
