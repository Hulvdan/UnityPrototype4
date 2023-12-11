#nullable enable
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class EmployeeBehaviourSet {
    public EmployeeBehaviourSet(List<EmployeeBehaviour> behaviours) {
        this.behaviours = behaviours;
    }

    public bool CanBeRun(Building building, BuildingDatabase bdb) {
        Assert.IsTrue(behaviours.Count > 0);

        for (var i = 0; i < behaviours.Count; i++) {
            if (!behaviours[i].CanBeRun(i, building, bdb)) {
                return false;
            }
        }

        return true;
    }

    public void BookRequiredTiles(Building building, BuildingDatabase bdb) {
        for (var i = 0; i < behaviours.Count; i++) {
            var beh = behaviours[i];
            beh.BookRequiredTiles(i, building, bdb);
        }
    }

    public List<EmployeeBehaviour> behaviours { get; }
}
}
