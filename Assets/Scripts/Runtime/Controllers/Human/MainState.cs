using System;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using BFG.Runtime.Systems;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.Human {
public enum MainState {
    // Common
    MovingInTheWorld,

    // Transporter
    MovingInsideSegment,
    MovingResource,

    // Builder
    Building,

    // Employee
    Employee,
}

public class MainController {
    readonly BuildingDatabase _bdb;
    readonly HumanDatabase _db;
    readonly MovingInTheWorld _movingInTheWorld;
    readonly MovingInsideSegment _movingInsideSegment;
    readonly MovingResources _movingResources;
    readonly ConstructionController _constructionController;
    readonly EmployeeController _employeeController;
    readonly HumanData _data;

    public MainController(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ResourceTransportation resourceTransportation,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        _bdb = bdb;
        _db = db;
        _movingInTheWorld = new(this);
        _movingInsideSegment = new(this);
        _movingResources = new(this);
        _constructionController = new(this);
        _employeeController = new(bdb, db);

        _data = new(map, mapSize, cityHall, resourceTransportation, 1f, 1f, 2f, 1f);
    }

    public void SetState(Entities.Human human, MainState newState) {
        using var _ = Tracing.Scope();
        Tracing.Log($"SetState '{human.state}' -> '{newState}'");

        var oldState = human.state;
        human.state = newState;

        if (oldState != null) {
            switch (oldState) {
                case MainState.MovingInTheWorld:
                    _movingInTheWorld.OnExit(human, _data);
                    break;
                case MainState.MovingInsideSegment:
                    _movingInsideSegment.OnExit(human, _data);
                    break;
                case MainState.MovingResource:
                    _movingResources.OnExit(human, _data);
                    break;
                case MainState.Building:
                    _constructionController.OnExit(human, _data);
                    break;
                case MainState.Employee:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        switch (newState) {
            case MainState.MovingInTheWorld:
                _movingInTheWorld.OnEnter(human, _data);
                break;
            case MainState.MovingInsideSegment:
                _movingInsideSegment.OnEnter(human, _data);
                break;
            case MainState.MovingResource:
                _movingResources.OnEnter(human, _data);
                break;
            case MainState.Building:
                _constructionController.OnEnter(human, _data);
                break;
            case MainState.Employee:
                _employeeController.SwitchToTheNextBehaviour(human);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void Update(Entities.Human human, float dt) {
        switch (human.state) {
            case MainState.MovingInTheWorld:
                _movingInTheWorld.Update(human, _data, dt);
                break;
            case MainState.MovingInsideSegment:
                _movingInsideSegment.Update(human, _data, dt);
                break;
            case MainState.MovingResource:
                _movingResources.Update(human, _data, dt);
                break;
            case MainState.Building:
                _constructionController.Update(human, _data, dt);
                break;
            case MainState.Employee:
                _employeeController.Update(human, _data, dt);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log("OnSegmentChanged");

        Assert.AreNotEqual(human.state, null);

        switch (human.state) {
            case MainState.MovingInTheWorld:
                _movingInTheWorld.OnHumanCurrentSegmentChanged(human, _data, oldSegment);
                break;
            case MainState.MovingInsideSegment:
                _movingInsideSegment.OnHumanCurrentSegmentChanged(human, _data, oldSegment);
                break;
            case MainState.MovingResource:
                _movingResources.OnHumanCurrentSegmentChanged(human, _data, oldSegment);
                break;
            case MainState.Building:
                throw new NotSupportedException();
            default:
                throw new NotSupportedException();
        }
    }

    public void OnHumanMovedToTheNextTile(Entities.Human human) {
        using var _ = Tracing.Scope();

        switch (human.state) {
            case MainState.MovingInTheWorld:
                _movingInTheWorld.OnHumanMovedToTheNextTile(human, _data);
                break;
            case MainState.MovingInsideSegment:
                _movingInsideSegment.OnHumanMovedToTheNextTile(human, _data);
                break;
            case MainState.MovingResource:
                _movingResources.OnHumanMovedToTheNextTile(human, _data);
                break;
            case MainState.Employee:
                _employeeController.OnHumanMovedToTheNextTile(human, _data);
                break;
            case MainState.Building:
                throw new NotSupportedException();
            default:
                throw new NotSupportedException();
        }
    }
}
}
