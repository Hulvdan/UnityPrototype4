#nullable enable

namespace BFG.Runtime.Entities {
public class BuildingDatabase {
    public BuildingDatabase(IMap map, IMapSize mapSize) {
        Map = map;
        MapSize = mapSize;
    }

    public IMap Map;
    public IMapSize MapSize;

    public int MaxHarvestableBuildingSameResourcesOnTheTile = 8;
}
}
