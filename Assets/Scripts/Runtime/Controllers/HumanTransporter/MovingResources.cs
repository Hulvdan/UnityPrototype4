using System;
using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.Human {
public class MovingResources {
    public enum State {
        MovingToResource,
        PickingUpResource,
        MovingResource,
        PlacingResource,
    }

    public MovingResources(MainController controller) {
        _controller = controller;

        _movingToResource = new(this);
        _pickingUpResource = new(this);
        _movingResource = new(this);
        _placingResource = new(this);
    }

    public void OnEnter(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.movingResources, null);
        SetState(human, data, State.MovingToResource);
    }

    public void OnExit(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();
        Assert.AreNotEqual(human.movingResources, null);
        human.movingResources = null;

        Assert.AreEqual(human.movingResources_targetedResource, null);
        Assert.AreEqual(human.movingResources_pickingUpResourceElapsed, 0);
        Assert.AreEqual(human.movingResources_pickingUpResourceProgress, 0);
        Assert.AreEqual(human.movingResources_placingResourceElapsed, 0);
        Assert.AreEqual(human.movingResources_placingResourceProgress, 0);
    }

    public void NestedState_Exit(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, null);

        switch (human.movingResources!.Value) {
            case State.MovingToResource:
                _movingToResource.OnExit(human, data);
                break;
            case State.PickingUpResource:
                _pickingUpResource.OnExit(human, data);
                break;
            case State.MovingResource:
                _movingResource.OnExit(human, data);
                break;
            case State.PlacingResource:
                _placingResource.OnExit(human, data);
                break;
            default:
                throw new NotSupportedException();
        }

        _controller.SetState(human, MainState.MovingInTheWorld);
    }

    public void Update(
        Entities.Human human,
        HumanData data,
        float dt
    ) {
        switch (human.movingResources!.Value) {
            case State.MovingToResource:
                _movingToResource.Update(human, data, dt);
                break;
            case State.PickingUpResource:
                _pickingUpResource.Update(human, data, dt);
                break;
            case State.MovingResource:
                _movingResource.Update(human, data, dt);
                break;
            case State.PlacingResource:
                _placingResource.Update(human, data, dt);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, null);

        switch (human.movingResources!.Value) {
            case State.MovingToResource:
                _movingToResource.OnHumanCurrentSegmentChanged(human, data, oldSegment);
                break;
            case State.PickingUpResource:
                _pickingUpResource.OnHumanCurrentSegmentChanged(human, data, oldSegment);
                break;
            case State.MovingResource:
                _movingResource.OnHumanCurrentSegmentChanged(human, data, oldSegment);
                break;
            case State.PlacingResource:
                _placingResource.OnHumanCurrentSegmentChanged(human, data, oldSegment);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        switch (human.movingResources!.Value) {
            case State.MovingToResource:
                _movingToResource.OnHumanMovedToTheNextTile(human, data);
                break;
            case State.MovingResource:
                _movingResource.OnHumanMovedToTheNextTile(human, data);
                break;
            case State.PickingUpResource:
            case State.PlacingResource:
            default:
                throw new NotSupportedException();
        }
    }

    public void SetState(
        Entities.Human human,
        HumanData data,
        State state
    ) {
        using var _ = Tracing.Scope();

        var oldState = human.movingResources;
        if (oldState != null) {
            switch (human.movingResources!.Value) {
                case State.MovingToResource:
                    _movingToResource.OnExit(human, data);
                    break;
                case State.PickingUpResource:
                    _pickingUpResource.OnExit(human, data);
                    break;
                case State.MovingResource:
                    _movingResource.OnExit(human, data);
                    break;
                case State.PlacingResource:
                    _placingResource.OnExit(human, data);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        switch (state) {
            case State.MovingToResource:
                _movingToResource.OnEnter(human, data);
                break;
            case State.PickingUpResource:
                _pickingUpResource.OnEnter(human, data);
                break;
            case State.MovingResource:
                _movingResource.OnEnter(human, data);
                break;
            case State.PlacingResource:
                _placingResource.OnEnter(human, data);
                break;
            default:

                throw new NotSupportedException();
        }
    }

    readonly MainController _controller;
    readonly MovingToResource _movingToResource;
    readonly PickingUpResource _pickingUpResource;
    readonly MovingResource _movingResource;
    readonly PlacingResource _placingResource;
}
}
