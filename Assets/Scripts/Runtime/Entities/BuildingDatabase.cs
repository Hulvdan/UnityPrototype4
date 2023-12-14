#nullable enable

namespace BFG.Runtime.Entities {
public class BuildingDatabase {
    public BuildingDatabase(IMap map_, IMapSize mapSize_) {
        map = map_;
        mapSize = mapSize_;
    }

    public readonly IMap map;
    public readonly IMapSize mapSize;
    public BuildingController controller;

    public readonly int maxHarvestableBuildingSameResourcesOnTheTile = 8;
}
}
