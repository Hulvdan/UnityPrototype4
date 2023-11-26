using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HumanTransporterData {
    public ResourceTransportationSystem transportationSystem { get; }
    public IMap Map { get; }
    public IMapSize MapSize { get; }
    public Building CityHall { get; }

    public readonly float PickingUpResourceDuration;
    public readonly float PlacingResourceDuration;

    public HumanTransporterData(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ResourceTransportationSystem transportationSystem,
        float pickingUpResourceDuration,
        float placingResourceDuration
    ) {
        Map = map;
        MapSize = mapSize;
        CityHall = cityHall;
        PickingUpResourceDuration = pickingUpResourceDuration;
        PlacingResourceDuration = placingResourceDuration;
        this.transportationSystem = transportationSystem;
    }
}

public class HumanTransporter_MovingResource_Controller {
    public enum State {
        MovingToResource,
        PickingUpResource,
        MovingResource,
        PlacingResource,
    }

    public HumanTransporter_MovingResource_Controller(HumanTransporter_Controller controller) {
        _controller = controller;
    }

    public void OnEnter(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        Assert.AreEqual(human.stateMovingResource, null, "human.stateMovingResource == null");
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

        Assert.IsTrue(human.stateMovingResource is State.MovingToResource or State.PlacingResource);

        human.stateMovingResource = null;
        Tracing.Log("human.stateMovingResource = null;");

        human.targetedResource = null;

        human.stateMovingResource_pickingUpResourceElapsed = 0;
        human.stateMovingResource_placingResourceElapsed = 0;
        human.stateMovingResource_pickingUpResourceNormalized = 0;
        human.stateMovingResource_placingResourceNormalized = 0;
    }

    public void Update(
        HumanTransporter human,
        HumanTransporterData data,
        float dt
    ) {
        var res = human.targetedResource;

        if (human.stateMovingResource == State.PickingUpResource) {
            Assert.AreNotEqual(res, null, "human.targetedResource != null");

            human.stateMovingResource_pickingUpResourceElapsed += dt;
            human.stateMovingResource_pickingUpResourceNormalized =
                human.stateMovingResource_pickingUpResourceElapsed / data.PickingUpResourceDuration;

            if (human.stateMovingResource_pickingUpResourceNormalized > 1) {
                using var _ = Tracing.Scope();
                Tracing.Log("Human just picked up resource");

                human.stateMovingResource = State.MovingResource;
                Tracing.Log("human.stateMovingResource = State.MovingResource;");

                human.stateMovingResource_pickingUpResourceNormalized = 1;
                human.stateMovingResource_pickingUpResourceElapsed = data.PickingUpResourceDuration;

                Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
                Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");
                var path = human.segment.Graph.GetShortestPath(
                    human.pos, res!.Value.TransportationVertices[0]
                );
                human.AddPath(path);

                data.Map.onHumanTransporterPickedUpResource.OnNext(new() {
                    Human = human,
                    Resource = res.Value,
                });

                human.stateMovingResource_pickingUpResourceNormalized = 0;
                human.stateMovingResource_pickingUpResourceElapsed = 0;
            }
        }

        if (human.stateMovingResource == State.PlacingResource) {
            Assert.AreNotEqual(res, null, "human.targetedResource != null");

            human.stateMovingResource_placingResourceElapsed += dt;
            human.stateMovingResource_placingResourceNormalized =
                human.stateMovingResource_placingResourceElapsed / data.PlacingResourceDuration;

            if (human.stateMovingResource_placingResourceNormalized > 1) {
                using var _ = Tracing.Scope();
                Tracing.Log("Human just placed resource");

                human.stateMovingResource_placingResourceNormalized = 1;
                human.stateMovingResource_placingResourceElapsed = data.PlacingResourceDuration;

                data.transportationSystem.OnHumanPlacedResource(
                    human.pos, human.segment, res!.Value
                );
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
                    Tracing.Log(
                        "_controller.SetState(human, HumanTransporterState.MovingResource)");
                    _controller.SetState(human, HumanTransporterState.MovingResource);
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

        if (human.stateMovingResource == State.MovingToResource) {
            Tracing.Log("_controller.SetState(human, HumanTransporterState.MovingInTheWorld)");

            _controller.SetState(human, HumanTransporterState.MovingInTheWorld);
        }
    }

    public void OnHumanMovedToTheNextTile(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        if (human.stateMovingResource == State.MovingResource) {
            UpdateStates(human, data);
        }
    }

    void UpdateStates(
        HumanTransporter human,
        HumanTransporterData data
    ) {
        using var _ = Tracing.Scope();

        var segment = human.segment;

        if (human.stateMovingResource == null) {
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
                Tracing.Log("Human started moving to resource");

                human.stateMovingResource = State.MovingToResource;
                Tracing.Log("human.stateMovingResource = State.MovingToResource");

                Assert.AreEqual(human.movingTo, null, "human.movingTo == null");
                Assert.AreEqual(human.movingPath.Count, 0, "human.movingPath.Count == 0");
                if (resource.Pos != human.pos) {
                    human.AddPath(segment.Graph.GetShortestPath(human.pos, resource.Pos));
                }
            }
        }

        if (human.stateMovingResource == State.MovingToResource) {
            if (human.segment.resourcesToTransport.Peek().Pos == human.pos) {
                OnHumanStartedPickingUpResource(human, data);
            }
        }

        if (human.stateMovingResource == State.MovingResource) {
            Assert.IsTrue(
                human.targetedResource != null, "Assert.IsTrue(human.targetedResource != null);"
            );
            var res = human.targetedResource!.Value;

            if (human.movingTo == null) {
                Tracing.Log("Human started placing resource");

                human.stateMovingResource = State.PlacingResource;
                Tracing.Log("human.stateMovingResource = State.PlacingResource");

                data.Map.onHumanTransporterStartedPlacingResource.OnNext(new() {
                    Human = human,
                    Resource = res,
                });
            }
        }
    }

    void OnHumanStartedPickingUpResource(HumanTransporter human, HumanTransporterData data) {
        using var _ = Tracing.Scope();

        human.stateMovingResource = State.PickingUpResource;
        Tracing.Log("human.stateMovingResource = State.PickingUpResource");

        var resource = human.segment.resourcesToTransport.Dequeue();
        data.transportationSystem.OnHumanStartedPickingUpResource(resource);

        data.Map.onHumanTransporterStartedPickingUpResource.OnNext(new() {
            Human = human,
            Resource = resource,
        });
    }

    readonly HumanTransporter_Controller _controller;
}
}
