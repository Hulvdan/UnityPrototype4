// Generated via Assets/Scripts/Runtime/Extensions/codegen_vectors.py
//
// From YouTube by git-amend. Easy and Powerful Extension Methods | Unity C#.
// https://youtu.be/Nk49EUf7yyU

using UnityEngine;

namespace BFG.Runtime.Extensions {
public static class Vector2IntExtensions {
    public static Vector2Int With(
        this Vector2Int vector,
        int? x = null,
        int? y = null
    ) {
        return new(
            x ?? vector.x,
            y ?? vector.y
        );
    }

    public static Vector2Int Add(
        this Vector2Int vector,
        int? x = null,
        int? y = null
    ) {
        return new(
            vector.x + (x ?? 0),
            vector.y + (y ?? 0)
        );
    }
}
}
