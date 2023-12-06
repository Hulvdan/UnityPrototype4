using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.HumanTransporter.MovingResources.State;

namespace BFG.Runtime.Controllers.HumanTransporter {
public class MovingResource {
    public MovingResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.Human human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, MRState.MovingResource);
        Assert.AreNotEqual(human.movingResources_targetedResource, null);
        Assert.AreEqual(human.movingResources_targetedResource!.TargetedHuman, human);
        Assert.AreEqual(human.movingResources_targetedResource!.CarryingHuman, human);

        human.movingResources = MRState.MovingResource;
    }

    public void OnExit(Entities.Human human, HumanTransporterData data) {
        using var _ = Tracing.Scope();
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
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.moving.to == null) {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    readonly MovingResources _controller;
}
}
