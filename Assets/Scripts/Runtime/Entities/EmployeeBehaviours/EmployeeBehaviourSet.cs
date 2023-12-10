#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime.Entities {
public sealed class EmployeeBehaviourSet {
    public EmployeeBehaviourSet(List<EmployeeBehaviour> behaviours) {
        Behaviours = behaviours;
    }

    public bool CanBeRun(Building building, BuildingDatabase bdb) {
        for (var i = 0; i < Behaviours.Count; i++) {
            if (!Behaviours[i].CanBeRun(i, building, bdb)) {
                return false;
            }
        }

        return true;
    }

    public void BookRequiredTiles(Building building, BuildingDatabase bdb) {
    }

    public readonly List<EmployeeBehaviour> Behaviours;
}
}
