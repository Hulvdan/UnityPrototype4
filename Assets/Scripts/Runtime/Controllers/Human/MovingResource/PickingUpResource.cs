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

        var res = human.segment!.resourcesToTransport.Dequeue();
        Assert.AreNotEqual(res.booking, null);
        Assert.AreEqual(res, human.movingResources_targetedResource);
        Assert.AreEqual(human.movingResources_targetedResource!.carryingHuman, null);
        Assert.AreEqual(human.movingResources_targetedResource!.targetedHuman, human);

        res!.carryingHuman = human;

        data.transportation.OnHumanStartedPickingUpResource(res);
        data.map.onHumanStartedPickingUpResource.OnNext(new() {
            human = human,
            resource = res,
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
            human.movingResources_pickingUpResourceElapsed / data.pickingUpResourceDuration;

        if (human.movingResources_pickingUpResourceProgress < 1) {
            return;
        }

        human.movingResources_pickingUpResourceElapsed = data.pickingUpResourceDuration;
        human.movingResources_pickingUpResourceProgress = 1;

        data.map.onHumanFinishedPickingUpResource.OnNext(new() {
            human = human,
            resource = res,
        });

        if (res!.transportationVertices.Count > 0) {
            var path = human.segment!.graph.GetShortestPath(
                human.moving.pos, res!.transportationVertices[0]
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
