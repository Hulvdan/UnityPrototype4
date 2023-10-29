using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class TrainNode {
    public readonly Guid ID;

    public Vector2 CalculatedPosition;

    public float CalculatedRotation;

    public float Progress;

    public int SegmentIndex;

    public float Width;

    public TrainNode(Guid id, float width, int canStoreResourceCount = 1) {
        ID = id;
        Width = width;
        this.canStoreResourceCount = canStoreResourceCount;
    }

    public int canStoreResourceCount { get; }

    public List<Tuple<ScriptableResource, int>> storedResources { get; } = new();
}
}
