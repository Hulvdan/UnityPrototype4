using UnityEngine.Assertions;
using MRState = BFG.Runtime.HumanTransporter_MovingResource_Controller.State;

namespace BFG.Runtime {
public class HT_MR_MovingResource_Controller {
    public HT_MR_MovingResource_Controller(
        HumanTransporter_MovingResource_Controller controller
    ) {
        _controller = controller;
    }

    public void OnEnter(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, MRState.MovingResource);
        Assert.AreNotEqual(human.stateMovingResource_targetedResource, null);

        human.stateMovingResource = MRState.MovingResource;
    }

    public void OnExit(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();
    }

    public void Update(HumanTransporter human, HumanTransporterData data, float dt) {
    }

    public void OnHumanCurrentSegmentChanged(
        HumanTransporter human,
        HumanTransporterData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.movingTo == null) {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    readonly HumanTransporter_MovingResource_Controller _controller;
}
}
