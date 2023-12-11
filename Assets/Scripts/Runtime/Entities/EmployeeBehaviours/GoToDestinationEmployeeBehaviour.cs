#nullable enable
using System;
using BFG.Runtime.Controllers.Human;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class GoToDestinationEmployeeBehaviour : EmployeeBehaviour {
    public GoToDestinationEmployeeBehaviour(HumanDestinationType type) {
        _type = type;
    }

    public override bool CanBeRun(int behaviourId, Building building, BuildingDatabase bdb) {
        if (_type == HumanDestinationType.Building) {
            return true;
        }

        var function = GetVisitFunction();
        return VisitTilesAroundWorkingArea(building, bdb, function) != null;
    }

    public override void BookRequiredTiles(
        int behaviourId,
        Building building,
        BuildingDatabase bdb
    ) {
        if (
            _type is not (
            HumanDestinationType.Fishing
            or HumanDestinationType.Harvesting
            or HumanDestinationType.Planting
            )
        ) {
            return;
        }

        var pos = VisitTilesAroundWorkingArea(building, bdb, GetVisitFunction());
        Assert.AreNotEqual(pos, null);

        building.BookedTiles.Add(new(behaviourId, pos!.Value));
        bdb.Map.bookedTiles.Add(pos.Value);
    }

    public override void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.building, null);
        Assert.IsTrue(human.currentBehaviourId >= 0);
        var building = human.building!;

        Assert.IsTrue(CanBeRun(human.currentBehaviourId, building, bdb));

        if (_type == HumanDestinationType.Building) {
            return;
        }

        var tilePos = VisitTilesAroundWorkingArea(building, bdb, GetVisitFunction());
        Assert.AreNotEqual(tilePos, null);

        var path = bdb.Map.FindPath(human.moving.pos, tilePos!.Value, false);
        Assert.IsTrue(path.Success);

        Assert.AreEqual(human.moving.Path.Count, 0);
        human.moving.AddPath(path.Value);
    }

    public override void OnHumanMovedToTheNextTile(Human human, HumanData data, HumanDatabase db) {
        if (human.moving.to == null) {
            db.Controller.SwitchToTheNextBehaviour(human);
        }
    }

    static Vector2Int? VisitTilesAroundWorkingArea(
        Building building,
        BuildingDatabase bdb,
        Func<Building, BuildingDatabase, Vector2Int, bool> function
    ) {
        var bottomLeft = building.WorkingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < bottomLeft.y + size.y; y++) {
            for (var x = bottomLeft.x; x < bottomLeft.x + size.x; x++) {
                var pos = new Vector2Int(x, y);
                if (!bdb.MapSize.Contains(pos)) {
                    continue;
                }

                if (function(building, bdb, pos)) {
                    return pos;
                }
            }
        }

        return null;
    }

    Func<Building, BuildingDatabase, Vector2Int, bool> GetVisitFunction() {
        Assert.AreNotEqual(_type, HumanDestinationType.Building);

        return _type switch {
            HumanDestinationType.Harvesting => CanHarvestAt,
            HumanDestinationType.Planting => CanPlantAt,
            HumanDestinationType.Fishing => CanFishAt,
            HumanDestinationType.Building => throw new NotSupportedException(),
            _ => throw new NotImplementedException(),
        };
    }

    static bool CanPlantAt(Building _, BuildingDatabase bdb, Vector2Int pos) {
        if (pos.y == 0) {
            return false;
        }

        var tile = bdb.Map.terrainTiles[pos.y][pos.x];
        var tileBelow = bdb.Map.terrainTiles[pos.y - 1][pos.x];

        // `tile` is a cliff
        if (tileBelow.Height < tile.Height) {
            return false;
        }

        if (tile.Resource != null) {
            return false;
        }

        foreach (var b in bdb.Map.buildings) {
            if (b.Contains(pos.x, pos.y)) {
                return false;
            }
        }

        return true;
    }

    static bool CanHarvestAt(Building building, BuildingDatabase bdb, Vector2Int pos) {
        var tile = bdb.Map.terrainTiles[pos.y][pos.x];
        return tile.Resource == building.scriptable.harvestableResource;
    }

    static bool CanFishAt(Building building, BuildingDatabase bdb, Vector2Int pos) {
        // TODO(Hulvdan): Implement fishing
        throw new NotImplementedException();
    }

    readonly HumanDestinationType _type;
}
}
