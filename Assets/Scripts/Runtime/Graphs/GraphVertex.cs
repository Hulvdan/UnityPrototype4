using System;
using UnityEngine;

namespace BFG.Runtime.Graphs {
public sealed class GraphVertex : IEquatable<GraphVertex> {
    public Vector2Int pos;

    public GraphVertex(Vector2Int pos_) {
        pos = pos_;
    }

    public override string ToString() {
        return pos.ToString();
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

        if (!pos.Equals(other.pos)) {
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
        return HashCode.Combine(pos);
    }
}
}
