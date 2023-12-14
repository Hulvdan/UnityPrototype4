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
    public readonly Guid id = Guid.NewGuid();

    public readonly List<GraphVertex> vertices;
    public readonly Graph graph;
    public readonly List<Vector2Int> movementTiles;
    public Human assignedHuman;
    public readonly UniqueList<MapResource> linkedResources;
    public readonly SimplePriorityQueue<MapResource> resourcesToTransport;

    public GraphSegment(
        List<GraphVertex> vertices_,
        List<Vector2Int> movementTiles_,
        Graph graph_
    ) {
        Assert.IsNotNull(vertices_);
        Assert.IsNotNull(movementTiles_);
        Assert.IsNotNull(graph_);

        vertices = vertices_;
        movementTiles = movementTiles_;
        graph = graph_;
        linkedResources = new();
        resourcesToTransport = new();

        // Duplication Checks
        for (var i = 0; i < vertices_.Count; i++) {
            for (var j = 0; j < vertices_.Count; j++) {
                if (i == j) {
                    continue;
                }

                Assert.AreNotEqual(vertices_[i], vertices_[j]);
            }
        }

        for (var i = 0; i < movementTiles_.Count; i++) {
            for (var j = 0; j < movementTiles_.Count; j++) {
                if (i == j) {
                    continue;
                }

                Assert.AreNotEqual(movementTiles_[i], movementTiles_[j]);
            }
        }
    }

    public List<GraphSegment> linkedSegments { get; } = new();

    public bool HasSomeOfTheSameVertices(GraphSegment other) {
        foreach (var otherVertex in other.vertices) {
            foreach (var vertex in vertices) {
                if (otherVertex == vertex) {
                    return true;
                }
            }
        }

        return false;
    }

    public void Link(GraphSegment other) {
        linkedSegments.Add(other);
    }

    public void Unlink(GraphSegment other) {
        linkedSegments.Remove(other);
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

        var verticesEqual = Utils.GoodFukenListEquals(vertices, other.vertices);
        var movementTilesEqual = Utils.GoodFukenListEquals(movementTiles, other.movementTiles);
        var graphEqual = graph.Equals(other.graph);
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
        return HashCode.Combine(vertices, movementTiles, graph);
    }

    public override string ToString() {
        var str = "(Vertices: [";
        str += string.Join(", ", vertices);
        str += "], MovementTiles: [";
        str += string.Join(", ", movementTiles);
        str += "])";
        return str;
    }
}
}
