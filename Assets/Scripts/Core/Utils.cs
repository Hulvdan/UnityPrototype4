using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace BFG.Core {
public static class Utils {
    public static void Shuffle<T>(IList<T> arr, Random random) {
        for (var i = 0; i < arr.Count; i++) {
            var randomIndex = random.Next(arr.Count);
            (arr[randomIndex], arr[i]) = (arr[i], arr[randomIndex]);
        }
    }

    public static bool GoodFukenListEquals<T>([CanBeNull] List<T> a, [CanBeNull] List<T> b) {
        if (a == null && b == null) {
            return true;
        }

        if (a == null || b == null) {
            return false;
        }

        if (a.Count != b.Count) {
            return false;
        }

        for (var i = 0; i < a.Count; i++) {
            var aItem = a[i];
            var bItem = b[i];
            if (!aItem.Equals(bItem)) {
                return false;
            }
        }

        return true;
    }

    public static bool GoodFuken2DListEquals<T>(
        [CanBeNull]
        List<List<T>> a,
        [CanBeNull]
        List<List<T>> b
    ) {
        if (a == null && b == null) {
            return true;
        }

        if (a == null || b == null) {
            return false;
        }

        if (a.Count != b.Count) {
            return false;
        }

        for (var i = 0; i < a.Count; i++) {
            if (!GoodFukenListEquals(a[i], b[i])) {
                return false;
            }
        }

        return true;
    }

    public static readonly Direction[] DIRECTIONS = {
        Core.Direction.Right,
        Core.Direction.Up,
        Core.Direction.Left,
        Core.Direction.Down,
    };

    public static Direction Direction(Vector2Int a, Vector2Int b) {
        var c = b - a;
        switch (c.x) {
            case > 0:
                return Core.Direction.Right;
            case < 0:
                return Core.Direction.Left;
        }

        switch (c.y) {
            case > 0:
                return Core.Direction.Up;
            case < 0:
                return Core.Direction.Down;
        }

        Assert.IsTrue(false);
        return Core.Direction.Right;
    }

    public static (int min, int max) MinMax(int a, int b, int c) {
        return (Math.Min(a, b), Math.Max(b, c));
    }
}
}
