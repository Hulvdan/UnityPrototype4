using JetBrains.Annotations;

namespace BFG.Runtime {
public class HumanTransporter_MovingInTheWorld_Controller {
    public enum State {
        MovingToTheCityHall,
        MovingToSegment,
    }

    public HumanTransporter_MovingInTheWorld_Controller(HumanTransporter_Controller controller) {
        _controller = controller;
    }

    public void OnEnter(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        UpdateStates(human, map, mapSize, cityHall, null);
    }

    public void OnExit(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        human.stateMovingInTheWorld = null;
    }

    public void Update(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        float dt
    ) {
        UpdateStates(human, map, mapSize, cityHall, human.segment);
    }

    public void OnSegmentChanged(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        UpdateStates(human, map, mapSize, cityHall, oldSegment);
    }

    void UpdateStates(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        if (human.segment != null) {
            if (
                !ReferenceEquals(oldSegment, human.segment)
                || human.stateMovingInTheWorld != State.MovingToSegment
            ) {
                human.stateMovingInTheWorld = State.MovingToSegment;

                var center = human.segment.Graph.GetCenters()[0];
                var path = map.FindPath(human.movingTo ?? human.pos, center, true).Path;
                human.AddPath(path);
            }

            if (human.segment.Graph.Contains(human.pos)) {
                _controller.SetState(human, HumanTransporterState.MovingInsideSegment);
            }
        }
        else if (human.stateMovingInTheWorld != State.MovingToTheCityHall) {
            human.stateMovingInTheWorld = State.MovingToTheCityHall;

            var path = map.FindPath(human.movingTo ?? human.pos, cityHall.pos, true).Path;
            human.AddPath(path);
        }
    }

    readonly HumanTransporter_Controller _controller;
}
}
