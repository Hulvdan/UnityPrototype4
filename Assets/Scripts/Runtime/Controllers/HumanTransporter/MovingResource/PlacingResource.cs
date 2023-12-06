using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.Human.MovingResources.State;

namespace BFG.Runtime.Controllers.Human {
public class PlacingResource {
    public PlacingResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, MRState.PlacingResource);
        Assert.AreNotEqual(human.movingResources_targetedResource, null);
        Assert.AreEqual(human.movingResources_targetedResource!.TargetedHuman, human);
        Assert.AreEqual(human.movingResources_targetedResource!.CarryingHuman, human);
        Assert.AreEqual(0, human.movingResources_placingResourceElapsed);
        Assert.AreEqual(0, human.movingResources_placingResourceProgress);

        human.movingResources = MRState.PlacingResource;

        data.map.onHumanStartedPlacingResource.OnNext(new() {
            Human = human,
            Resource = human.movingResources_targetedResource,
        });
    }

    public void OnExit(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.movingResources_targetedResource, null);
        human.movingResources_placingResourceElapsed = 0;
        human.movingResources_placingResourceProgress = 0;
    }

    public void Update(Entities.Human human, HumanData data, float dt) {
        human.movingResources_placingResourceElapsed += dt;
        human.movingResources_placingResourceProgress =
            human.movingResources_placingResourceElapsed / data.PlacingResourceDuration;

        if (human.movingResources_placingResourceProgress < 1) {
            return;
        }

        human.movingResources_placingResourceElapsed = data.PlacingResourceDuration;
        human.movingResources_placingResourceProgress = 1;

        var res = human.movingResources_targetedResource;

        Building building = null;
        if (res!.Booking != null) {
            var b = res!.Booking.Value.Building;
            if (b.pos == human.moving.pos) {
                building = b;
            }
        }

        human.movingResources_targetedResource = null;

        data.transportation.OnHumanPlacedResource(human.moving.pos, human.segment, res!);
        data.map.onHumanPlacedResource.OnNext(new() {
            Human = human,
            Resource = res!,
            Building = building,
        });

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    readonly MovingResources _controller;
}
}
