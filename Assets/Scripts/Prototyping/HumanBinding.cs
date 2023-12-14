using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Runtime.Rendering;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Prototyping {
[Serializable]
public class HumanBinding {
    [ReadOnly]
    public GameObject Human;

    public MovementPattern Pattern;

    [HideInInspector]
    public List<Vector2Int> Path = new();

    [NonSerialized]
    public List<AnimationCurve> CurvePerFeedback = new();

    [NonSerialized]
    public List<Vector2Int> MovementPathSplitIntoTiles = new();

    [NonSerialized]
    public int CurrentIndex;

    public void Init() {
        foreach (var feedback in Pattern.Feedbacks) {
            CurvePerFeedback.Add(feedback.GetRandomCurve());
        }

        var path = Path;
        for (var i = 0; i < path.Count - 1; i++) {
            var a = path[i];
            var b = path[i + 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                MovementPathSplitIntoTiles.Add(tile);
            }
        }

        for (var i = path.Count - 1; i > 0; i--) {
            var a = path[i];
            var b = path[i - 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                MovementPathSplitIntoTiles.Add(tile);
            }
        }
    }

    public Vector2Int UpdateNextTileInPath() {
        CurrentIndex++;
        while (CurrentIndex >= MovementPathSplitIntoTiles.Count) {
            CurrentIndex -= MovementPathSplitIntoTiles.Count;
        }

        return Vector2Int.zero;
    }

    public Vector2Int GetMovingFrom() {
        return MovementPathSplitIntoTiles[CurrentIndex];
    }

    public Vector2Int GetMovingTo() {
        return MovementPathSplitIntoTiles[(CurrentIndex + 1) % MovementPathSplitIntoTiles.Count];
    }

    List<Vector2Int> GetFromToTiles(Vector2Int from, Vector2Int to) {
        Assert.IsTrue(from.x == to.x || from.y == to.y);
        Assert.IsFalse(from.x == to.x && from.y == to.y);

        var offset = Utils.Direction(from, to).AsOffset();
        var length = Math.Max(
            Math.Abs(from.x - to.x),
            Math.Abs(from.y - to.y)
        );

        var current = from;
        var res = new List<Vector2Int> { current };

        for (var i = 0; i < length - 1; i++) {
            current += offset;
            res.Add(current);
        }

        return res;
    }
}
}
