using System;
using JetBrains.Annotations;

namespace BFG.Runtime {
public enum HumanTransporterState {
    MovingInTheWorld,
    MovingInsideSegment,
    MovingItem,
}

public static class HumanTransporter_Controller {
    public static void SetState(
        HumanTransporter human,
        HumanTransporterState newState,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        var oldState = human.state;
        human.state = newState;

        if (oldState != null) {
            switch (oldState) {
                case HumanTransporterState.MovingInTheWorld:
                    HumanTransporter_MovingInTheWorld_Controller.OnExit(
                        human, map, mapSize, cityHall);
                    break;
                case HumanTransporterState.MovingInsideSegment:
                    HumanTransporter_MovingInsideSegment_Controller.OnExit(
                        human, map, mapSize, cityHall);
                    break;
                case HumanTransporterState.MovingItem:
                    HumanTransporter_MovingItem_Controller.OnExit(human, map, mapSize, cityHall);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(oldState), oldState, null);
            }
        }

        switch (newState) {
            case HumanTransporterState.MovingInTheWorld:
                HumanTransporter_MovingInTheWorld_Controller.OnEnter(human, map, mapSize, cityHall);
                break;
            case HumanTransporterState.MovingInsideSegment:
                HumanTransporter_MovingInsideSegment_Controller.OnEnter(
                    human, map, mapSize, cityHall);
                break;
            case HumanTransporterState.MovingItem:
                HumanTransporter_MovingItem_Controller.OnEnter(human, map, mapSize, cityHall);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public static void Update(HumanTransporter human, IMap map, IMapSize mapSize, Building cityHall,
        float dt) {
        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                HumanTransporter_MovingInTheWorld_Controller.Update(
                    human, map, mapSize, cityHall, dt
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                HumanTransporter_MovingInsideSegment_Controller.Update(
                    human, map, mapSize, cityHall, dt
                );
                break;
            case HumanTransporterState.MovingItem:
                HumanTransporter_MovingItem_Controller.Update(human, map, mapSize, cityHall, dt);
                break;
        }
    }

    public static void OnSegmentChanged(HumanTransporter human, IMap map, IMapSize mapSize,
        Building cityHall, [CanBeNull] GraphSegment oldSegment) {
        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                HumanTransporter_MovingInTheWorld_Controller.OnSegmentChanged(
                    human, map, mapSize, cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                HumanTransporter_MovingInsideSegment_Controller.OnSegmentChanged(
                    human, map, mapSize, cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingItem:
                HumanTransporter_MovingItem_Controller.OnSegmentChanged(
                    human, map, mapSize, cityHall, oldSegment
                );
                break;
        }
    }
}
}
