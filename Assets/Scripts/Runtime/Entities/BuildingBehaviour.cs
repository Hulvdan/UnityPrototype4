#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour : MonoBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void OnEnter(Building building, BuildingDatabase db) {
    }

    public virtual void OnExit(Building building, BuildingDatabase db) {
    }

    public virtual void Update(Building building, BuildingDatabase db, float dt) {
    }
}

public class IdleBuildingBehaviour : BuildingBehaviour {
    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public override void OnEnter(Building building, BuildingDatabase db) {
        Assert.AreEqual(building.idleElapsed, 0);
    }

    public override void OnExit(Building building, BuildingDatabase db) {
        building.idleElapsed = 0;
    }
}

public sealed class TakeResourceBuildingBehaviour : BuildingBehaviour {
}

public sealed class ProcessingBuildingBehaviour : BuildingBehaviour {
}

public sealed class PlaceResourceBuildingBehaviour : BuildingBehaviour {
}

public sealed class OutsourceHumanBuildingBehaviour : BuildingBehaviour {
    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        Assert.IsTrue(_behaviours.Count > 0);

        foreach (var behaviour in _behaviours) {
            if (!behaviour.CanBeRun(building, bdb)) {
                return false;
            }
        }

        return true;
    }

    [SerializeField]
    List<EmployeeBehaviour> _behaviours = new();
}

public abstract class EmployeeBehaviour : MonoBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void BookRequiredTiles(Building building, BuildingDatabase bdb) {
    }

    public virtual void OnEnter(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void OnExit(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void Update(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db,
        float dt
    ) {
    }
}

public sealed class ChooseDestinationEmployeeBehaviour : EmployeeBehaviour {
    [SerializeField]
    HumanDestinationType _type;

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        return _type switch {
            HumanDestinationType.HarvestingTile
                => VisitTilesAroundWorkingArea(building, bdb, CanHarvestAt),
            HumanDestinationType.PlantingTree
                => VisitTilesAroundWorkingArea(building, bdb, CanPlantAt),
            HumanDestinationType.FishingCoast
                => VisitTilesAroundWorkingArea(building, bdb, CanFishCoastAt),
            HumanDestinationType.Building => true,
            _ => throw new NotSupportedException(),
        };
    }

    // Must be called upon building starting its behaviour
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

    static bool HarvestableTileExists(Building building, BuildingDatabase bdb) {
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

    static bool CanPlantTree(Building building, BuildingDatabase bdb) {
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

    static bool CanFishCoastAt(Building building, BuildingDatabase bdb, Vector2Int pos) {
        // TODO(Hulvdan): Implement fishing
        throw new NotImplementedException();
    }
}

public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
}

public sealed class PlacingHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        var res = building.scriptable.harvestableResource;
        Assert.AreNotEqual(res, null);
        var resources = bdb.Map.mapResources[building.posY][building.posX];

        var count = 0;
        foreach (var resource in resources) {
            if (resource.Scriptable == res) {
                count++;
            }
        }

        return count < bdb.MaxHarvestableBuildingSameResourcesOnTheTile;
    }
}

public class HumanDatabase {
    public HumanDatabase(IMapSize mapSize, IMap map) {
        MapSize = mapSize;
        Map = map;
    }

    public IMap Map;
    public IMapSize MapSize;
}
}
