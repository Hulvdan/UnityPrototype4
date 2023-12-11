#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class EmployeeBehaviourSet {
    static readonly List<Vector2Int> TempBookedTiles = new();

    public EmployeeBehaviourSet(List<EmployeeBehaviour> behaviours) {
        this.behaviours = behaviours;
    }

    public bool CanBeRun(Building building, BuildingDatabase bdb) {
        Assert.IsTrue(behaviours.Count > 0);

        for (var i = 0; i < behaviours.Count; i++) {
            if (!behaviours[i].CanBeRun(i, building, bdb, TempBookedTiles)) {
                TempBookedTiles.Clear();
                return false;
            }
        }

        TempBookedTiles.Clear();
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
