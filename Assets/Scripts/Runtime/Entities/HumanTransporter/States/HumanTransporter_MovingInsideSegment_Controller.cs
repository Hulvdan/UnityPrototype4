using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HumanTransporter_MovingInsideSegment_Controller {
    public enum State {
        MovingToCenter,
        Idle,
    }

    public HumanTransporter_MovingInsideSegment_Controller(HumanTransporter_Controller controller) {
        _controller = controller;
    }

    public void OnEnter(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log("OnEnter");

        Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
        Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");

        if (human.segment.resourcesToTransport.Count == 0) {
            Tracing.Log("Setting path to center");
            var center = human.segment.Graph.GetCenters()[0];
            var path = human.segment.Graph.GetShortestPath(human.movingTo ?? human.pos, center);
            human.AddPath(path);
        }
    }

    public void OnExit(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        using var _ = Tracing.Scope();
        Tracing.Log("OnExit");

        human.stateMovingInsideSegment = null;
        human.movingPath.Clear();
    }

    public void Update(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        float dt
    ) {
        UpdateStates(human, map, mapSize, cityHall);
    }

    public void OnSegmentChanged(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
        using var _ = Tracing.Scope();

        Tracing.Log("_controller.SetState(human, HumanTransporterState.MovingInTheWorld)");
        _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
    }

    void UpdateStates(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        using var _ = Tracing.Scope();

        if (human.segment.resourcesToTransport.Count > 0) {
            if (human.movingTo == null) {
                Tracing.Log("_controller.SetState(human, HumanTransporterState.MovingItem)");
                _controller.SetState(human, HumanTransporterState.MovingItem);
                return;
            }

            human.movingPath.Clear();
        }
    }

    readonly HumanTransporter_Controller _controller;
}
}
