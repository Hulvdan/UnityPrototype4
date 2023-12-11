#nullable enable
namespace BFG.Runtime.Entities {
public class HumanDatabase {
    public HumanDatabase(IMapSize mapSize, IMap map) {
        MapSize = mapSize;
        Map = map;
    }

    public readonly IMap Map;
    public readonly IMapSize MapSize;
    public EmployeeController Controller;
}
}
