using System;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HumanTransporter_MovingResource_Controller {
    public enum State {
        MovingToResource,
        PickingUpResource,
        MovingResource,
        PlacingResource,
    }

    public HumanTransporter_MovingResource_Controller(HumanTransporter_Controller controller) {
        _controller = controller;

        _movingToResource = new(this);
        _pickingUpResource = new(this);
        _movingResource = new(this);
        _placingResource = new(this);
    }

    public void OnEnter(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.stateMovingResource, null);
        SetState(human, data, State.MovingToResource);
    }

    public void OnExit(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();
        Assert.AreNotEqual(human.stateMovingResource, null);
        human.stateMovingResource = null;

        Assert.AreEqual(human.stateMovingResource_targetedResource, null);
        Assert.AreEqual(human.stateMovingResource_pickingUpResourceElapsed, 0);
        Assert.AreEqual(human.stateMovingResource_pickingUpResourceNormalized, 0);
        Assert.AreEqual(human.stateMovingResource_placingResourceElapsed, 0);
        Assert.AreEqual(human.stateMovingResource_placingResourceNormalized, 0);
    }

    public void NestedState_Exit(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, null);

        switch (human.stateMovingResource!.Value) {
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
                throw new ArgumentOutOfRangeException();
        }

        _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
    }

    public void Update(
        HumanTransporter human,
        HumanTransporterData data,
        float dt
    ) {
        switch (human.stateMovingResource!.Value) {
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
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnHumanCurrentSegmentChanged(
        HumanTransporter human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, null);

        switch (human.stateMovingResource!.Value) {
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
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        switch (human.stateMovingResource!.Value) {
            case State.MovingToResource:
                _movingToResource.OnHumanMovedToTheNextTile(human, data);
                break;
            case State.MovingResource:
                _movingResource.OnHumanMovedToTheNextTile(human, data);
                break;
            case State.PickingUpResource:
            case State.PlacingResource:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetState(HumanTransporter human, HumanTransporterData data, State state) {
        using var _ = Tracing.Scope();

        var oldState = human.stateMovingResource;
        if (oldState != null) {
            switch (human.stateMovingResource.Value) {
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
                    throw new ArgumentOutOfRangeException();
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
                throw new ArgumentOutOfRangeException();
        }
    }

    readonly HumanTransporter_Controller _controller;
    readonly HT_MR_MovingToResource_Controller _movingToResource;
    readonly HT_MR_PickingUpResource_Controller _pickingUpResource;
    readonly HT_MR_MovingResource_Controller _movingResource;
    readonly HT_MR_PlacingResource_Controller _placingResource;
}
}
