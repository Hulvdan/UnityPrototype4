#nullable enable

namespace BFG.Runtime.Entities {
public sealed class OutsourceHumanBuildingBehaviour : BuildingBehaviour {
    public OutsourceHumanBuildingBehaviour(EmployeeBehaviourSet employeeBehaviourSet) {
        _employeeBehaviourSet = employeeBehaviourSet;
    }

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        return _employeeBehaviourSet.CanBeRun(building, bdb);
    }

    readonly EmployeeBehaviourSet _employeeBehaviourSet;
}
}
