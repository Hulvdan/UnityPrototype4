using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class MovingInTheWorld {
    public enum State {
        MovingToTheCityHall,
        MovingToSegment,
    }

    public MovingInTheWorld(MainController controller) {
        _controller = controller;
    }

    public void OnEnter(
        Entities.Human human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            Tracing.Log(
                $"human.segment.resourcesToTransport.Count = {human.segment.ResourcesToTransport.Count}");
        }

        human.moving.path.Clear();
        UpdateStates(human, data, null);
    }

    public void OnExit(
        Entities.Human human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        human.stateMovingInTheWorld = null;
        human.moving.to = null;
        human.moving.path.Clear();
    }

    public void Update(
        Entities.Human human,
        HumanTransporterData data,
        float dt
    ) {
        UpdateStates(human, data, human.segment);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Tracing.Log("OnSegmentChanged");
        UpdateStates(human, data, oldSegment);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanTransporterData data
    ) {
        // Hulvdan: Intentionally left blank
    }

    void UpdateStates(
        Entities.Human human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            if (
                human.moving.to != null
                && human.segment.Graph.Contains(human.moving.to.Value)
                && human.segment.Graph.Node(human.moving.to.Value) != 0
            ) {
                human.moving.path.Clear();
                return;
            }

            if (
                human.moving.to == null
                && human.segment.Graph.Contains(human.moving.pos)
                && human.segment.Graph.Node(human.moving.pos) != 0
            ) {
                Tracing.Log(
                    "_controller.SetState(human, HumanTransporterState.MovingInsideSegment)");
                _controller.SetState(human, MainState.MovingInsideSegment);
                return;
            }

            if (
                !ReferenceEquals(oldSegment, human.segment)
                || human.stateMovingInTheWorld != State.MovingToSegment
            ) {
                Tracing.Log("Setting human.stateMovingInTheWorld = State.MovingToSegment");
                human.stateMovingInTheWorld = State.MovingToSegment;

                var center = human.segment.Graph.GetCenters()[0];
                var path = data.map
                    .FindPath(human.moving.to ?? human.moving.pos, center, true)
                    .Path;
                human.moving.AddPath(path);
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
