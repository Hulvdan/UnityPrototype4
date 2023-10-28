using UnityEngine;

namespace BFG.Runtime {
public enum Direction {
    Right = 0,
    Up = 1,
    Left = 2,
    Down = 3,
}

public static class DirectionOffsets {
    public static readonly Vector2Int[] Offsets = {
        new(1, 0),
        new(0, 1),
        new(-1, 0),
        new(0, -1),
    };
}
}
