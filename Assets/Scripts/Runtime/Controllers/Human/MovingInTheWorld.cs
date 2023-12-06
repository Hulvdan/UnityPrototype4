using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.Human {
public class MovingInTheWorld {
    public enum State {
        MovingToTheCityHall,
        MovingToDestination,
    }

    public MovingInTheWorld(MainController controller) {
        _controller = controller;
    }

    public void OnEnter(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            Tracing.Log(
                $"human.segment.resourcesToTransport.Count = {human.segment.ResourcesToTransport.Count}");
        }

        human.moving.Path.Clear();
        UpdateStates(human, data, null, null);
    }

    public void OnExit(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        human.stateMovingInTheWorld = null;
        human.moving.to = null;
        human.moving.Path.Clear();
    }

    public void Update(
        Entities.Human human,
        HumanData data,
        float dt
    ) {
        UpdateStates(human, data, human.segment, human.building);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.type, Entities.Human.Type.Transporter);

        UpdateStates(human, data, oldSegment, null);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanData data
    ) {
        if (
            human.type == Entities.Human.Type.Constructor
            && human.building != null
            && human.moving.pos == human.building.pos
        ) {
            _controller.SetState(human, MainState.Building);
        }
    }

    void UpdateStates(
        Entities.Human human,
        HumanData data,
        [CanBeNull]
        GraphSegment oldSegment,
        [CanBeNull]
        Building oldBuilding
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            Assert.AreEqual(human.type, Entities.Human.Type.Transporter);

            // Hulvdan: Human stepped on a tile of the segment.
            // We don't need to keep the path anymore
            if (
                human.moving.to != null
                && human.segment.Graph.Contains(human.moving.to.Value)
                && human.segment.Graph.Node(human.moving.to.Value) != 0
            ) {
                human.moving.Path.Clear();
                return;
            }

            // Hulvdan: Human stepped on a tile of the segment and finished moving.
            // Starting MovingInsideSegment
            if (
                human.moving.to == null
                && human.segment.Graph.Contains(human.moving.pos)
                && human.segment.Graph.Node(human.moving.pos) != 0
            ) {
                Tracing.Log("_controller.SetState(human, HumanState.MovingInsideSegment)");
                _controller.SetState(human, MainState.MovingInsideSegment);
                return;
            }

            if (
                !ReferenceEquals(oldSegment, human.segment)
                || human.stateMovingInTheWorld != State.MovingToDestination
            ) {
                Tracing.Log("Setting human.stateMovingInTheWorld = State.MovingToSegment");
                human.stateMovingInTheWorld = State.MovingToDestination;

                var center = human.segment.Graph.GetCenters()[0];
                var path = data.map.FindPath(human.moving.to ?? human.moving.pos, center, true);

                Assert.IsTrue(path.Success);
                human.moving.AddPath(path.Path);
            }
        }
        else if (human.building != null) {
            Assert.IsTrue(human.type
                is Entities.Human.Type.Constructor
                or Entities.Human.Type.Employee
            );

            if (human.type == Entities.Human.Type.Constructor) {
                Assert.IsFalse(human.building.isConstructed);
            }
            else if (human.type == Entities.Human.Type.Employee) {
                Assert.IsTrue(human.building.isConstructed);
            }

            if (!ReferenceEquals(oldBuilding, human.building)) {
                human.moving.Path.Clear();

                var path = data.map.FindPath(
                    human.moving.to ?? human.moving.pos, human.building.pos, true
                );

                Assert.IsTrue(path.Success);
                human.moving.AddPath(path.Path);
            }
        }
        else if (human.stateMovingInTheWorld != State.MovingToTheCityHall) {
            Tracing.Log("human.stateMovingInTheWorld = State.MovingToTheCityHall");
            human.stateMovingInTheWorld = State.MovingToTheCityHall;

            var path = data.map.FindPath(
                human.moving.to ?? human.moving.pos, data.cityHall.pos, true
            );

            Assert.IsTrue(path.Success);
            human.moving.AddPath(path.Path);
        }
    }

    readonly MainController _controller;
}
}
