using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class PickingUpResource {
    public PickingUpResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, MRState.PickingUpResource);
        human.stateMovingResource = MRState.PickingUpResource;

        Assert.AreNotEqual(human.stateMovingResource_targetedResource, null);

        var res = human.segment!.ResourcesToTransport.Dequeue();
        Assert.AreNotEqual(res.Booking, null);
        Assert.AreEqual(res, human.stateMovingResource_targetedResource);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.CarryingHuman, null);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.TargetedHuman, human);

        res!.CarryingHuman = human;

        data.transportation.OnHumanStartedPickingUpResource(res);
        data.map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
            Human = human,
            Resource = res,
        });
    }

    public void OnExit(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.stateMovingResource_pickingUpResourceElapsed = 0;
        human.stateMovingResource_pickingUpResourceProgress = 0;
    }

    public void Update(Entities.HumanTransporter human, HumanTransporterData data, float dt) {
        var res = human.stateMovingResource_targetedResource;
        Assert.AreNotEqual(res, null, "human.targetedResource != null");

        human.stateMovingResource_pickingUpResourceElapsed += dt;
        human.stateMovingResource_pickingUpResourceProgress =
            human.stateMovingResource_pickingUpResourceElapsed / data.PickingUpResourceDuration;

        if (human.stateMovingResource_pickingUpResourceProgress < 1) {
            return;
        }

        human.stateMovingResource_pickingUpResourceElapsed = data.PickingUpResourceDuration;
        human.stateMovingResource_pickingUpResourceProgress = 1;

        data.map.onHumanTransporterPickedUpResource.OnNext(new() {
            Human = human,
            Resource = res,
        });

        if (res!.TransportationVertices.Count > 0) {
            var path = human.segment!.Graph.GetShortestPath(
                human.pos, res!.TransportationVertices[0]
            );
            human.AddPath(path);
            _controller.SetState(human, data, MRState.MovingResource);
        }
        else {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    readonly MovingResources _controller;
}
}
