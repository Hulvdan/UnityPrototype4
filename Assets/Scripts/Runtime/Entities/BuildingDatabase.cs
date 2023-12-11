#nullable enable

namespace BFG.Runtime.Entities {
public class BuildingDatabase {
    public BuildingDatabase(IMap map, IMapSize mapSize) {
        Map = map;
        MapSize = mapSize;
    }

    public readonly IMap Map;
    public readonly IMapSize MapSize;
    public BuildingController Controller;

    public int MaxHarvestableBuildingSameResourcesOnTheTile = 8;
}
}
