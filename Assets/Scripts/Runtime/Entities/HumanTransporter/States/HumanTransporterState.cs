using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public enum HumanTransporterState {
    MovingInTheWorld,
    MovingInsideSegment,
    MovingResource,
}

public class HumanTransporter_Controller {
    readonly IMap _map;
    readonly IMapSize _mapSize;
    readonly Building _cityHall;

    readonly HumanTransporter_MovingInTheWorld_Controller _movingInTheWorld;
    readonly HumanTransporter_MovingInsideSegment_Controller _movingInsideSegment;
    readonly HumanTransporter_MovingResource_Controller _movingResource;
    readonly ResourceTransportationSystem _resourceTransportationSystem;
    readonly HumanTransporterData _data;

    public HumanTransporter_Controller(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ResourceTransportationSystem resourceTransportationSystem
    ) {
        _map = map;
        _mapSize = mapSize;
        _cityHall = cityHall;
        _resourceTransportationSystem = resourceTransportationSystem;

        _movingInTheWorld = new(this);
        _movingInsideSegment = new(this);
        _movingResource = new(this);

        _data = new(_map, _mapSize, _cityHall, _resourceTransportationSystem, 1f, 1f);
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
                case HumanTransporterState.MovingResource:
                    _movingResource.OnExit(human, _data);
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
            case HumanTransporterState.MovingResource:
                _movingResource.OnEnter(
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
            case HumanTransporterState.MovingResource:
                _movingResource.Update(human, _data, dt);
                break;
        }
    }

    public void OnHumanCurrentSegmentChanged(
        HumanTransporter human,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log("OnSegmentChanged");

        Assert.AreNotEqual(human.state, null);

        switch (human.state) {
            case HumanTransporterState.MovingInTheWorld:
                _movingInTheWorld.OnHumanCurrentSegmentChanged(
                    human, _map, _mapSize, _cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingInsideSegment:
                _movingInsideSegment.OnHumanCurrentSegmentChanged(
                    human, _map, _mapSize, _cityHall, oldSegment
                );
                break;
            case HumanTransporterState.MovingResource:
                _movingResource.OnHumanCurrentSegmentChanged(
                    human, _data, oldSegment
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
            case HumanTransporterState.MovingResource:
                _movingResource.OnHumanMovedToTheNextTile(
                    human, _data
                );
                break;
        }
    }
}
}
