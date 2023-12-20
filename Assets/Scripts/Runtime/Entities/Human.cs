#nullable enable
using System;
using BFG.Runtime.Controllers.Human;
using BFG.Runtime.Graphs;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class Human {
    public enum Type {
        Transporter = 0,
        Constructor = 1,
        Employee = 2,
    }

    public static Human Transporter(Guid id, Vector2Int currentPos, GraphSegment segment) {
        return new(id, Type.Transporter, currentPos, segment, null);
    }

    public static Human Constructor(Guid id, Vector2Int currentPos, Building building) {
        return new(id, Type.Constructor, currentPos, null, building);
    }

    public static Human Employee(Guid id, Vector2Int currentPos, Building building) {
        return new(id, Type.Employee, currentPos, null, building);
    }

    Human(
        Guid id_,
        Type type_,
        Vector2Int currentPos,
        GraphSegment? segment_,
        Building? building_
    ) {
        switch (type_) {
            case Type.Transporter:
                Assert.AreNotEqual(segment_, null);
                Assert.AreEqual(building_, null);
                break;
            case Type.Constructor:
            case Type.Employee:
                Assert.AreNotEqual(building_, null);
                Assert.AreEqual(segment_, null);
                break;
            default:
                throw new NotSupportedException();
        }

        id = id_;
        segment = segment_;
        building = building_;
        type = type_;
        moving = new(currentPos);
    }

    public Guid id { get; }
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

    #region Employee

    public float harvestingElapsed;
    public int currentBehaviourId = -1;

    public EmployeeBehaviourSet? behaviourSet;
    public float processingElapsed;

    #endregion
}
}
