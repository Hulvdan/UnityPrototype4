using BFG.Runtime.Graphs;
using JetBrains.Annotations;

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
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            Tracing.Log(
                $"human.segment.resourcesToTransport.Count = {human.segment.ResourcesToTransport.Count}");
        }

        human.movingPath.Clear();
        UpdateStates(human, data, null);
    }

    public void OnExit(
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        human.stateMovingInTheWorld = null;
        human.movingTo = null;
        human.movingPath.Clear();
    }

    public void Update(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        float dt
    ) {
        UpdateStates(human, data, human.segment);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Tracing.Log("OnSegmentChanged");
        UpdateStates(human, data, oldSegment);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        // Hulvdan: Intentionally left blank
    }

    void UpdateStates(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        if (human.segment != null) {
            if (
                human.movingTo != null
                && human.segment.Graph.Contains(human.movingTo.Value)
                && human.segment.Graph.Node(human.movingTo.Value) != 0
            ) {
                human.movingPath.Clear();
                return;
            }

            if (
                human.movingTo == null
                && human.segment.Graph.Contains(human.pos)
                && human.segment.Graph.Node(human.pos) != 0
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
                var path = data.map.FindPath(human.movingTo ?? human.pos, center, true).Path;
                human.AddPath(path);
            }
        }
        else if (human.stateMovingInTheWorld != State.MovingToTheCityHall) {
            Tracing.Log("human.stateMovingInTheWorld = State.MovingToTheCityHall");
            human.stateMovingInTheWorld = State.MovingToTheCityHall;

            var path = data.map.FindPath(human.movingTo ?? human.pos, data.cityHall.pos, true).Path;
            human.AddPath(path);
        }
    }

    readonly MainController _controller;
}
}
