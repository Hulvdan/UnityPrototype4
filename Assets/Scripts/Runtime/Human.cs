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
    [Min(0)]
    float _harvestingDurationSeconds = 1f;

    [SerializeField]
    Vector2 _position;

    HumanState _state;

    Vector2 _target;

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
