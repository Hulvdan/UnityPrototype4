#nullable enable
using System;
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

    // Must be called upon building starting its processing cycle
    public override void BookRequiredTiles(
        int behaviourId,
        Building building,
        BuildingDatabase bdb
    ) {
        switch (building.scriptable.type) {
            case BuildingType.Harvest:
                BookHarvestTile(behaviourId, building, bdb);
                break;
            case BuildingType.Plant:
                BookPlantTile(behaviourId, building, bdb);
                break;
            case BuildingType.Fish:
                // TODO(Hulvdan): Implement fishing
                throw new NotImplementedException();
            case BuildingType.Produce:
            case BuildingType.SpecialCityHall:
                throw new NotSupportedException();
            default:
                throw new NotImplementedException();
        }
    }

    void BookHarvestTile(int behaviourIndex, Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        // TODO(Hulvdan): Randomization
        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                var pos = new Vector2Int(x, y);
                if (!CanHarvestAt(building, bdb, pos)) {
                    continue;
                }

                building.BookedTiles.Add(new(behaviourIndex, pos));
                bdb.Map.bookedTiles.Add(pos);
                return;
            }
        }

        Assert.IsTrue(false);
    }

    static void BookPlantTile(int behaviourIndex, Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        // TODO(Hulvdan): Randomization
        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                if (!CanPlantAt(building, bdb, new(x, y))) {
                    continue;
                }

                var tilePos = new Vector2Int(x, y);
                building.BookedTiles.Add(new(behaviourIndex, tilePos));
                bdb.Map.bookedTiles.Add(tilePos);
                return;
            }
        }
    }

    public override void OnEnter(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        Assert.IsTrue(CanBeRun(behaviourId, building, bdb));

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

    static Vector2Int? VisitTilesAroundWorkingArea(
        Building building,
        BuildingDatabase bdb,
        Func<Building, BuildingDatabase, Vector2Int, bool> function
    ) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
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
