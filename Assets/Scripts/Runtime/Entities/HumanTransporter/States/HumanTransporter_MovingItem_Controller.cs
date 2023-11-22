using JetBrains.Annotations;

namespace BFG.Runtime {
public static class HumanTransporter_MovingItem_Controller {
    public enum State {
        MovingToItem,
        PickingUpItem,
        MovingItem,
        PlacingItem,
    }

    public static void OnEnter(
        HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall
    ) {
    }

    public static void OnExit(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        human.stateMovingItem = null;
    }

    public static void Update(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        float dt
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
    }
}
}
