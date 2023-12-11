#nullable enable

namespace BFG.Runtime.Entities {
public class BuildingController {
    public BuildingController(BuildingDatabase bdb) {
        _bdb = bdb;
        bdb.Controller = this;
    }

    public void OutsourceEmployee(Building building, EmployeeBehaviourSet behaviourSet) {
        _bdb.Map.CreateEmployee(building, behaviourSet);
    }

    public void SwitchToTheNextBehaviour(Building building) {
        building.CurrentBehaviourIndex++;
        if (building.CurrentBehaviourIndex >= building.scriptable.buildingBehaviours.Count) {
            building.CurrentBehaviourIndex = -1;
            // TODO: Event building finished processing cycle
            return;
        }

        var newBeh = building.scriptable.buildingBehaviours[building.CurrentBehaviourIndex];
        newBeh.OnEnter(building, _bdb);
    }

    public void Update(Building building, float dt) {
        if (
            building.scriptable.type
            is BuildingType.Fish
            or BuildingType.Harvest
            or BuildingType.Plant
        ) {
            return;
        }

        if (building.CanStartProcessingCycle(_bdb)) {
            building.StartProcessingCycle(_bdb);
        }

        if (building.CurrentBehaviourIndex != -1) {
            var beh = building.scriptable.buildingBehaviours[building.CurrentBehaviourIndex];
            beh.UpdateDt(building, _bdb, dt);
        }
    }

    readonly BuildingDatabase _bdb;
}
}
