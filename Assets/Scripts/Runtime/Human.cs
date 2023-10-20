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
    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

    [SerializeField]
    [ReadOnly]
    Building _building;

    [SerializeField]
    [OnValueChanged("UpdateValues")]
    [Min(0)]
    int _buildingIndex;

    [SerializeField]
    Vector2 _position;

    [ReadOnly]
    public readonly Guid ID;

    Vector2Int? _positionTarget;

    public Human(Guid id, Building building, Vector2 position) {
        ID = id;
        _building = building;
        _position = position;
    }

    public Vector2 position => _position;

    public HumanState state { get; set; } = HumanState.Idle;

    public Vector2Int? positionTarget {
        get => _positionTarget;
        set => _positionTarget = value;
    }

    public Building building => _building;

    void UpdateValues() {
        if (_buildingIndex >= _map.buildings.Count) {
            Debug.LogError("Wrong _buildingIndex!");
        }
        else {
            _building = _map.buildings[_buildingIndex];
        }
    }
}
}
