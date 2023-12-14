#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime.Entities {
public class Building {
    public bool isConstructed => constructionElapsed >= scriptable.constructionDuration;
    public float constructionProgress => constructionElapsed / scriptable.constructionDuration;
    public float constructionElapsed { get; set; }

    public Human? constructor { get; set; }
    public Human? employee { get; set; }

    public IScriptableBuilding scriptable { get; }

    public int posX { get; }
    public int posY { get; }

    public Vector2Int pos => new(posX, posY);
    public float timeSinceHumanWasCreated { get; set; } = float.PositiveInfinity;
    public float timeSinceItemWasPlaced { get; set; } = float.PositiveInfinity;

    Guid _id;

    public readonly List<ResourceToBook> resourcesToBook = new();

    public Building(
        Guid id,
        IScriptableBuilding scriptable_,
        Vector2Int pos,
        float constructionElapsed_
    ) {
        _id = id;
        scriptable = scriptable_;
        posX = pos.x;
        posY = pos.y;

        constructionElapsed = constructionElapsed_;
        workingAreaBottomLeftPos = -Vector2Int.one;

        if (scriptable_.type is BuildingType.Fish or BuildingType.Harvest or BuildingType.Plant) {
            workingAreaBottomLeftPos = pos - scriptable_.workingAreaSize / 2;
        }
    }

    public Guid id {
        get {
            if (_id == Guid.Empty) {
                _id = Guid.NewGuid();
            }

            return _id;
        }
    }

    public RectInt rect => new(posX, posY, scriptable.size.x, scriptable.size.y);
    public Human? spawnedHuman;
    public bool employeeIsInside;

    public readonly List<MapResource> placedResourcesForConstruction = new();

    public bool Contains(Vector2Int pos_) {
        return Contains(pos_.x, pos_.y);
    }

    public bool Contains(int x, int y) {
        return pos.x <= x
               && x < pos.x + scriptable.size.x
               && pos.y <= y
               && y < pos.y + scriptable.size.y;
    }

    #region BuildingData

    public bool CanStartProcessingCycle(BuildingDatabase bdb) {
        if (currentBehaviourIndex != -1) {
            return false;
        }

        if (!employeeIsInside) {
            return false;
        }

        foreach (var beh in scriptable.behaviours) {
            if (!beh.CanBeRun(this, bdb)) {
                return false;
            }
        }

        return true;
    }

    public Vector2Int workingAreaBottomLeftPos;

    public float takingResourceElapsed;
    public float processingElapsed;
    public float placingResourceElapsed;
    public Human? createdHuman;

    // CurrentBehaviourIndex = -1 = building is idle right now
    public int currentBehaviourIndex = -1;
    public readonly List<(int, Vector2Int)> bookedTiles = new();

    #endregion
}
}
