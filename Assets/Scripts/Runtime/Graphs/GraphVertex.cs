using System;
using UnityEngine;

namespace BFG.Runtime.Graphs {
public sealed class GraphVertex : IEquatable<GraphVertex> {
    public Vector2Int Pos;

    public GraphVertex(Vector2Int pos) {
        Pos = pos;
    }

    public override string ToString() {
        return Pos.ToString();
    }

    public static bool operator ==(GraphVertex obj1, GraphVertex obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return true;
        }

        if (ReferenceEquals(null, obj1)) {
            return false;
        }

        return obj1.Equals(obj2);
    }

    public static bool operator !=(GraphVertex obj1, GraphVertex obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return false;
        }

        if (ReferenceEquals(null, obj1)) {
            return true;
        }

        return !obj1.Equals(obj2);
    }

    public bool Equals(GraphVertex other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        if (!Pos.Equals(other.Pos)) {
            return false;
        }

        return true;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj.GetType() != GetType()) {
            return false;
        }

        return Equals((GraphVertex)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Pos);
    }
}
}
