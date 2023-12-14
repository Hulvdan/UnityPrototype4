#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class EmployeeBehaviourSet {
    static readonly List<Vector2Int> _TEMP_BOOKED_TILES = new();

    public EmployeeBehaviourSet(List<EmployeeBehaviour> behaviours_) {
        behaviours = behaviours_;
    }

    public bool CanBeRun(Building building, BuildingDatabase bdb) {
        Assert.IsTrue(behaviours.Count > 0);

        for (var i = 0; i < behaviours.Count; i++) {
            if (!behaviours[i].CanBeRun(i, building, bdb, _TEMP_BOOKED_TILES)) {
                _TEMP_BOOKED_TILES.Clear();
                return false;
            }
        }

        _TEMP_BOOKED_TILES.Clear();
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
