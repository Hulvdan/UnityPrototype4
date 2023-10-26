using UnityEngine;

namespace BFG.Runtime {
public class TrainNode {
    public Vector2 CalculatedPosition;
    public float CalculatedRotation;
    public float Progress;
    public int SegmentIndex;

    public float Width;

    public TrainNode(float width) {
        Width = width;
    }
}
}
