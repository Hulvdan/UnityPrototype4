using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
[Serializable]
public class TrainNode {
    public Vector2 CalculatedPosition;

    public float CalculatedRotation;

    public float Progress;

    public int SegmentIndex;

    public float Width;
    public readonly Guid ID;

    public TrainNode(Guid id, float width, int canStoreResourceCount = 1) {
        ID = id;
        Width = width;
        this.canStoreResourceCount = canStoreResourceCount;
    }

    public int canStoreResourceCount { get; }

    public List<ResourceObj> storedResources { get; } = new();
}
}
