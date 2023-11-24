using JetBrains.Annotations;
using UnityEngine.AI;

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
    }

    public void OnExit(
        HumanTransporter human,
        IMap map,
        IMapSize mapSize,
        Building cityHall
    ) {
        human.stateMovingInsideSegment = null;
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
