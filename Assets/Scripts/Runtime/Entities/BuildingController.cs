#nullable enable

using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class BuildingController {
    public BuildingController(BuildingDatabase bdb) {
        _bdb = bdb;
        bdb.controller = this;
    }

    public void OutsourceEmployee(Building building, EmployeeBehaviourSet behaviourSet) {
        _bdb.map.CreateHuman_Employee_ForTheNextProcessingCycle(building, behaviourSet);
    }

    public void SwitchToTheNextBehaviour(Building building) {
        building.currentBehaviourIndex++;
        if (building.currentBehaviourIndex >= building.scriptable.behaviours.Count) {
            building.currentBehaviourIndex = -1;
            // TODO(Hulvdan): Event building finished processing cycle
            return;
        }

        var newBeh = building.scriptable.behaviours[building.currentBehaviourIndex];
        newBeh.OnEnter(building, _bdb);
    }

    public void Update(Building building, float dt) {
        if (
            building.scriptable.type
            is not (
            BuildingType.Fish
            or BuildingType.Harvest
            or BuildingType.Plant
            )
        ) {
            return;
        }

        if (building.CanStartProcessingCycle(_bdb)) {
            Assert.AreEqual(building.currentBehaviourIndex, -1);

            foreach (var beh in building.scriptable.behaviours) {
                beh.BookRequiredTiles(building, _bdb);
            }

            SwitchToTheNextBehaviour(building);
        }

        if (building.currentBehaviourIndex != -1) {
            var beh = building.scriptable.behaviours[building.currentBehaviourIndex];
            beh.UpdateDt(building, _bdb, dt);
        }
    }

    readonly BuildingDatabase _bdb;
}
}
