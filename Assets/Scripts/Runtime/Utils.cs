using System.Collections.Generic;
using Random = System.Random;

namespace BFG.Runtime {
public static class Utils {
    public static void Shuffle<T>(IList<T> arr, Random random) {
        for (var i = 0; i < arr.Count; ++i) {
            var randomIndex = random.Next(arr.Count);
            (arr[randomIndex], arr[i]) = (arr[i], arr[randomIndex]);
        }
    }
}
}
