#nullable enable
namespace BFG.Runtime.Entities {
public class HumanDatabase {
    public HumanDatabase(IMapSize mapSize, IMap map) {
        MapSize = mapSize;
        Map = map;
    }

    public IMap Map;
    public IMapSize MapSize;
}
}
