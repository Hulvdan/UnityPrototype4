// Generated via Assets/Scripts/Runtime/Extensions/codegen_vectors.py
//
// From YouTube by git-amend. Easy and Powerful Extension Methods | Unity C#.
// https://youtu.be/Nk49EUf7yyU
using UnityEngine;

namespace BFG.Runtime.Extensions {
public static class Vector2Extensions {
    public static Vector3 With(
        this Vector3 vector,
        float? x = null
        float? y = null
        
    ) {
        return new(
            x ?? vector.x,
            y ?? vector.y,
            
        );
    }

    public static Vector3 Add(
        this Vector3 vector,
        float? x = null,
        float? y = null,
        
    ) {
        return new(
            vector.x + (x ?? 0),
            vector.y + (y ?? 0),
            
        );
    }
}
}