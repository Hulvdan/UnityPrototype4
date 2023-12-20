using System;
using BFG.Runtime.Controllers.Human;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
    public ProcessingEmployeeBehaviour(int? unbookingBehaviourId, HumanProcessingType type) {
        Assert.IsTrue(unbookingBehaviourId >= 0);

        _unbookingBehaviourId = unbookingBehaviourId;
        _type = type;
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

        if (_unbookingBehaviourId != null) {
            UnbookTilesThatWereBookedBy(db, building, _unbookingBehaviourId.Value);
            db.map.EmployeeFinishedProcessingCallback(human, _type);
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

        human.processingElapsed += dt;

        var processingDuration = GetProcessingDuration(human.building!.scriptable);

        if (human.processingElapsed >= processingDuration) {
            human.processingElapsed = processingDuration;
            db.controller.SwitchToTheNextBehaviour(human);
        }
    }

    float GetProcessingDuration(IScriptableBuilding building) {
        if (building.type == BuildingType.Harvest) {
            return building.harvestableResource.harvestingDuration;
        }

        if (building.type == BuildingType.Plant) {
            return building.plantableResource.plantingDuration;
        }

        throw new NotImplementedException();
    }

    readonly int? _unbookingBehaviourId;
    readonly HumanProcessingType _type;
}
}
