using System;
using UnityEngine;

namespace BFG.Runtime {
public enum Direction {
    Right = 0,
    Up = 1,
    Left = 2,
    Down = 3,
}

public static class DirectionExtensions {
    public static Vector2Int AsOffset(this Direction direction) {
        return direction switch {
            Direction.Right => Vector2Int.right,
            Direction.Up => Vector2Int.up,
            Direction.Left => Vector2Int.left,
            Direction.Down => Vector2Int.down,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
        };
    }

    public static Direction Opposite(this Direction direction) {
        return (Direction)(((int)direction + 2) % 4);
    }
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
