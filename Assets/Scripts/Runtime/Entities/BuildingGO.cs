using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime.Entities {
[Serializable]
public class BuildingGo {
    [SerializeField]
    [Required]
    ScriptableBuilding _scriptableBuilding;

    [SerializeField]
    int _posX;

    [SerializeField]
    int _posY;

    [SerializeField]
    [Range(0, 1)]
    float _buildingProgress;

    public Building IntoBuilding() {
        if (Mathf.Approximately(_buildingProgress, 1)) {
            _buildingProgress = 1;
        }

        return new(
            Guid.NewGuid(),
            _scriptableBuilding,
            new(_posX, _posY),
            _buildingProgress
        );
    }
}
}
