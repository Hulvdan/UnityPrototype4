using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
// TODO: IComparable, IEquatable should be considered for removal
public class GraphSegment : IComparable<GraphSegment>, IEquatable<GraphSegment> {
    public readonly List<GraphVertex> Vertexes;
    public readonly Graph Graph;
    public readonly List<Vector2Int> MovementTiles;

    public GraphSegment(List<GraphVertex> vertexes, List<Vector2Int> movementTiles, Graph graph) {
        Assert.IsNotNull(vertexes);
        Assert.IsNotNull(movementTiles);
        Assert.IsNotNull(graph);

        Vertexes = vertexes;
        MovementTiles = movementTiles;

        Graph = graph;
        vertexes.Sort();
        // TODO: Should be considered for removal
        movementTiles.Sort(Utils.StupidVector2IntComparation);

        // Duplication Checks
        for (var i = 0; i < vertexes.Count; i++) {
            for (var j = 0; j < vertexes.Count; j++) {
                if (i == j) {
                    continue;
                }

                Assert.AreNotEqual(vertexes[i], vertexes[j]);
            }
        }

        for (var i = 0; i < movementTiles.Count; i++) {
            for (var j = 0; j < movementTiles.Count; j++) {
                if (i == j) {
                    continue;
                }

                Assert.AreNotEqual(movementTiles[i], movementTiles[j]);
            }
        }
    }

    public static bool operator ==(GraphSegment obj1, GraphSegment obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return true;
        }

        if (ReferenceEquals(null, obj1)) {
            return false;
        }

        return obj1.Equals(obj2);
    }

    public static bool operator !=(GraphSegment obj1, GraphSegment obj2) {
        if (ReferenceEquals(null, obj1) && ReferenceEquals(null, obj2)) {
            return false;
        }

        if (ReferenceEquals(null, obj1)) {
            return true;
        }

        return !obj1.Equals(obj2);
    }

    public bool Equals(GraphSegment other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        var goodFukenListEquals = Utils.GoodFukenListEquals(Vertexes, other.Vertexes);
        var fukenListEquals = Utils.GoodFukenListEquals(MovementTiles, other.MovementTiles);
        var equals = Graph.Equals(other.Graph);
        return goodFukenListEquals
               && fukenListEquals
               && equals;
    }

    public int CompareTo(GraphSegment other) {
        var cmp = Vertexes.Count.CompareTo(other.Vertexes.Count);
        if (cmp != 0) {
            return cmp;
        }

        cmp = MovementTiles.Count.CompareTo(other.MovementTiles.Count);
        if (cmp != 0) {
            return cmp;
        }

        for (var i = 0; i < Vertexes.Count; i++) {
            cmp = Vertexes[i].CompareTo(other.Vertexes[i]);
            if (cmp != 0) {
                return cmp;
            }
        }

        for (var i = 0; i < MovementTiles.Count; i++) {
            cmp = Utils.StupidVector2IntComparation(MovementTiles[i], other.MovementTiles[i]);
            if (cmp != 0) {
                return cmp;
            }
        }

        cmp = Graph.CompareTo(other.Graph);
        if (cmp != 0) {
            return cmp;
        }

        return 0;
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

        return Equals((GraphSegment)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(Vertexes, MovementTiles, Graph);
    }

    public override string ToString() {
        var str = "(Vertexes: [";
        str += string.Join(", ", Vertexes);
        str += "], MovementTiles: [";
        str += string.Join(", ", MovementTiles);
        str += "])";
        return str;
    }
}
}
