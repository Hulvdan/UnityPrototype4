using BFG.Runtime.Controllers.Human;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class PickingUpHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreEqual(human.movingResources_pickingUpResourceElapsed, 0);
        Assert.AreEqual(human.movingResources_pickingUpResourceProgress, 0);
        Assert.AreNotEqual(human.building, null);

        var res = db.map.CreateMapResource(
            human.moving.pos, human.building!.scriptable.harvestableResource
        );
        human.movingResources_targetedResource = res;
        db.map.onHumanStartedPickingUpResource.OnNext(new() {
            human = human,
            resource = res,
        });
    }

    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.movingResources_targetedResource, null);

        human.movingResources_pickingUpResourceElapsed = 0;
        human.movingResources_pickingUpResourceProgress = 0;
        db.map.onHumanFinishedPickingUpResource.OnNext(new() {
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
        human.movingResources_pickingUpResourceElapsed += dt;
        human.movingResources_pickingUpResourceProgress =
            human.movingResources_pickingUpResourceElapsed / data.pickingUpResourceDuration;

        if (human.movingResources_pickingUpResourceElapsed >= data.pickingUpResourceDuration) {
            human.movingResources_pickingUpResourceElapsed = data.pickingUpResourceDuration;
            human.movingResources_pickingUpResourceProgress = 1;
            db.controller.SwitchToTheNextBehaviour(human);
        }
    }
}
}
