using System;
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

    public readonly float HarvestingDelay;
    public readonly float ForestHarvestingDuration;

    public HumanData(
        IMap map,
        IMapSize mapSize,
        Building cityHall,
        ResourceTransportation transportation,
        float pickingUpResourceDuration,
        float placingResourceDuration,
        float harvestingDelay,
        float forestHarvestingDuration
    ) {
        this.map = map;
        this.mapSize = mapSize;
        this.cityHall = cityHall;
        PickingUpResourceDuration = pickingUpResourceDuration;
        PlacingResourceDuration = placingResourceDuration;
        HarvestingDelay = harvestingDelay;
        ForestHarvestingDuration = forestHarvestingDuration;
        this.transportation = transportation;
    }
}
}
