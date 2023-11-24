using JetBrains.Annotations;
using UnityEngine.Android;
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
        var res = human.targetedResource;

        if (human.stateMovingItem == State.PickingUpItem) {
            Assert.AreNotEqual(res, null);

            human.stateMovingItem_pickingUpResourceElapsed += dt;
            human.stateMovingItem_pickingUpResourceNormalized =
                human.stateMovingItem_pickingUpResourceElapsed / data.PickingUpItemDuration;

            if (human.stateMovingItem_pickingUpResourceNormalized > 1) {
                human.stateMovingItem_pickingUpResourceNormalized = 1;
                human.stateMovingItem_pickingUpResourceElapsed = data.PickingUpItemDuration;
                human.stateMovingItem = State.MovingItem;

                Assert.AreEqual(human.movingPath.Count, 0);
                var path = human.segment.Graph.GetShortestPath(
                    human.pos, res.Value.ItemMovingVertices[0]
                );
                human.AddPath(path);

                data.Map.onHumanTransporterPickedUpResource.OnNext(new() {
                    Human = human,
                });

                human.stateMovingItem_pickingUpResourceNormalized = 0;
                human.stateMovingItem_pickingUpResourceElapsed = 0;
            }
        }

        if (human.stateMovingItem == State.PlacingItem) {
            Assert.AreNotEqual(res, null);

            human.stateMovingItem_placingResourceElapsed += dt;
            human.stateMovingItem_placingResourceNormalized =
                human.stateMovingItem_placingResourceElapsed / data.PlacingItemDuration;

            if (human.stateMovingItem_placingResourceNormalized > 1) {
                human.stateMovingItem_placingResourceNormalized = 1;
                human.stateMovingItem_placingResourceElapsed = data.PlacingItemDuration;

                _controller.SetState(human, HumanTransporterState.MovingInTheWorld);

                res.Value.TravellingSegments.RemoveAt(0);
                res.Value.ItemMovingVertices.RemoveAt(0);

                if (res.Value.TravellingSegments.Count > 0) {
                    var mapResource = res.Value;

                    // TODO: Handle duplication of code from ItemTransportationSystem
                    // Updating booking. Needs to be changed in Map too
                    mapResource.TravellingSegments[0].resourcesReadyToBeTransported
                        .Enqueue(mapResource);

                    var list = data.Map.mapResources[mapResource.Pos.y][mapResource.Pos.x];
                    for (var i = 0; i < list.Count; i++) {
                        if (list[i].ID == mapResource.ID) {
                            list[i] = mapResource;
                            break;
                        }
                    }
                }

                data.Map.onHumanTransporterPlacedResource.OnNext(new() {
                    Human = human,
                });

                human.stateMovingItem_placingResourceNormalized = 0;
                human.stateMovingItem_placingResourceElapsed = 0;
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
            UpdateStates(human, data);
        }
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
                StartPickingUpItem(human, data);
            }
            else {
                human.stateMovingItem = State.MovingToItem;
                human.AddPath(segment.Graph.GetShortestPath(human.pos, resource.Pos));
            }
        }

        if (human.stateMovingItem == State.MovingToItem) {
            if (human.segment.resourcesReadyToBeTransported.Peek().Pos == human.pos) {
                StartPickingUpItem(human, data);
            }
        }

        if (human.stateMovingItem == State.MovingItem) {
            Assert.IsTrue(human.targetedResource != null);
            var res = human.targetedResource.Value;

            if (human.pos == res.ItemMovingVertices[0]) {
                human.stateMovingItem = State.PlacingItem;

                data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
                    Human = human,
                });
            }
        }
    }

    static void StartPickingUpItem(HumanTransporter human, HumanTransporterData data) {
        human.stateMovingItem = State.PickingUpItem;
        human.segment.resourcesReadyToBeTransported.Dequeue();

        data.Map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
            Human = human,
        });
    }

    readonly HumanTransporter_Controller _controller;
}
}
