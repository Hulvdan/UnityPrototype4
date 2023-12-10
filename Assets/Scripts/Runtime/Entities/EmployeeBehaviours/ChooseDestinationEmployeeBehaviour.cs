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
        Func<Building, BuildingDatabase, Vector2Int, bool> alg;
        switch (_type) {
            case HumanDestinationType.Harvesting:
                alg = CanHarvestAt;
                break;
            case HumanDestinationType.Planting:
                alg = CanPlantAt;
                break;
            case HumanDestinationType.Fishing:
                alg = CanFishAt;
                break;
            case HumanDestinationType.Building:
                return true;
            default:
                throw new NotImplementedException();
        }

        return VisitTilesAroundWorkingArea(building, bdb, alg);
    }

    // Must be called upon building starting its processing cycle
    public override void BookRequiredTiles(Building building, BuildingDatabase bdb) {
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

                var pos = new Vector2Int(x, y);
                if (!CanHarvestAt(building, bdb, pos)) {
                    continue;
                }

                building.BookedTiles.Add(pos);
                bdb.Map.bookedTiles.Add(pos);
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
