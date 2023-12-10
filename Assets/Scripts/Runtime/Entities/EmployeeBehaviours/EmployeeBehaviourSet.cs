#nullable enable
using System.Collections.Generic;

namespace BFG.Runtime.Entities {
public sealed class EmployeeBehaviourSet {
    public EmployeeBehaviourSet(List<EmployeeBehaviour> behaviours) {
        _behaviours = behaviours;
    }

    public bool CanBeRun(Building building, BuildingDatabase bdb) {
        for (var i = 0; i < _behaviours.Count; i++) {
            if (!_behaviours[i].CanBeRun(i, building, bdb)) {
                return false;
            }
        }

        return true;
    }

    public void BookRequiredTiles(int behaviourId, Building building, BuildingDatabase bdb) {
        foreach (var beh in _behaviours) {
            beh.BookRequiredTiles(behaviourId, building, bdb);
        }
    }

    readonly List<EmployeeBehaviour> _behaviours;

    public List<EmployeeBehaviour> Behaviours => _behaviours;
}
}
