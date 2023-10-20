using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
[Serializable]
public class Human {
    [SerializeField]
    [ReadOnly]
    Building _harvestBuilding;

    [SerializeField]
    [ReadOnly]
    [CanBeNull]
    Building _storeBuilding;

    [SerializeField]
    Vector2 _position;

    [SerializeField]
    [ReadOnly]
    float _harvestingElapsed;

    [ShowInInspector]
    [ReadOnly]
    public readonly Guid ID;

    Vector2Int? _harvestTilePosition;

    public Human(Guid id, Building harvestBuilding, Vector2 position) {
        ID = id;
        _harvestBuilding = harvestBuilding;
        _position = position;
    }

    public float harvestingElapsed {
        get => _harvestingElapsed;
        set => _harvestingElapsed = value;
    }

    public Vector2 position {
        get => _position;
        set => _position = value;
    }

    [ShowInInspector]
    [ReadOnly]
    public HumanState state { get; set; } = HumanState.Idle;

    public Vector2Int? harvestTilePosition {
        get => _harvestTilePosition;
        set => _harvestTilePosition = value;
    }

    [ShowInInspector]
    [ReadOnly]
    public Vector2Int _movingFrom;

    public Building harvestBuilding => _harvestBuilding;

    [CanBeNull]
    public Building storeBuilding {
        get => _storeBuilding;
        set => _storeBuilding = value;
    }

    public Vector2Int movingFrom {
        get => _movingFrom;
        set => _movingFrom = value;
    }
}
}
