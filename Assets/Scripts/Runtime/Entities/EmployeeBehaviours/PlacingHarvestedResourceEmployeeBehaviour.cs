#nullable enable
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class PlacingHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        var res = building.scriptable.harvestableResource;
        Assert.AreNotEqual(res, null);
        var resources = bdb.Map.mapResources[building.posY][building.posX];

        var count = 0;
        foreach (var resource in resources) {
            if (resource.Scriptable == res) {
                count++;
            }
        }

        return count < bdb.MaxHarvestableBuildingSameResourcesOnTheTile;
    }
}
}
