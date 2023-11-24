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
        Assert.AreEqual(human.movingTo, null);
        Assert.AreEqual(human.movingPath.Count, 0);

        if (human.segment.resourcesReadyToBeTransported.Count == 0) {
            var center = human.segment.Graph.GetCenters()[0];
            var path = map.FindPath(human.movingTo ?? human.pos, center, true).Path;
            human.AddPath(path);
        }
    }

    public void OnExit(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        human.stateMovingInsideSegment = null;
        human.movingTo = null;
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
        if (human.segment.resourcesReadyToBeTransported.Count > 0) {
            _controller.SetState(human, HumanTransporterState.MovingItem);
        }
    }

    readonly HumanTransporter_Controller _controller;
}
}
