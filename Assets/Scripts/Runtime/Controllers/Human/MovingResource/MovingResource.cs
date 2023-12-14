using BFG.Runtime.Graphs;
using UnityEngine.Assertions;
using MRState = BFG.Runtime.Controllers.Human.MovingResources.State;

namespace BFG.Runtime.Controllers.Human {
public class MovingResource {
    public MovingResource(MovingResources controller) {
        _controller = controller;
    }

    public void OnEnter(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.movingResources, MRState.MovingResource);
        Assert.AreNotEqual(human.movingResources_targetedResource, null);
        Assert.AreEqual(human.movingResources_targetedResource!.targetedHuman, human);
        Assert.AreEqual(human.movingResources_targetedResource!.carryingHuman, human);

        human.movingResources = MRState.MovingResource;
    }

    public void OnExit(Entities.Human human, HumanData data) {
        using var _ = Tracing.Scope();
    }

    public void Update(Entities.Human human, HumanData data, float dt) {
        // Hulvdan: Intentionally left blank
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        if (human.moving.to == null) {
            _controller.SetState(human, data, MRState.PlacingResource);
        }
    }

    readonly MovingResources _controller;
}
}
