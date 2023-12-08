#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour : MonoBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase db) {
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
    public override bool CanBeRun(Building building, BuildingDatabase db) {
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
    public override bool CanBeRun(Building building, BuildingDatabase db) {
        Assert.IsTrue(_behaviours.Count > 0);

        foreach (var behaviour in _behaviours) {
            if (!behaviour.CanBeRun(building, db)) {
                return false;
            }
        }

        return true;
    }

    [SerializeField]
    List<EmployeeBehaviour> _behaviours = new();
}

public abstract class EmployeeBehaviour : MonoBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase db) {
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
            HumanDestinationType.HarvestingTile => HarvestableTileExists(building, bdb),
            HumanDestinationType.PlantingTree => CanPlantTree(building, bdb),
            HumanDestinationType.FishingCoast => CanFishCoast(building, bdb),
            HumanDestinationType.Building => true,
            _ => throw new NotSupportedException(),
        };
    }

    // Must be called upon building starting its behaviour
    public void BookRequiredTiles(Building building, BuildingDatabase bdb) {
        switch (building.scriptable.type) {
            case BuildingType.Harvest:
                break;
            case BuildingType.Plant:
                break;
            case BuildingType.Fish:
                break;
            case BuildingType.Produce:
            case BuildingType.SpecialCityHall:
            default:
                throw new NotSupportedException();
        }

        if (building.scriptable.type == BuildingType.Plant) {
            var bottomLeft = building.workingAreaBottomLeftPos;
            var size = building.scriptable.WorkingAreaSize;

            BookPlantTile(building, bdb, bottomLeft, size);
        }
    }

    static void BookPlantTile(
        Building building,
        BuildingDatabase bdb,
        Vector2Int bottomLeft,
        Vector2Int size
    ) {
        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                var tile = bdb.Map.terrainTiles[y][x];
                var tilePos = new Vector2Int(x, y);
                if (
                    tile.Resource != building.scriptable.harvestableResource
                    || bdb.Map.bookedTiles.Contains(tilePos)
                ) {
                    continue;
                }

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

    bool HarvestableTileExists(Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = bottomLeft.y; y < size.y; y++) {
            for (var x = bottomLeft.x; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }

                var tile = bdb.Map.terrainTiles[y][x];
                if (tile.Resource == building.scriptable.harvestableResource) {
                    return true;
                }
            }
        }

        return false;
    }

    bool CanPlantTree(Building building, BuildingDatabase bdb) {
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

                if (!CanPlant(bdb, y, x)) {
                    continue;
                }

                return true;
            }
        }

        return false;
    }

    static bool CanPlant(BuildingDatabase bdb, int y, int x) {
        var tile = bdb.Map.terrainTiles[y][x];
        var tileBelow = bdb.Map.terrainTiles[y - 1][x];

        // `tile` is a cliff
        if (tileBelow.Height < tile.Height) {
            return false;
        }

        if (tile.Resource != null) {
            return false;
        }

        foreach (var building in bdb.Map.buildings) {
            if (building.Contains(x, y)) {
                return false;
            }
        }

        return true;
    }

    bool CanFishCoast(Building building, BuildingDatabase bdb) {
        // TODO(Hulvdan): Implement fishing
        throw new NotImplementedException();
    }
}

public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
}

public sealed class PlacingHarvestedResourceEmployeeBehaviour : EmployeeBehaviour {
    public override bool CanBeRun(Building building, BuildingDatabase db) {
        var res = building.scriptable.harvestableResource;
        Assert.AreNotEqual(res, null);
        var resources = db.Map.mapResources[building.posY][building.posX];

        var count = 0;
        foreach (var resource in resources) {
            if (resource.Scriptable == res) {
                count++;
            }
        }

        return count < db.MaxHarvestableBuildingSameResourcesOnTheTile;
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
