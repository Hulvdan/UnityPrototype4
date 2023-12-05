// From YouTube by git-amend. Easy and Powerful Extension Methods | Unity C#.
// https://youtu.be/Nk49EUf7yyU

using UnityEngine;

namespace BFG.Runtime.Extensions {
public static class Vector2IntExtensions {
    /// <summary>
    ///     Sets any x y values of a Vector2Int.
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2)
    ///     var vector = Vector2Int.one.With(y: 2);
    ///     </code>
    /// </example>
    public static Vector2Int With(this Vector2Int vector, int? x = null, int? y = null) {
        return new(
            x ?? vector.x,
            y ?? vector.y
        );
    }

    /// <summary>
    ///     Adds to any x y values of a Vector2Int
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2)
    ///     var vector = Vector2Int.one.Add(y: 1);
    ///     </code>
    /// </example>
    public static Vector2Int Add(this Vector2Int vector, int? x = null, int? y = null) {
        return new(
            vector.x + (x ?? 0),
            vector.y + (y ?? 0)
        );
    }
}
}
