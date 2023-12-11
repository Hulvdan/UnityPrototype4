using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.Human.MovingResources.State;

namespace BFG.Runtime.Controllers.Human {
public class PickingUpResource {
    public PickingUpResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, MRState.PickingUpResource);
        human.movingResources = MRState.PickingUpResource;

        Assert.AreNotEqual(human.movingResources_targetedResource, null);

        var res = human.segment!.ResourcesToTransport.Dequeue();
        Assert.AreNotEqual(res.Booking, null);
        Assert.AreEqual(res, human.movingResources_targetedResource);
        Assert.AreEqual(human.movingResources_targetedResource!.CarryingHuman, null);
        Assert.AreEqual(human.movingResources_targetedResource!.TargetedHuman, human);

        res!.CarryingHuman = human;

        data.transportation.OnHumanStartedPickingUpResource(res);
        data.map.onHumanStartedPickingUpResource.OnNext(new() {
            Human = human,
            Resource = res,
        });
    }

    public void OnExit(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();

        human.movingResources_pickingUpResourceElapsed = 0;
        human.movingResources_pickingUpResourceProgress = 0;
    }

    public void Update(Entities.Human human, HumanData data, float dt) {
        var res = human.movingResources_targetedResource;
        Assert.AreNotEqual(res, null, "human.targetedResource != null");

        human.movingResources_pickingUpResourceElapsed += dt;
        human.movingResources_pickingUpResourceProgress =
            human.movingResources_pickingUpResourceElapsed / data.PickingUpResourceDuration;

        if (human.movingResources_pickingUpResourceProgress < 1) {
            return;
        }

        human.movingResources_pickingUpResourceElapsed = data.PickingUpResourceDuration;
        human.movingResources_pickingUpResourceProgress = 1;

        data.map.onHumanFinishedPickingUpResource.OnNext(new() {
            Human = human,
            Resource = res,
        });

        if (res!.TransportationVertices.Count > 0) {
            var path = human.segment!.Graph.GetShortestPath(
                human.moving.pos, res!.TransportationVertices[0]
            );
            human.moving.AddPath(path);
            _controller.SetState(human, data, MRState.MovingResource);
        }
        else {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    readonly MovingResources _controller;
}
}
