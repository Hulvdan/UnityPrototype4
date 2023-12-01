using System.Diagnostics.Tracing;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.HumanTransporter_MovingResource_Controller.State;

namespace BFG.Runtime {
public class HT_MR_MovingToResource_Controller {
    public HT_MR_MovingToResource_Controller(
        HumanTransporter_MovingResource_Controller humanTransporter_MovingResource_Controller
    ) {
        _controller = humanTransporter_MovingResource_Controller;
    }

    public void OnEnter(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.stateMovingResource, null);
        human.stateMovingResource = MRState.MovingToResource;

        var segment = human.segment;
        var resource = segment.resourcesToTransport.First;

        human.stateMovingResource_targetedResource = resource;
        resource.TargetedHuman = human;
        if (resource.Pos == human.pos && human.movingTo == null) {
            _controller.SetState(human, data, MRState.PickingUpResource);
            return;
        }

        Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
        Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");

        var graphContains = segment.Graph.Contains(human.pos);
        var nodeIsWalkable = segment.Graph.Node(human.pos) != 0;

        if (resource.Pos != human.pos && graphContains && nodeIsWalkable) {
            human.AddPath(segment.Graph.GetShortestPath(human.pos, resource.Pos));
        }
        else if (!graphContains || !nodeIsWalkable) {
            _controller.NestedState_Exit(human, data);
        }
    }

    public void OnExit(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.movingPath.Clear();
    }

    public void Update(HumanTransporter human, HumanTransporterData data, float dt) {
        if (human.stateMovingResource_targetedResource == null) {
            _controller.NestedState_Exit(human, data);
        }
    }

    public void OnHumanCurrentSegmentChanged(
        HumanTransporter human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.stateMovingResource_targetedResource == null) {
            _controller.NestedState_Exit(human, data);
            return;
        }

        if (human.pos == human.stateMovingResource_targetedResource.Pos) {
            _controller.SetState(human, data, MRState.PickingUpResource);
        }
    }

    readonly HumanTransporter_MovingResource_Controller _controller;
}
}
