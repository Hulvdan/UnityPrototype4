#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime.Entities {
// Hulvdan: Mb it should not be a class?
public class HumanMovingComponent {
    public Vector2Int pos { get; set; }
    public float elapsed { get; set; }
    public float progress { get; set; }
    public Vector2 from { get; set; }
    public Vector2Int? to { get; set; }

    public readonly List<Vector2Int> path = new();

    public HumanMovingComponent(Vector2Int initialPosition) {
        pos = initialPosition;
        from = initialPosition;
    }

    public void AddPath(List<Vector2Int> path_) {
        path.Clear();

        var isFirst = true;
        foreach (var tile in path_) {
            if (isFirst) {
                isFirst = false;

                if (tile != (to ?? pos)) {
                    path.Add(tile);
                }

                continue;
            }

            path.Add(tile);
        }

        if (to == null) {
            PopMovingTo();
        }
    }

    public void PopMovingTo() {
        if (path.Count == 0) {
            elapsed = 0;
            to = null;
        }
        else {
            to = path[0];
            path.RemoveAt(0);
        }
    }
}
}
