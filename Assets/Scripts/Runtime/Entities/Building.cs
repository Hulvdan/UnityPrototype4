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
    public bool employeeIsInside;

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

    public void Update(BuildingDatabase db, float dt) {
        if (scriptable.type is BuildingType.Harvest or BuildingType.Produce) {
            if (CurrentBehaviourIndex >= 0) {
                Behaviours[CurrentBehaviourIndex].UpdateDt(this, db, dt);
            }
        }
    }

    #region BuildingData

    public bool CanStartProcessingCycle(BuildingDatabase bdb) {
        if (CurrentBehaviourIndex == -1) {
            return false;
        }

        foreach (var beh in Behaviours) {
            if (!beh.CanBeRun(this, bdb)) {
                return false;
            }
        }

        return true;
    }

    public void StartProcessingCycle(BuildingDatabase bdb) {
        Assert.AreEqual(CurrentBehaviourIndex, -1);

        foreach (var beh in Behaviours) {
            beh.BookRequiredTiles(this, bdb);
        }

        CurrentBehaviourIndex = 0;
    }

    public Vector2Int workingAreaBottomLeftPos;

    public float takingResourceElapsed;
    public float processingElapsed;
    public float placingResourceElapsed;
    public Human? createdHuman;

    // CurrentBehaviourIndex = -1 = building is idle right now
    public int CurrentBehaviourIndex = -1;
    public List<BuildingBehaviour> Behaviours = new();
    public List<(int, Vector2Int)> BookedTiles = new();

    #endregion
}
}
