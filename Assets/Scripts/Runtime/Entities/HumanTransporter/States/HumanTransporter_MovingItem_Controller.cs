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

        Assert.IsTrue(human.stateMovingItem is State.MovingToItem or State.PlacingItem);

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
                    Resource = res.Value,
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

                OnHumanPlacedResource(human, data, res!.Value);
                data.Map.onHumanTransporterPlacedResource.OnNext(new() {
                    Human = human,
                    Resource = res!.Value,
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
        using var _ = Tracing.Scope();

        Tracing.Log("human.movingPath.Clear()");
        human.movingPath.Clear();

        if (human.stateMovingItem == State.MovingToItem) {
            Tracing.Log("_controller.SetState(human, HumanTransporterState.MovingInTheWorld)");

            _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
        }
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.stateMovingItem == State.MovingItem) {
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
                OnHumanStartedPickingUpResource(human, data);
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
                OnHumanStartedPickingUpResource(human, data);
            }
        }

        if (human.stateMovingItem == State.MovingItem) {
            Assert.IsTrue(
                human.targetedResource != null, "Assert.IsTrue(human.targetedResource != null);"
            );
            var res = human.targetedResource!.Value;

            if (human.movingTo == null) {
                Tracing.Log("Human started placing item");

                human.stateMovingItem = State.PlacingItem;
                Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

                data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
                    Human = human,
                    Resource = res,
                });
            }
        }
    }

    void OnHumanStartedPickingUpResource(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();
        Tracing.Log("Human started picking up item");

        human.stateMovingItem = State.PickingUpItem;
        Tracing.Log($"human.stateMovingItem = {human.stateMovingItem}");

        var resource = human.segment.resourcesToTransport.Dequeue();
        data.Map.mapResources[resource.Pos.y][resource.Pos.x].Remove(resource);

        data.Map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
            Human = human,
            Resource = resource,
        });
    }

    static void OnHumanPlacedResource(
        HumanTransporter human,
        HumanTransporterData data,
        MapResource res
    ) {
        if (human.pos == res.ItemMovingVertices[0]) {
            res.TravellingSegments[0]
                .resourcesWithThisSegmentInPath
                .Remove(res);
        }

        res.Pos = human.pos;
        var movVert = res.ItemMovingVertices[0];
        res.TravellingSegments.RemoveAt(0);
        res.ItemMovingVertices.RemoveAt(0);

        var movedToTheNextSegmentInPath = res.Pos == movVert
                                          && res.TravellingSegments.Count > 0;
        var movedInsideBuilding = res.Booking != null
                                  && res.Booking.Value.Building.pos == human.pos;

        if (movedToTheNextSegmentInPath) {
            // TODO: Handle duplication of code from ItemTransportationSystem
            res
                .TravellingSegments[0]
                .resourcesToTransport
                .Enqueue(res);

            var list = data.Map.mapResources[res.Pos.y][res.Pos.x];
            for (var i = 0; i < list.Count; i++) {
                if (list[i].ID == res.ID) {
                    list[i] = res;
                    break;
                }
            }
        }
        else if (movedInsideBuilding) {
            res.Booking.Value.Building.resourcesForConstruction.Add(res);
            human.segment.resourcesWithThisSegmentInPath.Remove(res);

            res.Booking = null;
        }
        // Resource was just placed on the map
        else {
            foreach (var segment in res.TravellingSegments) {
                res.Booking = null;
                segment.resourcesWithThisSegmentInPath.Remove(res);
                data.Map.mapResources[res.Pos.y][res.Pos.x].Add(res);
            }

            res.TravellingSegments.Clear();
            res.ItemMovingVertices.Clear();
        }
    }

    readonly HumanTransporter_Controller _controller;
}
}
