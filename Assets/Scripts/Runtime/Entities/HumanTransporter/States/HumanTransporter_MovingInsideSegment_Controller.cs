using JetBrains.Annotations;

namespace BFG.Runtime {
public static class HumanTransporter_MovingInsideSegment_Controller {
    public enum State {
        MovingToCenter,
        Idle,
    }

    public static void OnEnter(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall
    ) {
    }

    public static void OnExit(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall
    ) {
        human.stateMovingInsideSegment = null;
    }

    public static void Update(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall, float dt
    ) {
    }

    public static void OnSegmentChanged(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        HumanTransporter_Controller.SetState(
            human, HumanTransporterState.MovingInTheWorld, map, mapSize, cityHall
        );
    }
}
}
