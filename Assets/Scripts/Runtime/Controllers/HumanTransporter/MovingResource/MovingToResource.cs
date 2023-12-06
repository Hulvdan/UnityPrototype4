using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class MovingToResource {
    public MovingToResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.Human human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(null, human.movingResources);
        Assert.AreNotEqual(null, human.segment);
        human.movingResources = MRState.MovingToResource;

        var segment = human.segment;
        var res = segment!.ResourcesToTransport.First;
        Assert.AreNotEqual(null, res.Booking);

        human.movingResources_targetedResource = res;
        res.TargetedHuman = human;
        if (res.Pos == human.moving.pos && human.moving.to == null) {
            _controller.SetState(human, data, MRState.PickingUpResource);
            return;
        }

        Assert.AreEqual(null, human.moving.to, "human.movingTo == null");
        Assert.AreEqual(0, human.moving.path.Count, "human.movingPath.Count == 0");

        var graphContains = segment.Graph.Contains(human.moving.pos);
        var nodeIsWalkable = segment.Graph.Node(human.moving.pos) != 0;

        if (res.Pos != human.moving.pos && graphContains && nodeIsWalkable) {
            human.moving.AddPath(segment.Graph.GetShortestPath(human.moving.pos, res.Pos));
        }
        else if (!graphContains || !nodeIsWalkable) {
            _controller.NestedState_Exit(human, data);
        }
    }

    public void OnExit(Entities.Human human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.moving.path.Clear();
    }

    public void Update(Entities.Human human, HumanTransporterData data, float dt) {
        // Hulvdan: Intentionally left blank
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.movingResources_targetedResource == null) {
            _controller.NestedState_Exit(human, data);
            return;
        }

        if (human.moving.pos == human.movingResources_targetedResource.Pos) {
            _controller.SetState(human, data, MRState.PickingUpResource);
        }
    }

    readonly MovingResources _controller;
}
}
