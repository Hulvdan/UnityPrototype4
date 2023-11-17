using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace BFG.Runtime {
public static class Utils {
    public static void Shuffle<T>(IList<T> arr, Random random) {
        for (var i = 0; i < arr.Count; i++) {
            var randomIndex = random.Next(arr.Count);
            (arr[randomIndex], arr[i]) = (arr[i], arr[randomIndex]);
        }
    }

    public static int StupidVector2IntComparation(Vector2Int a, Vector2Int b) {
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
}
}
