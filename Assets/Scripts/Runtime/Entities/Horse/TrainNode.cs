using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime {
[Serializable]
public class TrainNode {
    [FormerlySerializedAs("CalculatedPosition")]
    public Vector2 Position;

    [FormerlySerializedAs("CalculatedRotation")]
    public float Rotation;

    public float Progress;

    public int SegmentIndex;

    public float Width;
    public readonly Guid ID;
    public bool isLocomotive;

    public TrainNode(
        Guid id,
        float width,
        bool isLocomotive = false,
        int canStoreResourceCount = 1
    ) {
        ID = id;
        Width = width;
        this.canStoreResourceCount = canStoreResourceCount;
        this.isLocomotive = isLocomotive;
    }

    public int canStoreResourceCount { get; }

    public List<ResourceObj> storedResources { get; } = new();
}
}
