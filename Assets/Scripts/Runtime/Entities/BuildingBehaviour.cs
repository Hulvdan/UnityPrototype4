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

    public virtual void OnEnter(Human human, HumanDatabase db) {
    }

    public virtual void OnExit(Human human, HumanDatabase db) {
    }

    public virtual void Update(Human human, HumanDatabase db, float dt) {
    }
}

public sealed class ChooseDestinationEmployeeBehaviour : EmployeeBehaviour {
    [SerializeField]
    HumanDestinationType _type;

    public override bool CanBeRun(Building building, BuildingDatabase bdb) {
        switch (_type) {
            case HumanDestinationType.HarvestingTile:
                return CanHarvestTile(building, bdb);
            case HumanDestinationType.PlantingTree:
                return CanPlantTree(building, bdb);
            case HumanDestinationType.FishingCoast:
                return CanFishCoast(building, bdb);
            case HumanDestinationType.Building:
                break;
            default:
                throw new NotSupportedException();
        }
    }

    bool CanHarvestTile(Building building, BuildingDatabase bdb) {
        var bottomLeft = building.workingAreaBottomLeftPos;
        var size = building.scriptable.WorkingAreaSize;

        for (var y = 0; y < size.y; y++) {
            for (var x = 0; x < size.x; x++) {
                if (!bdb.MapSize.Contains(x, y)) {
                    continue;
                }
            }
        }

        return true;
    }

    bool CanPlantTree(Building building, BuildingDatabase bdb) {
    }

    bool CanFishCoast(Building building, BuildingDatabase bdb) {
    }

    public override void OnEnter(Human human, HumanDatabase db) {
    }
}

public sealed class MoveToTheDestinationEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(Human human, HumanDatabase db) {
        Assert.AreEqual(human.destination, null);
        base.OnExit(human, db);
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
