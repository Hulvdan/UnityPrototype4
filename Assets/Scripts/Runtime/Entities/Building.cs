#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class Building {
    public bool isConstructed => constructionElapsed >= scriptable.ConstructionDuration;
    public float constructionProgress => constructionElapsed / scriptable.ConstructionDuration;
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

    public readonly List<ResourceToBook> ResourcesToBook = new();

    public Building(
        Guid id,
        IScriptableBuilding scriptable,
        Vector2Int pos,
        float constructionElapsed
    ) {
        _id = id;
        this.scriptable = scriptable;
        posX = pos.x;
        posY = pos.y;

        this.constructionElapsed = constructionElapsed;
        WorkingAreaBottomLeftPos = -Vector2Int.one;

        if (scriptable.type is BuildingType.Fish or BuildingType.Harvest or BuildingType.Plant) {
            WorkingAreaBottomLeftPos = pos - scriptable.WorkingAreaSize / 2;
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
    public Human? SpawnedHuman;
    public bool EmployeeIsInside;

    public readonly List<MapResource> PlacedResourcesForConstruction = new();

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return pos.x <= x
               && x < pos.x + scriptable.size.x
               && pos.y <= y
               && y < pos.y + scriptable.size.y;
    }

    #region BuildingData

    public bool CanStartProcessingCycle(BuildingDatabase bdb) {
        if (CurrentBehaviourIndex != -1) {
            return false;
        }

        if (!EmployeeIsInside) {
            return false;
        }

        foreach (var beh in scriptable.behaviours) {
            if (!beh.CanBeRun(this, bdb)) {
                return false;
            }
        }

        return true;
    }

    public void StartProcessingCycle(BuildingDatabase bdb) {
        Assert.AreEqual(CurrentBehaviourIndex, -1);

        foreach (var beh in scriptable.behaviours) {
            beh.BookRequiredTiles(this, bdb);
        }

        CurrentBehaviourIndex = 0;
    }

    public Vector2Int WorkingAreaBottomLeftPos;

    public float takingResourceElapsed;
    public float processingElapsed;
    public float placingResourceElapsed;
    public Human? createdHuman;

    // CurrentBehaviourIndex = -1 = building is idle right now
    public int CurrentBehaviourIndex = -1;
    public List<(int, Vector2Int)> BookedTiles = new();

    #endregion
}
}
