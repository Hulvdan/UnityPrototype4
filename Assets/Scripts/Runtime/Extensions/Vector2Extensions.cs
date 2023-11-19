using UnityEngine;

namespace BFG.Runtime.Extensions {
public static class Vector2Extensions {
    /// <summary>
    ///     Sets any x y values of a Vector2.
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2)
    ///     var vector = Vector2.one.With(y: 2);
    ///     </code>
    /// </example>
    public static Vector2 With(this Vector2 vector, float? x = null, float? y = null) {
        return new(
            x ?? vector.x,
            y ?? vector.y
        );
    }

    /// <summary>
    ///     Adds to any x y values of a Vector2
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2)
    ///     var vector = Vector2.one.Add(y: 1);
    ///     </code>
    /// </example>
    public static Vector2 Add(this Vector2 vector, float? x = null, float? y = null) {
        return new(
            vector.x + (x ?? 0),
            vector.y + (y ?? 0)
        );
    }
}
}
