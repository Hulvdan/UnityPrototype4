// From YouTube by git-amend. Easy and Powerful Extension Methods | Unity C#.
// https://youtu.be/Nk49EUf7yyU

using UnityEngine;

namespace BFG.Runtime.Extensions {
public static class Vector3Extensions {
    /// <summary>
    ///     Sets any x y z values of a Vector3.
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2, 1)
    ///     var vector = Vector3.one.With(y: 2);
    ///     </code>
    /// </example>
    public static Vector3 With(
        this Vector3 vector, float? x = null, float? y = null, float? z = null
    ) {
        return new(
            x ?? vector.x,
            y ?? vector.y,
            z ?? vector.z
        );
    }

    /// <summary>
    ///     Adds to any x y z values of a Vector3
    /// </summary>
    /// <example>
    ///     <code>
    ///     // (1, 2, 1)
    ///     var vector = Vector3.one.Add(y: 1);
    ///     </code>
    /// </example>
    public static Vector3 Add(
        this Vector3 vector, float? x = null, float? y = null, float? z = null
    ) {
        return new(
            vector.x + (x ?? 0),
            vector.y + (y ?? 0),
            vector.z + (z ?? 0)
        );
    }
}
}
