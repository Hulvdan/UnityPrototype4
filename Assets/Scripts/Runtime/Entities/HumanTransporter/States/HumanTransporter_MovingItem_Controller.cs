using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HumanTransporterData {
    public IMap Map;
    public IMapSize MapSize;
    public Building CityHall;

    public readonly float PickingUpItemDuration;
    public readonly float PlacingItemDuration;

    public HumanTransporterData(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        float pickingUpItemDuration,
        float placingItemDuration
    ) {
        Map = map;
        MapSize = mapSize;
        CityHall = cityHall;
        PickingUpItemDuration = pickingUpItemDuration;
        PlacingItemDuration = placingItemDuration;
    }
}

public class HumanTransporter_MovingItem_Controller {
    public enum State {
        MovingToItem,
        PickingUpItem,
        MovingItem,
        PlacingItem,
    }

    public HumanTransporter_MovingItem_Controller(HumanTransporter_Controller controller) {
        _controller = controller;
    }

    public void OnEnter(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        Assert.IsTrue(human.segment.resourcesReadyToBeTransported.Count > 0);
        UpdateStates(human, data);
    }

    public void OnExit(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        human.stateMovingItem = null;
        human.targetedResource = null;
        human.stateMovingItem_pickingUpResourceElapsed = 0;
        human.stateMovingItem_placingResourceElapsed = 0;
        human.stateMovingItem_pickingUpResourceNormalized = 0;
        human.stateMovingItem_placingResourceNormalized = 0;
    }

    public void Update(
        HumanTransporter human,
        HumanTransporterData data,
        float dt
    ) {
        if (human.stateMovingItem == State.PickingUpItem) {
            if (human.stateMovingItem_pickingUpResourceElapsed == 0) {
            }

            human.stateMovingItem_pickingUpResourceElapsed += dt;
            human.stateMovingItem_pickingUpResourceNormalized =
                human.stateMovingItem_pickingUpResourceElapsed / data.PickingUpItemDuration;

            if (human.stateMovingItem_pickingUpResourceNormalized > 1) {
                human.stateMovingItem_pickingUpResourceNormalized = 1;
                human.stateMovingItem_pickingUpResourceElapsed = data.PickingUpItemDuration;
                human.stateMovingItem = State.MovingItem;

                data.Map.onHumanTransporterPickedUpResource.OnNext(new() {
                    Human = human,
                });
            }
        }

        if (human.stateMovingItem == State.PlacingItem) {
            human.stateMovingItem_placingResourceElapsed += dt;
            human.stateMovingItem_placingResourceNormalized =
                human.stateMovingItem_placingResourceElapsed / data.PlacingItemDuration;

            if (human.stateMovingItem_placingResourceNormalized > 1) {
                human.stateMovingItem_placingResourceNormalized = 1;
                human.stateMovingItem_placingResourceElapsed = data.PlacingItemDuration;

                _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
                data.Map.onHumanTransporterPlacedResource.OnNext(new() {
                    Human = human,
                });
            }
        }

        UpdateStates(human, data);
    }

    public void OnSegmentChanged(
        HumanTransporter human,
        HumanTransporterData data,
        [CanBeNull]
        GraphSegment oldSegment
    ) {
    }

    void UpdateStates(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        var segment = human.segment;

        if (human.stateMovingItem == null) {
            Assert.IsTrue(human.segment.resourcesReadyToBeTransported.Count > 0);

            var resource = segment.resourcesReadyToBeTransported.Peek();
            human.targetedResource = resource;
            if (resource.Pos == human.pos) {
                human.stateMovingItem = State.PickingUpItem;
                human.segment.resourcesReadyToBeTransported.Dequeue();

                data.Map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
                    Human = human,
                });
            }
            else {
                human.stateMovingItem = State.MovingToItem;
                human.AddPath(segment.Graph.GetShortestPath(human.pos, resource.Pos));
            }
        }

        if (human.stateMovingItem == State.MovingToItem) {
            if (human.segment.resourcesReadyToBeTransported.Peek().Pos == human.pos) {
                human.stateMovingItem = State.PickingUpItem;
            }
        }

        if (human.stateMovingItem == State.MovingItem) {
            if (human.justStartedMovingItem) {
                Assert.IsTrue(human.targetedResource != null);
                Assert.AreEqual(human.movingPath.Count, 0);

                var path = segment.Graph.GetShortestPath(
                    human.pos, human.targetedResource.Value.ItemMovingVertices[0]
                );
                human.AddPath(path);
            }
        }
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        if (human.stateMovingItem == State.MovingItem) {
            var res = human.targetedResource.Value;

            var i = 0;
            var row = data.Map.mapResources[res.Pos.y][res.Pos.x];
            foreach (var resource in row) {
                if (resource.ID == res.ID) {
                    row.RemoveAt(i);
                    break;
                }

                i++;
            }

            data.Map.mapResources[human.pos.y][human.pos.x].Add(res);

            if (human.pos == res.ItemMovingVertices[0]) {
                human.stateMovingItem = State.PlacingItem;
                res.TravellingSegments.RemoveAt(0);

                data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
                    Human = human,
                });
            }
        }
    }

    readonly HumanTransporter_Controller _controller;
}
}
