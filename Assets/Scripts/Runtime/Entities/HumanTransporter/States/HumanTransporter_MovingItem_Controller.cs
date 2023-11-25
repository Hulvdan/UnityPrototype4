using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HumanTransporterData {
    public readonly IMap Map;
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
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.stateMovingItem, null, "human.stateMovingItem == null");
        Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
        Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");
        Assert.IsTrue(human.segment.resourcesToTransport.Count > 0,
            "human.segment.resourcesToTransport.Count > 0");

        UpdateStates(human, data);
    }

    public void OnExit(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        human.stateMovingItem = null;
        Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

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
            Assert.AreNotEqual(res, null, "human.targetedResource != null");

            human.stateMovingItem_pickingUpResourceElapsed += dt;
            human.stateMovingItem_pickingUpResourceNormalized =
                human.stateMovingItem_pickingUpResourceElapsed / data.PickingUpItemDuration;

            if (human.stateMovingItem_pickingUpResourceNormalized > 1) {
                using var _ = Tracing.Scope();
                Tracing.Log("Human just picked up resource");

                human.stateMovingItem = State.MovingItem;
                Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

                human.stateMovingItem_pickingUpResourceNormalized = 1;
                human.stateMovingItem_pickingUpResourceElapsed = data.PickingUpItemDuration;

                Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
                Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");
                var path = human.segment.Graph.GetShortestPath(
                    human.pos, res!.Value.ItemMovingVertices[0]
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
            Assert.AreNotEqual(res, null, "human.targetedResource != null");

            human.stateMovingItem_placingResourceElapsed += dt;
            human.stateMovingItem_placingResourceNormalized =
                human.stateMovingItem_placingResourceElapsed / data.PlacingItemDuration;

            if (human.stateMovingItem_placingResourceNormalized > 1) {
                using var _ = Tracing.Scope();
                Tracing.Log("Human just placed resource");

                human.stateMovingItem_placingResourceNormalized = 1;
                human.stateMovingItem_placingResourceElapsed = data.PlacingItemDuration;

                var mapResource = res!.Value;
                mapResource.TravellingSegments.RemoveAt(0);
                mapResource.ItemMovingVertices.RemoveAt(0);

                if (res!.Value.TravellingSegments.Count > 0) {
                    // TODO: Handle duplication of code from ItemTransportationSystem
                    // Updating booking. Needs to be changed in Map too
                    mapResource
                        .TravellingSegments[0]
                        .resourcesToTransport
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

                if (human.segment.resourcesToTransport.Count == 0) {
                    Tracing.Log("human.segment.resourcesToTransport.Count == 0");
                    Tracing.Log(
                        "_controller.SetState(human, HumanTransporterState.MovingInTheWorld)");
                    _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
                }
                else {
                    Tracing.Log("human.segment.resourcesToTransport.Count != 0");
                    Tracing.Log("_controller.SetState(human, HumanTransporterState.MovingItem)");
                    _controller.SetState(human, HumanTransporterState.MovingItem);
                }

                return;
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
        using var _ = Tracing.Scope();

        if (human.stateMovingItem == State.MovingItem) {
            var res = human.targetedResource!.Value;

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
        using var _ = Tracing.Scope();

        var segment = human.segment;

        if (human.stateMovingItem == null) {
            Assert.IsTrue(
                human.segment.resourcesToTransport.Count > 0,
                "human.segment.resourcesToTransport.Count > 0"
            );

            var resource = segment.resourcesToTransport.Peek();
            human.targetedResource = resource;
            if (resource.Pos == human.pos && human.movingTo == null) {
                StartPickingUpItem(human, data);
            }
            else {
                Tracing.Log("Human started moving to item");

                human.stateMovingItem = State.MovingToItem;
                Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

                Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
                Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");
                if (resource.Pos != human.pos) {
                    human.AddPath(segment.Graph.GetShortestPath(human.pos, resource.Pos));
                }
            }
        }

        if (human.stateMovingItem == State.MovingToItem) {
            if (human.segment.resourcesToTransport.Peek().Pos == human.pos) {
                StartPickingUpItem(human, data);
            }
        }

        if (human.stateMovingItem == State.MovingItem) {
            Assert.IsTrue(
                human.targetedResource != null, "Assert.IsTrue(human.targetedResource != null);"
            );
            var res = human.targetedResource!.Value;

            if (human.pos == res.ItemMovingVertices[0]) {
                Tracing.Log("Human started placing item");

                human.stateMovingItem = State.PlacingItem;
                Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

                data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
                    Human = human,
                });
            }
        }
    }

    void StartPickingUpItem(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();
        Tracing.Log("Human started picking up item");

        human.stateMovingItem = State.PickingUpItem;
        Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

        human.segment.resourcesToTransport.Dequeue();

        data.Map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
            Human = human,
        });
    }

    readonly HumanTransporter_Controller _controller;
}
}
