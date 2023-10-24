using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BFG.Runtime {
public class MapNode {
    public Vector2Int Position => Vector2Int.zero;
}

public class TrainNode {
    public Vector2 Position {
        get => Vector2.zero;
        set { }
    }
}

public enum ChainNodeType {
    Horse,
    Cart
}

public class MovementChainNode {
    public ChainNodeType Type;
    public MovementChainNode Previous;
    public float Width = 0.8f; // Max is 1!
}

public class HorseMovementSystem {
    List<TrainNode> _trainNodes;
    float _speed = 1f;
    float _duration = 1f;

    // MovementNode

    void Update() {
        foreach (var node in _trainNodes) {
            // node.Position +=
        }
    }
}
}
