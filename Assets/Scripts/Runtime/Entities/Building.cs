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
    bool _isBooked;

    [SerializeField]
    List<Tuple<ScriptableResource, int>> _storedResources = new();

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

    public List<Tuple<ScriptableResource, int>> storedResources => _storedResources;
}
}
