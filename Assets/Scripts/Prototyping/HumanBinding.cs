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
    public GameObject human;

    public MovementPattern pattern;

    [HideInInspector]
    public List<Vector2Int> path = new();

    [NonSerialized]
    public List<AnimationCurve> curvePerFeedback = new();

    [NonSerialized]
    public List<Vector2Int> movementPathSplitIntoTiles = new();

    [NonSerialized]
    public int currentIndex;

    public void Init() {
        foreach (var feedback in pattern.feedbacks) {
            curvePerFeedback.Add(feedback.GetRandomCurve());
        }

        for (var i = 0; i < path.Count - 1; i++) {
            var a = path[i];
            var b = path[i + 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                movementPathSplitIntoTiles.Add(tile);
            }
        }

        for (var i = path.Count - 1; i > 0; i--) {
            var a = path[i];
            var b = path[i - 1];

            foreach (var tile in GetFromToTiles(a, b)) {
                movementPathSplitIntoTiles.Add(tile);
            }
        }
    }

    public Vector2Int UpdateNextTileInPath() {
        currentIndex++;
        while (currentIndex >= movementPathSplitIntoTiles.Count) {
            currentIndex -= movementPathSplitIntoTiles.Count;
        }

        return Vector2Int.zero;
    }

    public Vector2Int GetMovingFrom() {
        return movementPathSplitIntoTiles[currentIndex];
    }

    public Vector2Int GetMovingTo() {
        return movementPathSplitIntoTiles[(currentIndex + 1) % movementPathSplitIntoTiles.Count];
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
