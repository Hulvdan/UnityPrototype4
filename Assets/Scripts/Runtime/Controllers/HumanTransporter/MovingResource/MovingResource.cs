using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class MovingResource {
    public MovingResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.stateMovingResource, MRState.MovingResource);
        Assert.AreNotEqual(human.stateMovingResource_targetedResource, null);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.TargetedHuman, human);
        Assert.AreEqual(human.stateMovingResource_targetedResource!.CarryingHuman, human);

        human.stateMovingResource = MRState.MovingResource;
    }

    public void OnExit(Entities.HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();
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
    }

    public void OnHumanMovedToTheNextTile(
        Entities.HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.movingTo == null) {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    readonly MovingResources _controller;
}
}
