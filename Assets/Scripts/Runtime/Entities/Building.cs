using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class Building {
    [FormerlySerializedAs("_ID")]
    [SerializeField]
    [Required]
    Guid _id = Guid.Empty;

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

    [SerializeField]
    List<Tuple<ScriptableResource, int>> _storedResources = new();

    [SerializeField]
    List<Tuple<ScriptableResource, int>> _producedResources = new();

    public ScriptableBuilding scriptableBuilding => _scriptableBuilding;
    public int posX => _posX;
    public int posY => _posY;
    public Vector2Int position => new(_posX, _posY);

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

    public List<Tuple<ScriptableResource, int>> storedResources {
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

    public List<Tuple<ScriptableResource, int>> producedResources {
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
        return position.x <= x
               && x <= position.x + scriptableBuilding.size.x
               && position.y <= y
               && y <= position.y + scriptableBuilding.size.y;
    }

    public bool CanStoreResource() {
        return storedResources.Count < scriptableBuilding.storeItemsAmount;
    }

    public StoreResourceResult StoreResource(ScriptableResource foundResource, int count) {
        if (
            scriptableBuilding.type != BuildingType.Produce
            && scriptableBuilding.type != BuildingType.Store
        ) {
            Debug.LogError("WTF?");
        }

        storedResources.Add(new(foundResource, count));

        if (CanBeProcessed()) {
            IsProcessing = true;
            ProcessingElapsed = 0;
            return StoreResourceResult.AddedToProcessingImmediately;
        }

        return StoreResourceResult.AddedToTheStore;
    }

    public float ProcessingElapsed;
    public bool IsProcessing;

    bool CanBeProcessed() {
        if (producedResources.Count >= scriptableBuilding.produceItemsAmount) {
            return false;
        }

        return !IsProcessing;
    }
}

public enum StoreResourceResult {
    AddedToTheStore,
    AddedToProcessingImmediately,
}
}
