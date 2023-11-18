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
        Vertexes = vertexes;
        MovementTiles = movementTiles;

        // TODO: Should be considered for removal
        Graph = graph;
        vertexes.Sort();
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

    public bool Equals(GraphSegment other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(Vertexes, other.Vertexes)
               && Equals(Graph, other.Graph)
               && Equals(MovementTiles, other.MovementTiles);
    }

    public int CompareTo(GraphSegment other) {
        if (Vertexes.Count > other.Vertexes.Count) {
            return 1;
        }

        if (Vertexes.Count < other.Vertexes.Count) {
            return -1;
        }

        if (MovementTiles.Count > other.MovementTiles.Count) {
            return 1;
        }

        if (MovementTiles.Count > other.MovementTiles.Count) {
            return -1;
        }

        for (var i = 0; i < Vertexes.Count; i++) {
            var cmp = Vertexes[i].CompareTo(other.Vertexes[i]);
            if (cmp != 0) {
                return cmp;
            }
        }

        for (var i = 0; i < MovementTiles.Count; i++) {
            var cmp = Utils.StupidVector2IntComparation(MovementTiles[i], other.MovementTiles[i]);
            if (cmp != 0) {
                return cmp;
            }
        }

        var graphCmp = Graph.CompareTo(other.Graph);
        if (graphCmp != 0) {
            return graphCmp;
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
