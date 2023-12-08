#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class BuildingController {
    public void Init(BuildingDatabase db) {
        _db = db;
    }

    public void SwitchToTheNextBehaviour(Building building) {
        if (building.currentBehaviourIndex >= 0) {
            var oldBeh = building.Behaviours[building.currentBehaviourIndex];
            oldBeh.OnExit(building, _db);
        }

        building.currentBehaviourIndex++;
        if (building.currentBehaviourIndex >= building.Behaviours.Count) {
            building.currentBehaviourIndex = 0;
        }

        if (building.)
    }

    BuildingDatabase _db;
}

public class Building {
    public bool isConstructed => constructionElapsed >= scriptable.ConstructionDuration;
    public float constructionProgress => constructionElapsed / scriptable.ConstructionDuration;
    public float constructionElapsed { get; set; } = 0f;

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
            Assert.IsTrue(currentBehaviourIndex >= 0);
            var beh = Behaviours[currentBehaviourIndex];
            beh.Update(this, db, dt);
        }
    }

    #region BuildingData

    public float idleElapsed;
    public float takingResourceElapsed;
    public float processingElapsed;
    public float placingResourceElapsed;
    public Human? createdHuman;

    public int currentBehaviourIndex = -1;
    public List<BuildingBehaviour> Behaviours = new();

    #endregion
}
}
