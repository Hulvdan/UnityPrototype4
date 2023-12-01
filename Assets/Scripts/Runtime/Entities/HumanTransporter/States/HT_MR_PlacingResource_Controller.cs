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

        Assert.AreEqual(human.stateMovingResource, MRState.PlacingResource);
        human.stateMovingResource = MRState.PlacingResource;
    }

    public void OnExit(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.stateMovingResource_placingResourceElapsed = 0;
        human.stateMovingResource_placingResourceNormalized = 0;
    }

    public void Update(HumanTransporter human, HumanTransporterData data, float dt) {
        if (human.stateMovingResource_placingResourceNormalized > 1) {
            var res = human.stateMovingResource_targetedResource;
            human.stateMovingResource_targetedResource = null;
        }
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
