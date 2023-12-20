#nullable enable
using System.Collections.Generic;
using BFG.Runtime.Controllers.Human;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class PlacingHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override bool CanBeRun(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        List<Vector2Int> tempBookedTiles
    ) {
        var res = building.scriptable.harvestableResource;
        Assert.AreNotEqual(res, null);
        var resources = bdb.map.mapResources[building.posY][building.posX];

        var count = 0;
        foreach (var resource in resources) {
            if (resource.scriptable == res) {
                count++;
            }
        }

        return count < bdb.maxHarvestableBuildingSameResourcesOnTheTile;
    }

    public override void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreEqual(human.movingResources_placingResourceElapsed, 0);
        Assert.AreEqual(human.movingResources_placingResourceProgress, 0);
        Assert.AreNotEqual(human.building, null);

        db.map.onHumanStartedPlacingResource.OnNext(new() {
            human = human,
            resource = human.movingResources_targetedResource,
        });
    }

    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.movingResources_targetedResource, null);

        human.movingResources_placingResourceElapsed = 0;
        human.movingResources_placingResourceProgress = 0;

        var pos = human.moving.pos;

        // TODO(Hulvdan): Move as a callback to `Map`
        db.map.mapResources[pos.y][pos.x].Add(human.movingResources_targetedResource);

        db.map.onHumanFinishedPlacingResource.OnNext(new() {
            human = human,
            resource = human.movingResources_targetedResource!,
        });
    }

    public override void UpdateDt(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db,
        HumanData data,
        float dt
    ) {
        human.movingResources_placingResourceElapsed += dt;
        human.movingResources_placingResourceProgress =
            human.movingResources_placingResourceElapsed / data.placingResourceDuration;

        if (human.movingResources_placingResourceElapsed >= data.placingResourceDuration) {
            human.movingResources_placingResourceElapsed = data.placingResourceDuration;
            human.movingResources_placingResourceProgress = 1;
            db.controller.SwitchToTheNextBehaviour(human);
        }
    }
}
}
