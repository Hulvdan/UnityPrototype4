using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class PlacingResource {
    public PlacingResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, MRState.PlacingResource);
        Assert.AreNotEqual(human.stateMovingResource_targetedResource, null);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.TargetedHuman, human);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.CarryingHuman, human);
        Assert.AreEqual(0, human.stateMovingResource_placingResourceElapsed);
        Assert.AreEqual(0, human.stateMovingResource_placingResourceProgress);

        human.stateMovingResource = MRState.PlacingResource;

        data.map.onHumanTransporterStartedPlacingResource.OnNext(new() {
            Human = human,
            Resource = human.stateMovingResource_targetedResource,
        });
    }

    public void OnExit(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.stateMovingResource_targetedResource, null);
        human.stateMovingResource_placingResourceElapsed = 0;
        human.stateMovingResource_placingResourceProgress = 0;
    }

    public void Update(Entities.HumanTransporter human, HumanTransporterData data, float dt) {
        human.stateMovingResource_placingResourceElapsed += dt;
        human.stateMovingResource_placingResourceProgress =
            human.stateMovingResource_placingResourceElapsed / data.PlacingResourceDuration;

        if (human.stateMovingResource_placingResourceProgress < 1) {
            return;
        }

        human.stateMovingResource_placingResourceElapsed = data.PlacingResourceDuration;
        human.stateMovingResource_placingResourceProgress = 1;

        var res = human.stateMovingResource_targetedResource;

        Building building = null;
        if (res!.Booking != null) {
            var b = res!.Booking.Value.Building;
            if (b.pos == human.pos) {
                building = b;
            }
        }

        human.stateMovingResource_targetedResource = null;

        data.transportationSystem.OnHumanPlacedResource(human.pos, human.segment, res!);
        data.map.onHumanTransporterPlacedResource.OnNext(new() {
            Human = human,
            Resource = res!,
            Building = building,
        });

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.HumanTransporter human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    readonly MovingResources _controller;
}
}
