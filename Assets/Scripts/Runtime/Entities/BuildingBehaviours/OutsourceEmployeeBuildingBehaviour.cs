#nullable enable

namespace BFG.Runtime.Entities {
public sealed class OutsourceEmployeeBuildingBehaviour : BuildingBehaviour {
    public OutsourceEmployeeBuildingBehaviour(EmployeeBehaviourSet employeeBehaviourSet) {
        _employeeBehaviourSet = employeeBehaviourSet;
    }

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        return _employeeBehaviourSet.CanBeRun(building, bdb);
    }

    readonly EmployeeBehaviourSet _employeeBehaviourSet;
}
}
