using System;
using Sirenix.OdinInspector;
using UnityEngine;

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
    bool _isBooked;

    public ScriptableBuilding scriptableBuilding => _scriptableBuilding;
    public int posX => _posX;
    public int posY => _posY;
    public Vector2Int position => new(_posX, _posY);

    public bool isBooked {
        get => _isBooked;
        set => _isBooked = value;
    }
}
}
