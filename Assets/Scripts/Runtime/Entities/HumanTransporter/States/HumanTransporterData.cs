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
}
