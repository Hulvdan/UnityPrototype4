using System;
using System.Collections.Generic;
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
        return Utils.StupidVector2IntComparation(Pos, other.Pos);
    }

    public override string ToString() {
        return Pos.ToString();
    }

    public bool Equals(GraphVertex other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Pos.Equals(other.Pos);
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
