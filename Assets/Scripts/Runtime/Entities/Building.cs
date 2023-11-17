using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class Building {
    public bool IsProducing;
    public float ProducingElapsed;

    public bool isBuilt => BuildingProgress >= 1;
    public float BuildingProgress;

    Guid _id = Guid.Empty;

    public IScriptableBuilding scriptableBuilding { get; }

    public int posX { get; }
    public int posY { get; }

    public Vector2Int pos => new(posX, posY);

    readonly List<ResourceObj> _producedResources = new();
    readonly List<ResourceObj> _storedResources = new();

    public Building(
        Guid id, IScriptableBuilding scriptable, Vector2Int pos, float buildingProgress
    ) {
        _id = id;
        scriptableBuilding = scriptable;
        posX = pos.x;
        posY = pos.y;

        BuildingProgress = buildingProgress;
    }

    public Guid id {
        get {
            if (_id == Guid.Empty) {
                _id = Guid.NewGuid();
            }

            return _id;
        }
    }

    public bool isBooked { get; set; }

    public List<ResourceObj> storedResources {
        get {
            if (
                scriptableBuilding.type != BuildingType.Store
                && scriptableBuilding.type != BuildingType.Produce
            ) {
                Debug.LogError("WTF");
            }

            return _storedResources;
        }
    }

    public List<ResourceObj> producedResources {
        get {
            if (scriptableBuilding.type != BuildingType.Produce) {
                Debug.LogError("WTF?");
            }

            return _producedResources;
        }
    }

    public RectInt rect => new(posX, posY, scriptableBuilding.size.x, scriptableBuilding.size.y);

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return pos.x <= x
               && x < pos.x + scriptableBuilding.size.x
               && pos.y <= y
               && y < pos.y + scriptableBuilding.size.y;
    }

    public bool CanStoreResource() {
        return storedResources.Count < scriptableBuilding.storeItemsAmount;
    }

    public StoreResourceResult StoreResource(ResourceObj resource) {
        if (
            scriptableBuilding.type != BuildingType.Produce
            && scriptableBuilding.type != BuildingType.Store
        ) {
            Debug.LogError("WTF?");
        }

        storedResources.Add(resource);

        if (CanStartProcessing()) {
            IsProducing = true;
            ProducingElapsed = 0;
            storedResources.RemoveAt(0);
            return StoreResourceResult.AddedToProcessingImmediately;
        }

        return StoreResourceResult.AddedToTheStore;
    }

    public bool CanStartProcessing() {
        if (producedResources.Count >= scriptableBuilding.produceItemsAmount) {
            return false;
        }

        return !IsProducing;
    }
}

public enum StoreResourceResult {
    AddedToTheStore,
    AddedToProcessingImmediately,
}
}
