using BFG.Runtime.Entities;
using BFG.Runtime.Systems;

namespace BFG.Runtime.Controllers.Human {
public class HumanData {
    public ResourceTransportation transportation { get; }
    public IMap map { get; }
    public IMapSize mapSize { get; }
    public Building cityHall { get; }

    public readonly float pickingUpResourceDuration;
    public readonly float placingResourceDuration;

    public readonly float harvestingDelay;
    public readonly float forestHarvestingDuration;

    public HumanData(
        IMap map_,
        IMapSize mapSize_,
        Building cityHall_,
        ResourceTransportation transportation_,
        float pickingUpResourceDuration_,
        float placingResourceDuration_,
        float harvestingDelay_,
        float forestHarvestingDuration_
    ) {
        map = map_;
        mapSize = mapSize_;
        cityHall = cityHall_;
        pickingUpResourceDuration = pickingUpResourceDuration_;
        placingResourceDuration = placingResourceDuration_;
        harvestingDelay = harvestingDelay_;
        forestHarvestingDuration = forestHarvestingDuration_;
        transportation = transportation_;
    }
}
}
