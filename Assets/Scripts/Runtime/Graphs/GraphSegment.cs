using System;
using System.Collections.Generic;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Entities;
using Priority_Queue;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Graphs {
public sealed class GraphSegment : IEquatable<GraphSegment> {
    public readonly Guid ID = Guid.NewGuid();

    public readonly List<GraphVertex> Vertices;
    public readonly Graph Graph;
    public readonly List<Vector2Int> MovementTiles;
    public Human AssignedHuman;
    public readonly UniqueList<MapResource> LinkedResources;
    public readonly SimplePriorityQueue<MapResource> ResourcesToTransport;

    public GraphSegment(
        List<GraphVertex> vertices,
        List<Vector2Int> movementTiles,
        Graph graph
    ) {
        Assert.IsNotNull(vertices);
        Assert.IsNotNull(movementTiles);
        Assert.IsNotNull(graph);

        Vertices = vertices;
        MovementTiles = movementTiles;
        Graph = graph;
        LinkedResources = new();
        ResourcesToTransport = new();

        // Duplication Checks
        for (var i = 0; i < vertices.Count; i++) {
            for (var j = 0; j < vertices.Count; j++) {
                if (i == j) {
                    continue;
                }

                Assert.AreNotEqual(vertices[i], vertices[j]);
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

    public List<GraphSegment> LinkedSegments { get; } = new();

    public bool HasSomeOfTheSameVertices(GraphSegment other) {
        foreach (var otherVertex in other.Vertices) {
            foreach (var vertex in Vertices) {
                if (otherVertex == vertex) {
                    return true;
                }
            }
        }

        return false;
    }

    public void Link(GraphSegment other) {
        LinkedSegments.Add(other);
    }

    public void Unlink(GraphSegment other) {
        LinkedSegments.Remove(other);
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

        var verticesEqual = Utils.GoodFukenListEquals(Vertices, other.Vertices);
        var movementTilesEqual = Utils.GoodFukenListEquals(MovementTiles, other.MovementTiles);
        var graphEqual = Graph.Equals(other.Graph);
        return verticesEqual
               && movementTilesEqual
               && graphEqual;
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
        return HashCode.Combine(Vertices, MovementTiles, Graph);
    }

    public override string ToString() {
        var str = "(Vertices: [";
        str += string.Join(", ", Vertices);
        str += "], MovementTiles: [";
        str += string.Join(", ", MovementTiles);
        str += "])";
        return str;
    }
}
}
