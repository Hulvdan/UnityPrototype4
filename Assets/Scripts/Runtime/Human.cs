using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public enum HumanState {
    Idle,
    HeadingToTheTarget,
    Harvesting,
    HeadingBackToTheBuilding
}

[Serializable]
public class Human {
    [SerializeField]
    [ReadOnly]
    Building _building;

    [SerializeField]
    Vector2 _position;

    [SerializeField]
    [ReadOnly]
    float _harvestingElapsed;

    [ShowInInspector]
    [ReadOnly]
    public readonly Guid ID;

    Vector2Int? _positionTarget;

    public Human(Guid id, Building building, Vector2 position) {
        ID = id;
        _building = building;
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

    public Vector2Int? positionTarget {
        get => _positionTarget;
        set => _positionTarget = value;
    }

    public Building building => _building;
}
}
