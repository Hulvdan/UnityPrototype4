#nullable enable
using System;
using System.Collections.Generic;
using BFG.Runtime.Controllers.Human;
using BFG.Runtime.Graphs;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
// public class HumanBuilder {
//     public HumanBuilder(Guid id, Building building, Vector2Int currentPos) {
//         Assert.IsFalse(building.isBuilt);
//
//         ID = id;
//         this.building = building;
//         moving = new(currentPos);
//     }
//
//     public Guid ID { get; }
//     public HumanMovingComponent moving { get; }
//
//     public Building building { get; }
// }

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

public class Human {
    public enum Type {
        Transporter = 0,
        Builder = 1,
    }

    public static Human Transporter(Guid id, Vector2Int currentPos, GraphSegment segment) {
        return new(id, Type.Transporter, currentPos, segment, null);
    }

    public static Human Builder(Guid id, Vector2Int currentPos, Building building) {
        return new(id, Type.Builder, currentPos, null, building);
    }

    Human(
        Guid id,
        Type type,
        Vector2Int currentPos,
        GraphSegment? segment,
        Building? building
    ) {
        switch (type) {
            case Type.Transporter:
                Assert.AreNotEqual(segment, null);
                Assert.AreEqual(building, null);
                break;
            case Type.Builder:
                Assert.AreNotEqual(building, null);
                Assert.AreEqual(segment, null);
                break;
            default:
                throw new NotSupportedException();
        }

        ID = id;
        this.segment = segment;
        this.building = building;
        this.type = type;
        moving = new(currentPos);
    }

    public Guid ID { get; }
    public HumanMovingComponent moving { get; }

    public GraphSegment? segment { get; set; }
    public Type type { get; }
    public Building? building { get; set; }
    public MainState? state { get; set; }

    #region MovingInTheWorld

    public MovingInTheWorld.State? stateMovingInTheWorld { get; set; }

    #endregion

    #region MovingResources

    public MovingResources.State? movingResources;

    public float movingResources_pickingUpResourceElapsed;
    public float movingResources_pickingUpResourceProgress;
    public float movingResources_placingResourceElapsed;
    public float movingResources_placingResourceProgress;

    public MapResource? movingResources_targetedResource = null;

    #endregion

    #region Building

    public float building_elapsed;
    public float building_progress;

    #endregion
}
}
