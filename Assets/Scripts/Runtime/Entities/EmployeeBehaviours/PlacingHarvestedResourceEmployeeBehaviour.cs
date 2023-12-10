#nullable enable
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class PlacingHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override bool CanBeRun(int behaviourId, Building building, BuildingDatabase bdb) {
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

    public override void OnEnter(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        // TODO: Event on started placing the resource
    }

    public override void OnExit(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        // TODO: Event on finished placing the resource
    }
}
}
