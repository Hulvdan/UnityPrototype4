using System;
using System.Collections.Generic;
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

    [SerializeField]
    List<Tuple<ScriptableResource, int>> _storedResources = new();

    public ScriptableBuilding scriptableBuilding => _scriptableBuilding;
    public int posX => _posX;
    public int posY => _posY;
    public Vector2Int position => new(_posX, _posY);

    public bool isBooked {
        get => _isBooked;
        set => _isBooked = value;
    }

    public List<Tuple<ScriptableResource, int>> storedResources => _storedResources;
}
}
