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

    public static int StupidVector2IntComparison(Vector2Int a, Vector2Int b) {
        if (a.x > b.x) {
            return 1;
        }

        if (a.x < b.x) {
            return -1;
        }

        if (a.y > b.y) {
            return 1;
        }

        if (a.y < b.y) {
            return -1;
        }

        return 0;
    }

    public static bool GoodFukenListEquals<T>([CanBeNull] List<T> obj1, [CanBeNull] List<T> obj2) {
        var null1 = ReferenceEquals(null, obj1);
        var null2 = ReferenceEquals(null, obj2);
        if (null1 && null2) {
            return true;
        }

        if ((null1 && !null2) || (!null1 && null2)) {
            return false;
        }

        for (var i = 0; i < obj1.Count; i++) {
            var a = obj1[i];
            var b = obj2[i];
            if (!a.Equals(b)) {
                return false;
            }
        }

        return true;
    }

    public static bool GoodFuken2DListEquals<T>(
        [CanBeNull]
        List<List<T>> obj1,
        [CanBeNull]
        List<List<T>> obj2
    ) {
        var null1 = ReferenceEquals(null, obj1);
        var null2 = ReferenceEquals(null, obj2);
        if (null1 && null2) {
            return true;
        }

        if ((null1 && !null2) || (!null1 && null2)) {
            return false;
        }

        for (var i = 0; i < obj1.Count; i++) {
            if (!GoodFukenListEquals(obj1[i], obj2[i])) {
                return false;
            }
        }

        return true;
    }

    public static readonly Direction[] Directions = {
        Core.Direction.Right, Core.Direction.Up, Core.Direction.Left, Core.Direction.Down,
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
