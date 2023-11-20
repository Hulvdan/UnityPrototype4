using System;
using System.Collections.Generic;
using BFG.Core;
using UnityEngine;

namespace BFG.Runtime {
public class GraphVertex : IComparable<GraphVertex>, IEquatable<GraphVertex> {
    public List<ResourceObj> Resources;
    public Vector2Int Pos;

    public GraphVertex(List<ResourceObj> resources, Vector2Int pos) {
        Resources = resources;
        Pos = pos;
    }

    public int CompareTo(GraphVertex other) {
        return Utils.StupidVector2IntComparison(Pos, other.Pos);
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

        if (!Utils.GoodFukenListEquals(Resources, other.Resources)) {
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
        return HashCode.Combine(Resources, Pos);
    }
}
}
