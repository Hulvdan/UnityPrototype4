using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class HorseTrain {
    public readonly float Speed;

    public HorseTrain(float speed) {
        Assert.IsTrue(speed >= 0);
        Speed = speed;
    }

    public List<TrainNode> nodes { get; } = new();
    public List<Vector2Int> segmentVertexes { get; } = new();

    public int SegmentsCount => segmentVertexes.Count - 1;

    public void AddLocomotive(TrainNode node, int segmentIndex, float segmentProgress) {
        nodes.Add(node);
        node.Progress = segmentProgress;
        node.SegmentIndex = segmentIndex;
    }

    public void AddNode(TrainNode node) {
        HorseMovementSystem.NormalizeNodeDistances(node, nodes[^1]);
        nodes.Add(node);
    }

    public void AddSegmentVertex(Vector2Int vertex) {
        segmentVertexes.Add(vertex);
    }

    void PopBackSegmentVertex() {
        segmentVertexes.RemoveAt(0);
        foreach (var node in nodes) {
            node.SegmentIndex -= 1;
        }
    }
}
}
