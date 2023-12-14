#nullable enable
namespace BFG.Runtime.Entities {
public class HumanDatabase {
    public HumanDatabase(IMapSize mapSize_, IMap map_) {
        mapSize = mapSize_;
        map = map_;
    }

    public readonly IMap map;
    public readonly IMapSize mapSize;
    public EmployeeController controller;
}
}
