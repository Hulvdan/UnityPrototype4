#nullable enable
namespace BFG.Runtime.Entities {
public class BuildingDatabase {
    public IMap Map;
    public float CoalMiningDuration = 2f;

    public BuildingDatabase(IMap map) {
        Map = map;
    }
}
}
