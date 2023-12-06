#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime.Entities {
public class HumanMovingComponent {
    public Vector2Int pos { get; set; }
    public float elapsed { get; set; }
    public float progress { get; set; }
    public Vector2 from { get; set; }
    public Vector2Int? to { get; set; }

    public readonly List<Vector2Int> Path = new();

    public HumanMovingComponent(Vector2Int initialPosition) {
        pos = initialPosition;
        from = initialPosition;
    }

    public void AddPath(List<Vector2Int> path) {
        Path.Clear();

        var isFirst = true;
        foreach (var tile in path) {
            if (isFirst) {
                isFirst = false;

                if (tile != (to ?? pos)) {
                    Path.Add(tile);
                }

                continue;
            }

            Path.Add(tile);
        }

        if (to == null) {
            PopMovingTo();
        }
    }

    public void PopMovingTo() {
        if (Path.Count == 0) {
            elapsed = 0;
            to = null;
        }
        else {
            to = Path[0];
            Path.RemoveAt(0);
        }
    }
}
}
