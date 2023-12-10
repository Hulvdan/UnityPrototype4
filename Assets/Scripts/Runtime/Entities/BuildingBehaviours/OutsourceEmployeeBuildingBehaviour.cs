#nullable enable

namespace BFG.Runtime.Entities {
public sealed class OutsourceEmployeeBuildingBehaviour : BuildingBehaviour {
    public OutsourceEmployeeBuildingBehaviour(EmployeeBehaviourSet employeeBehaviourSet) {
        _employeeBehaviourSet = employeeBehaviourSet;
    }

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        return _employeeBehaviourSet.CanBeRun(building, bdb);
    }

    public override void OnEnter(Building building, BuildingDatabase bdb) {
        building.SpawnedHuman = bdb.Controller.CreateEmployee(_employeeBehaviourSet, building.pos);
    }

    public override void OnExit(Building building, BuildingDatabase bdb) {
        // TODO: Remove employee properly
        building.SpawnedHuman = null;
    }

    readonly EmployeeBehaviourSet _employeeBehaviourSet;
}
}
