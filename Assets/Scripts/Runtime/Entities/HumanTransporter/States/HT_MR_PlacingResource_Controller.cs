using UnityEngine.Assertions;
using MRState = BFG.Runtime.HumanTransporter_MovingResource_Controller.State;

namespace BFG.Runtime {
public class HT_MR_PlacingResource_Controller {
    public HT_MR_PlacingResource_Controller(
        HumanTransporter_MovingResource_Controller controller
    ) {
        _controller = controller;
    }

    public void OnEnter(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, MRState.PlacingResource);
        human.stateMovingResource = MRState.PlacingResource;

        data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
            Human = human,
            Resource = human.stateMovingResource_targetedResource,
        });
    }

    public void OnExit(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.stateMovingResource_targetedResource = null;
        human.stateMovingResource_placingResourceElapsed = 0;
        human.stateMovingResource_placingResourceNormalized = 0;
    }

    public void Update(HumanTransporter human, HumanTransporterData data, float dt) {
        human.stateMovingResource_placingResourceElapsed += dt;
        human.stateMovingResource_placingResourceNormalized =
            human.stateMovingResource_placingResourceElapsed / data.PlacingResourceDuration;

        if (human.stateMovingResource_placingResourceNormalized < 1) {
            return;
        }

        human.stateMovingResource_placingResourceElapsed = data.PlacingResourceDuration;
        human.stateMovingResource_placingResourceNormalized = 1;

        var res = human.stateMovingResource_targetedResource;
        human.stateMovingResource_targetedResource = null;

        Building building = null;
        if (res!.Booking != null) {
            var b = res!.Booking.Value.Building;
            if (b.pos == human.pos) {
                building = b;
            }
        }

        data.transportationSystem.OnHumanPlacedResource(human.pos, human.segment, res!, false);
        data.Map.onHumanTransporterPlacedResource.OnNext(new() {
            Human = human,
            Resource = res!,
            Building = building,
        });

        _controller.NestedState_Exit(human, data);
    }

    public void OnHumanCurrentSegmentChanged(
        HumanTransporter human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    readonly HumanTransporter_MovingResource_Controller _controller;
}
}
