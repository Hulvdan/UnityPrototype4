using JetBrains.Annotations;

namespace BFG.Runtime {
public static class HumanTransporter_MovingInTheWorld_Controller {
    public enum State {
        MovingToTheCityHall,
        MovingToSegment,
    }

    public static void OnEnter(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall
    ) {
        UpdateStates(human, map, mapSize, cityHall, null);
    }

    public static void OnExit(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall
    ) {
        human.stateMovingInTheWorld = null;
    }

    public static void Update(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall, float dt
    ) {
        UpdateStates(human, map, mapSize, cityHall, human.segment);
    }

    public static void OnSegmentChanged(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        UpdateStates(human, map, mapSize, cityHall, oldSegment);
    }

    static void UpdateStates(
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
                HumanTransporter_Controller.SetState(
                    human, HumanTransporterState.MovingInsideSegment, map, mapSize, cityHall
                );
            }
        }
        else if (human.stateMovingInTheWorld != State.MovingToTheCityHall) {
            human.stateMovingInTheWorld = State.MovingToTheCityHall;

            var path = map.FindPath(human.movingTo ?? human.pos, cityHall.pos, true).Path;
            human.AddPath(path);
        }
    }
}
}
