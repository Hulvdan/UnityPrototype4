using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class Building {
    [SerializeField]
    [Required]
    ScriptableBuilding _scriptableBuilding;

    [SerializeField]
    int _posX;

    [SerializeField]
    int _posY;

    [SerializeField]
    [ReadOnly]
    bool _isBooked;

    [FormerlySerializedAs("ProcessingElapsed")]
    [ShowIf("@_scriptableBuilding._type == BuildingType.Produce")]
    public float ProducingElapsed;

    [FormerlySerializedAs("IsProcessing")]
    [ShowIf("@_scriptableBuilding._type == BuildingType.Produce")]
    public bool IsProducing;

    public bool isBuilt => BuildingProgress >= 1;

    [SerializeField]
    [Range(0, 1)]
    public float BuildingProgress;

    [FormerlySerializedAs("_ID")]
    [SerializeField]
    [Required]
    Guid _id = Guid.Empty;

    [SerializeField]
    List<ResourceObj> _producedResources = new();

    [SerializeField]
    List<ResourceObj> _storedResources = new();

    public ScriptableBuilding scriptableBuilding => _scriptableBuilding;
    public int posX => _posX;
    public int posY => _posY;
    public Vector2Int pos => new(_posX, _posY);

    public Guid ID {
        get {
            if (_id == Guid.Empty) {
                _id = Guid.NewGuid();
            }

            return _id;
        }
    }

    public bool isBooked {
        get => _isBooked;
        set => _isBooked = value;
    }

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

    public RectInt rect => new(posX, posY, _scriptableBuilding.size.x, _scriptableBuilding.size.y);

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
