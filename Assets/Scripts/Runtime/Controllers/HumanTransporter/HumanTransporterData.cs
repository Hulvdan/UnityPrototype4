using BFG.Runtime.Entities;
using BFG.Runtime.Systems;

namespace BFG.Runtime.Controllers.Human {
public class HumanData {
    public ResourceTransportation transportation { get; }
    public IMap map { get; }
    public IMapSize mapSize { get; }
    public Building cityHall { get; }

    public readonly float PickingUpResourceDuration;
    public readonly float PlacingResourceDuration;

    public HumanData(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ResourceTransportation transportation,
        float pickingUpResourceDuration,
        float placingResourceDuration
    ) {
        this.map = map;
        this.mapSize = mapSize;
        this.cityHall = cityHall;
        PickingUpResourceDuration = pickingUpResourceDuration;
        PlacingResourceDuration = placingResourceDuration;
        this.transportation = transportation;
    }
}
}
