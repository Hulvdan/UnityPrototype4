using System;
using JetBrains.Annotations;

namespace BFG.Runtime {
public enum HumanTransporterState {
    MovingInTheWorld,
    MovingInsideSegment,
    MovingItem,
}

public class HumanTransporter_Controller {
    readonly IMap _map;
    readonly IMapSize _mapSize;
    readonly Building _cityHall;

    readonly HumanTransporter_MovingInTheWorld_Controller _movingInTheWorld;
    readonly HumanTransporter_MovingInsideSegment_Controller _movingInsideSegment;
    readonly HumanTransporter_MovingItem_Controller _movingItem;
    readonly ItemTransportationSystem _itemTransportationSystem;
    readonly HumanTransporterData _data;

    public HumanTransporter_Controller(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ItemTransportationSystem itemTransportationSystem
    ) {
        _map = map;
        _mapSize = mapSize;
        _cityHall = cityHall;
        _itemTransportationSystem = itemTransportationSystem;

        _movingInTheWorld = new(this);
        _movingInsideSegment = new(this);
        _movingItem = new(this);

        _data = new(_map, _mapSize, _cityHall, _itemTransportationSystem, 1f, 1f);
    }

    public void SetState(
        HumanTransporter human,
        HumanTransporterState newState
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log($"SetState '{human.state}' -> '{newState}'");

        var oldState = human.state;
        human.state = newState;

        if (oldState != null) {
            switch (oldState) {
                case HumanTransporterState.MovingInTheWorld:
                    _movingInTheWorld.OnExit(
                        human, _map, _mapSize, _cityHall
                    );
                    break;
                case HumanTransporterState.MovingInsideSegment:
                    _movingInsideSegment.OnExit(
                        human, _map, _mapSize, _cityHall
                    );
                    break;
                case HumanTransporterState.MovingItem:
                    _movingItem.OnExit(human, _data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(oldState), oldState, null);
            }
        }

        switch (newState) {
            case HumanTransporterState.MovingInTheWorld:
                _movingInTheWorld.OnEnter(
                    human, _map, _mapSize, _cityHall
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                _movingInsideSegment.OnEnter(
                    human, _map, _mapSize, _cityHall
                );
                break;
            case HumanTransporterState.MovingItem:
                _movingItem.OnEnter(
                    human, _data
                );
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public void Update(
        HumanTransporter human,
        float dt
    ) {
        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                _movingInTheWorld.Update(human, _map, _mapSize, _cityHall, dt);
                break;
            case HumanTransporterState.MovingInsideSegment:
                _movingInsideSegment.Update(human, _map, _mapSize, _cityHall, dt);
                break;
            case HumanTransporterState.MovingItem:
                _movingItem.Update(human, _data, dt);
                break;
        }
    }

    public void OnSegmentChanged(
        HumanTransporter human,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log("OnSegmentChanged");

        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                _movingInTheWorld.OnSegmentChanged(
                    human, _map, _mapSize, _cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                _movingInsideSegment.OnSegmentChanged(
                    human, _map, _mapSize, _cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingItem:
                _movingItem.OnSegmentChanged(
                    human, _data, oldSegment
                );
                break;
        }
    }

    public void OnHumanMovedToTheNextTile(HumanTransporter human) {
        using var _ = Tracing.Scope();

        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                _movingInTheWorld.OnHumanMovedToTheNextTile(
                    human, _data
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                _movingInsideSegment.OnHumanMovedToTheNextTile(
                    human, _data
                );
                break;
            case HumanTransporterState.MovingItem:
                _movingItem.OnHumanMovedToTheNextTile(
                    human, _data
                );
                break;
        }
    }
}
}
