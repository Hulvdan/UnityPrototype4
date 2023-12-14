using BFG.Runtime.Controllers.Human;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
    public ProcessingEmployeeBehaviour(int? unbooksBehaviourId) {
        Assert.IsTrue(unbooksBehaviourId >= 0);

        _unbooksBehaviourId = unbooksBehaviourId;
    }

    public override void OnEnter(Human human, BuildingDatabase bdb, HumanDatabase db) {
        Assert.AreEqual(human.harvestingElapsed, 0);
        Assert.AreNotEqual(human.building, null);
    }

    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.building, null);
        var building = human.building!;

        if (_unbooksBehaviourId != null) {
            UnbookTilesThatWereBookedBy(db, building, _unbooksBehaviourId.Value);
        }

        human.harvestingElapsed = 0;
    }

    void UnbookTilesThatWereBookedBy(HumanDatabase db, Building building, int behaviourId) {
        var foundIndex = -1;

        for (var i = 0; i < building.bookedTiles.Count; i++) {
            var (tileBehaviourId, _) = building.bookedTiles[i];
            if (tileBehaviourId == behaviourId) {
                foundIndex = i;
                break;
            }
        }

        Assert.IsTrue(foundIndex >= 0);
        db.map.bookedTiles.Remove(building.bookedTiles[foundIndex].Item2);
        building.bookedTiles.RemoveAt(foundIndex);
    }

    public override void UpdateDt(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db,
        HumanData data,
        float dt
    ) {
        Assert.AreNotEqual(human.building, null);
        human.harvestingElapsed += dt;

        var harvestingDuration = human.building!.scriptable.harvestableResource.harvestingDuration;

        if (human.harvestingElapsed >= harvestingDuration) {
            human.harvestingElapsed = harvestingDuration;
            db.controller.SwitchToTheNextBehaviour(human);
        }
    }

    readonly int? _unbooksBehaviourId;
}
}
