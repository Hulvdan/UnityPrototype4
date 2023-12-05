using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class MovingToResource {
    public MovingToResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(null, human.stateMovingResource);
        Assert.AreNotEqual(null, human.segment);
        human.stateMovingResource = MRState.MovingToResource;

        var segment = human.segment;
        var res = segment!.ResourcesToTransport.First;
        Assert.AreNotEqual(null, res.Booking);

        human.stateMovingResource_targetedResource = res;
        res.TargetedHuman = human;
        if (res.Pos == human.pos && human.movingTo == null) {
            _controller.SetState(human, data, MRState.PickingUpResource);
            return;
        }

        Assert.AreEqual(null, human.movingTo, "human.movingTo == null");
        Assert.AreEqual(0, human.movingPath.Count, "human.movingPath.Count == 0");

        var graphContains = segment.Graph.Contains(human.pos);
        var nodeIsWalkable = segment.Graph.Node(human.pos) != 0;

        if (res.Pos != human.pos && graphContains && nodeIsWalkable) {
            human.AddPath(segment.Graph.GetShortestPath(human.pos, res.Pos));
        }
        else if (!graphContains || !nodeIsWalkable) {
            _controller.NestedState_Exit(human, data);
        }
    }

    public void OnExit(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.movingPath.Clear();
    }

    public void Update(Entities.HumanTransporter human, HumanTransporterData data, float dt) {
        // There is no need in implementation
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.HumanTransporter human,
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

    readonly MovingResources _controller;
}
}
