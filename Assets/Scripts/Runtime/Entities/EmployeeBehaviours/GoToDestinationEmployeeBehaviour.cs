#nullable enable
using System;
using System.Collections.Generic;
using BFG.Runtime.Controllers.Human;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class GoToDestinationEmployeeBehaviour : EmployeeBehaviour {
    public GoToDestinationEmployeeBehaviour(HumanDestinationType type, int? books) {
        _type = type;
        _books = books;
    }

    public override bool CanBeRun(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        List<Vector2Int> tempBookedTiles
    ) {
        if (_type == HumanDestinationType.Building) {
            return true;
        }

        var pos = VisitTilesAroundWorkingArea(building, bdb, GetVisitFunction(), tempBookedTiles);
        return pos != null;
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
            Assert.AreEqual(_books, null);
            return;
        }

        Assert.AreNotEqual(_books, null);

        var pos = VisitTilesAroundWorkingArea(building, bdb, GetVisitFunction(), null);
        Assert.AreNotEqual(pos, null);

        building.bookedTiles.Add(new(_books!.Value, pos!.Value));
        bdb.map.bookedTiles.Add(pos.Value);
    }

    public override void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.building, null);
        Assert.IsTrue(human.currentBehaviourId >= 0);
        var building = human.building!;

        Vector2Int? tilePos = building.pos;

        if (_type != HumanDestinationType.Building) {
            tilePos = VisitTilesAroundWorkingArea(building, bdb, GetVisitFunction(), null);
            Assert.AreNotEqual(tilePos, null);
        }

        var path = bdb.map.FindPath(human.moving.pos, tilePos!.Value, false);
        Assert.IsTrue(path.success);

        Assert.AreEqual(human.moving.path.Count, 0);
        human.moving.AddPath(path.value);
    }

    public override void OnHumanMovedToTheNextTile(Human human, HumanData data, HumanDatabase db) {
        if (human.moving.to == null) {
            db.controller.SwitchToTheNextBehaviour(human);
        }
    }

    static Vector2Int? VisitTilesAroundWorkingArea(
        Building building,
        BuildingDatabase bdb,
        Func<Building, BuildingDatabase, Vector2Int, bool> function,
        List<Vector2Int>? tempBookedTiles
    ) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.workingAreaSize;

        for (var y = bottomLeft.y; y < bottomLeft.y + size.y; y++) {
            for (var x = bottomLeft.x; x < bottomLeft.x + size.x; x++) {
                var pos = new Vector2Int(x, y);
                if (!bdb.mapSize.Contains(pos)) {
                    continue;
                }

                if (tempBookedTiles != null && tempBookedTiles.Contains(pos)) {
                    continue;
                }

                if (function(building, bdb, pos)) {
                    tempBookedTiles?.Add(pos);
                    return pos;
                }
            }
        }

        return null;
    }

    // TODO(Hulvdan): Investigate the ways of rewriting it as a macros / codegen
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

        var tile = bdb.map.terrainTiles[pos.y][pos.x];
        var tileBelow = bdb.map.terrainTiles[pos.y - 1][pos.x];

        // It's a cliff
        if (tileBelow.height < tile.height) {
            return false;
        }

        if (tile.resource != null) {
            return false;
        }

        if (bdb.map.elementTiles[pos.y][pos.x].type != ElementTileType.None) {
            return false;
        }

        return true;
    }

    static bool CanHarvestAt(Building building, BuildingDatabase bdb, Vector2Int pos) {
        var tile = bdb.map.terrainTiles[pos.y][pos.x];
        return tile.resource == building.scriptable.harvestableResource;
    }

    static bool CanFishAt(Building building, BuildingDatabase bdb, Vector2Int pos) {
        // TODO(Hulvdan): Implement fishing
        throw new NotImplementedException();
    }

    readonly HumanDestinationType _type;
    readonly int? _books;
}
}
