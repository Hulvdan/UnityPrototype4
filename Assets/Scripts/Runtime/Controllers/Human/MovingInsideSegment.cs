using BFG.Runtime.Graphs;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime.Controllers.Human {
public class MovingInsideSegment {
    public MovingInsideSegment(MainController controller) {
        _controller = controller;
    }

    public void OnEnter(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreNotEqual(human.segment, null, "human.segment != null");
        Assert.AreEqual(human.moving.to, null, "human.movingTo == null");
        Assert.AreEqual(human.moving.path.Count, 0, "human.movingPath.Count == 0");

        if (human.segment!.resourcesToTransport.Count == 0) {
            Tracing.Log("Setting path to center");
            var center = human.segment.graph.GetCenters()[0];
            var path =
                human.segment.graph.GetShortestPath(human.moving.to ?? human.moving.pos,
                    center);
            human.moving.AddPath(path);
        }
    }

    public void OnExit(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();

        human.moving.path.Clear();
    }

    public void Update(
        Entities.Human human,
        HumanData data,
        float dt
    ) {
        UpdateStates(human, data);
    }

    public void OnHumanCurrentSegmentChanged(
        Entities.Human human,
        HumanData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Tracing.Log("_controller.SetState(human, HumanState.MovingInTheWorld)");
        _controller.SetState(human, MainState.MovingInTheWorld);
    }

    public void OnHumanMovedToTheNextTile(
        Entities.Human human,
        HumanData data
    ) {
        // Hulvdan: Intentionally left blank
    }

    void UpdateStates(
        Entities.Human human,
        HumanData data
    ) {
        using var _ = Tracing.Scope();
        if (human.segment == null) {
            human.moving.path.Clear();
            _controller.SetState(human, MainState.MovingInTheWorld);
            return;
        }

        if (human.segment.resourcesToTransport.Count > 0) {
            if (human.moving.to == null) {
                Tracing.Log("_controller.SetState(human, HumanState.MovingItem)");
                _controller.SetState(human, MainState.MovingResource);
                return;
            }

            human.moving.path.Clear();
        }
    }

    readonly MainController _controller;
}
}
