#nullable enable
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class GoToDestinationEmployeeBehaviour : EmployeeBehaviour {
    public GoToDestinationEmployeeBehaviour(HumanDestinationType type) {
        _type = type;
    }

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        if (_type == HumanDestinationType.Building) {
            return true;
        }

        Func<Building, BuildingDatabase, Vector2Int, bool> alg = _type switch {
            HumanDestinationType.Harvesting => CanHarvestAt,
            HumanDestinationType.Planting => CanPlantAt,
            HumanDestinationType.Fishing => CanFishAt,
            HumanDestinationType.Building => throw new NotSupportedException(),
            _ => throw new NotImplementedException(),
        };

        return VisitTilesAroundWorkingArea(building, bdb, alg);
    }

    // Must be called upon building starting its processing cycle
    public void BookRequiredTiles(Building building, BuildingDatabase bdb) {
        switch (building.scriptable.type) {
            case BuildingType.Harvest:
                BookHarvestTile(building, bdb);
                break;
            case BuildingType.Plant:
                BookPlantTile(building, bdb);
                break;
            case BuildingType.Fish:
                // TODO(Hulvdan): Implement fishing
                throw new NotSupportedException();
            case BuildingType.Produce:
            case BuildingType.SpecialCityHall:
            default:
                throw new NotSupportedException();
        }
    }

    void BookHarvestTile(Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        // TODO(Hulvdan): Randomization
        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                var tile = bdb.Map.terrainTiles[y][x];
                var tilePos = new Vector2Int(x, y);

                if (!CanHarvestAt(building, bdb, new(x, y))) {
                    continue;
                }

                building.BookedTiles.Add(tilePos);
                bdb.Map.bookedTiles.Add(tilePos);
                return;
            }
        }

        Assert.IsTrue(false);
    }

    static void BookPlantTile(Building building, BuildingDatabase bdb) {
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
                building.BookedTiles.Add(tilePos);
                bdb.Map.bookedTiles.Add(tilePos);
                return;
            }
        }
    }

    public override void OnEnter(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        Assert.IsTrue(CanBeRun(building, bdb));
    }

    static bool VisitTilesAroundWorkingArea(
        Building building,
        BuildingDatabase bdb,
        Func<Building, BuildingDatabase, Vector2Int, bool> function
    ) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                if (function(building, bdb, new(x, y))) {
                    return true;
                }
            }
        }

        return true;
    }

    static bool HarvestableTileExists(
        Building building,
        BuildingDatabase bdb,
        ScriptableResource res
    ) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                if (CanHarvestAt(building, bdb, new(x, y))) {
                    return true;
                }
            }
        }

        return false;
    }

    static bool CanPlant(Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < size.y; y++) {
            // It's always a cliff
            if (y == 0) {
                continue;
            }

            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                if (!CanPlantAt(building, bdb, new(x, y))) {
                    continue;
                }

                return true;
            }
        }

        return false;
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

    HumanDestinationType _type;
}
}
