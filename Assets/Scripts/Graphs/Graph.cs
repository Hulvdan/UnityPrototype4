using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using BFG.Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Graphs {
public sealed class Graph : IEquatable<Graph> {
    const int DEV_NUMBER_OF_BUILD_PATH_ITERATIONS = 256;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Node(Vector2Int pos) {
        return Node(pos.x, pos.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Node(int x, int y) {
        Assert.IsTrue(_finishedBuilding);
        Assert.IsTrue(Contains(x, y));
        Assert.AreNotEqual(_offset, null);

        var off = _offset!.Value;
        return nodes[y - off.y][x - off.x];
    }

    public void Mark(Vector2Int pos, Direction direction, bool value = true) {
        Mark(pos.x, pos.y, direction, value);
    }

    public void Mark(int x, int y, Direction direction, bool value = true) {
        Assert.IsFalse(_finishedBuilding);

        if (_offset == null) {
            _offset = new Vector2Int(x, y);

            var node = GraphNode.Mark(0, direction, value);
            nodes = new() { new() { node } };

            if (node != 0) {
                _nodesCount += 1;
            }
        }
        else {
            ResizeIfNeeded(x, y);

            var node = nodes[y - _offset.Value.y][x - _offset.Value.x];

            var nodeIsZeroButWontBeAfter = node == 0 && value;
            var nodeIsNotZeroButWillBe = !value && node != 0 &&
                                         GraphNode.Mark(node, direction, false) == 0;
            if (nodeIsZeroButWontBeAfter) {
                _nodesCount += 1;
            }
            else if (nodeIsNotZeroButWillBe) {
                _nodesCount -= 1;
            }

            node = GraphNode.Mark(node, direction, value);
            nodes[y - _offset.Value.y][x - _offset.Value.x] = node;
        }
    }

    #region Pathfinding

    /// <summary>
    ///     Builds a shortest path from originPos to destinationPos.
    ///     Graph has to contain both originPos and destinationPos.
    /// </summary>
    /// <returns>
    ///     A list of nodes starting from originPos up to the destinationPos.
    ///     Includes both originPos and destinationPos.
    /// </returns>
    public List<Vector2Int> GetShortestPath(Vector2Int originPos, Vector2Int destinationPos) {
        // 1. Wikipedia. Floyd–Warshall Algorithm. Look for `FloydWarshallWithPathReconstruction`.
        // It allows to efficiently reconstruct a path from any two connected vertices.
        // https://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm

        Assert.IsTrue(_finishedBuilding);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);
        Assert.IsTrue(_offset != null);

        Assert.IsTrue(Contains(originPos));
        Assert.IsTrue(Contains(destinationPos));

        return BuildPath(originPos, destinationPos, _offset!.Value, GetData());
    }

    static List<Vector2Int> BuildPath(
        Vector2Int originPos,
        Vector2Int destinationPos,
        Vector2Int offset,
        CalculatedGraphPathData data
    ) {
        // procedure Path(u, v)
        //     if prev[u][v] = null then
        //         return []
        //     path ← [v]
        //     while u ≠ v
        //         v ← prev[u][v]
        //         path.prepend(v)
        //     return path
        var nodeIndex2Pos = data.NodeIndex2Pos;
        var pos2NodeIndex = data.Pos2NodeIndex;
        var prev = data.Prev;

        Assert.IsTrue(pos2NodeIndex.ContainsKey(originPos - offset));
        Assert.IsTrue(pos2NodeIndex.ContainsKey(destinationPos - offset));
        var originNodeIndex = pos2NodeIndex[originPos - offset];
        var destinationNodeIndex = pos2NodeIndex[destinationPos - offset];

        var path = new List<Vector2Int> { destinationPos };
        var currentIteration = 0;
        while (
            currentIteration++ < DEV_NUMBER_OF_BUILD_PATH_ITERATIONS
            && originNodeIndex != destinationNodeIndex
        ) {
            var i = prev[originNodeIndex][destinationNodeIndex];
            Assert.IsTrue(i != int.MinValue);

            destinationNodeIndex = i;
            path.Add(nodeIndex2Pos[destinationNodeIndex] + offset);
        }

        Assert.IsTrue(currentIteration < DEV_NUMBER_OF_BUILD_PATH_ITERATIONS);

        path.Reverse();
        return path;
    }

    #endregion

    public List<Vector2Int> GetCenters() {
        // This algorithm is based off Reference #1.
        //
        // References:
        // 1. Codeforces. Center of a graph.
        // https://codeforces.com/blog/entry/17974
        //
        // 2. HAL open science. A new algorithm for graph center computation
        // and graph partitioning according to the distance to the center
        // https://hal.science/hal-02304090/document
        Assert.IsTrue(_finishedBuilding);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);
        Assert.AreNotEqual(_offset, null);
        Assert.IsTrue(IsUndirected());

        if (_centers != null) {
            return _centers;
        }

        var data = GetData();
        var dist = data.Dist;
        var nodeIndex2Pos = data.NodeIndex2Pos;

        // Counting values of eccentricity
        var nodeEccentricities = new int[_nodesCount];
        for (var i = 0; i < _nodesCount; i++) {
            for (var j = 0; j < _nodesCount; j++) {
                nodeEccentricities[i] = Math.Max(nodeEccentricities[i], dist[i][j]);
            }
        }

        var rad = int.MaxValue;
        var diam = 0;
        for (var i = 0; i < _nodesCount; i++) {
            rad = Math.Min(rad, nodeEccentricities[i]);
            diam = Math.Max(diam, nodeEccentricities[i]);
        }

        var centerNodeIndices = new HashSet<int>();
        for (var i = 0; i < _nodesCount; i++) {
            if (nodeEccentricities[i] == rad) {
                centerNodeIndices.Add(i);
            }
        }

        var centerNodePositions = new List<Vector2Int> { Capacity = centerNodeIndices.Count };

        foreach (var i in centerNodeIndices) {
            centerNodePositions.Add(nodeIndex2Pos[i] + _offset!.Value);
        }

        _centers = centerNodePositions;
        return centerNodePositions;
    }

    bool IsUndirected() {
        Assert.IsTrue(_finishedBuilding);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                if (!AdjacentTilesAreConnected(x, y)) {
                    return false;
                }
            }
        }

        return true;
    }

    bool AdjacentTilesAreConnected(int x, int y) {
        var node = nodes[y][x];

        foreach (var dir in Utils.Directions) {
            if (!GraphNode.Has(node, dir)) {
                continue;
            }

            var newPos = new Vector2Int(x, y) + dir.AsOffset();
            if (newPos.x < 0 || newPos.y < 0 || newPos.x >= width || newPos.y >= height) {
                return false;
            }

            var adjacentNode = nodes[newPos.y][newPos.x];
            var opposite = GraphNode.Has(adjacentNode, dir.Opposite());
            if (!opposite) {
                return false;
            }
        }

        return true;
    }

    public override string ToString() {
        Assert.IsTrue(_finishedBuilding);
        var builder = new StringBuilder();

        for (var y = 0; y < height; y++) {
            var row = nodes[height - y - 1];
            foreach (var node in row) {
                builder.Append(GraphNode.ToDisplayString(node));
            }

            if (y != height - 1) {
                builder.Append("\n");
            }
        }

        return builder.ToString();
    }

    public bool Equals(Graph other) {
        Assert.IsTrue(_finishedBuilding);

        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        Assert.IsTrue(other._finishedBuilding);

        return _nodesCount.Equals(other._nodesCount)
               && Nullable.Equals(_offset, other._offset)
               && Utils.GoodFuken2DListEquals(nodes, other.nodes);
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

        return Equals((Graph)obj);
    }

#pragma warning disable S2328 "GetHashCode" should not reference mutable fields
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() {
#pragma warning enable S2328
        Assert.IsTrue(_finishedBuilding);

        return HashCode.Combine(_offset, nodes);
    }

    void ResizeIfNeeded(int x, int y) {
        Assert.IsFalse(_finishedBuilding);
        Assert.AreNotEqual(_offset, null);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        var off = _offset!.Value;

        if (y < off.y) {
            var newNodes = new List<List<byte>>();

            var addedRowsCount = off.y - y;

            for (var yy = 0; yy < addedRowsCount; yy++) {
                var row = new List<byte> { Capacity = width };
                for (var xx = 0; xx < width; xx++) {
                    row.Add(new());
                }

                newNodes.Add(row);
            }

            foreach (var row in nodes) {
                newNodes.Add(row);
            }

            nodes = newNodes;
        }

        if (y >= off.y + height) {
            var addedRowsCount = y - height - off.y + 1;
            var oldWidth = width;

            for (var i = 0; i < addedRowsCount; i++) {
                var newRow = new List<byte> { Capacity = oldWidth };
                for (var j = 0; j < oldWidth; j++) {
                    newRow.Add(new());
                }

                nodes.Add(newRow);
            }
        }

        if (x < off.x) {
            var oldWidth = width;
            var addedColumnsCount = off.x - x;
            var newWidth = width + addedColumnsCount;

            for (var i = 0; i < height; i++) {
                var newRow = new List<byte> { Capacity = newWidth };

                for (var xx = 0; xx < addedColumnsCount; xx++) {
                    newRow.Add(new());
                }

                for (var xx = 0; xx < oldWidth; xx++) {
                    newRow.Add(nodes[i][xx]);
                }

                nodes[i] = newRow;
            }
        }

        if (x >= off.x + width) {
            var addedColumnsCount = x - width - off.x + 1;

            foreach (var row in nodes) {
                for (var i = 0; i < addedColumnsCount; i++) {
                    row.Add(new());
                }
            }
        }

        _offset = new Vector2Int(
            Math.Min(x, off.x),
            Math.Min(y, off.y)
        );
    }

    public bool Contains(int x, int y) {
        var off = offset;
        return y >= off.y
               && y < off.y + height
               && x >= off.x
               && x < off.x + width;
    }

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public Vector2Int offset {
        get {
            Assert.AreNotEqual(_offset, null);
            return _offset!.Value;
        }
    }

    public List<List<byte>> nodes { get; private set; } = new();

    public int height => nodes.Count;
    public int width => nodes[0].Count;

    public static class Tests {
        public static List<List<byte>> GetNodes(Graph graph) {
            return graph.nodes;
        }

        public static bool IsUndirected(Graph graph) {
            return graph.IsUndirected();
        }
    }

    Vector2Int? _offset;
    int _nodesCount;

    CalculatedGraphPathData GetData() {
        if (_data == null) {
            _data = RecalculateData();
        }

        return _data;
    }

    [CanBeNull]
    CalculatedGraphPathData _data;

    List<Vector2Int> _centers;
    bool _finishedBuilding;

    public void FinishBuilding() {
        Assert.IsFalse(_finishedBuilding);
        Assert.IsTrue(_nodesCount > 0);
        Assert.AreNotEqual(_offset, null);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        _finishedBuilding = true;
    }

    public int Cost(Vector2Int origin, Vector2Int destination) {
        Assert.AreNotEqual(Node(origin), (byte)0);
        Assert.AreNotEqual(Node(destination), (byte)0);

        var data = GetData();
        var iOrigin = data.Pos2NodeIndex[origin];
        var iDestination = data.Pos2NodeIndex[destination];

        return data.Dist[iOrigin][iDestination];
    }

    CalculatedGraphPathData RecalculateData() {
        var nodeIndex2Pos = new Dictionary<int, Vector2Int>();
        var pos2NodeIndex = new Dictionary<Vector2Int, int>();

        var nodeIndex = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = nodes[y][x];
                if (node == 0) {
                    continue;
                }

                nodeIndex2Pos.Add(nodeIndex, new(x, y));
                pos2NodeIndex.Add(new(x, y), nodeIndex);
                nodeIndex += 1;
            }
        }

        // NOTE: |V| = _nodesCount
        // > let dist be a |V| × |V| array of minimum distances initialized to ∞ (infinity)
        // > let prev be a |V| × |V| array of minimum distances initialized to null
        var dist = new int[_nodesCount][];
        var prev = new int[_nodesCount][];
        for (var y = 0; y < _nodesCount; y++) {
            var distRow = new int[_nodesCount];
            for (var x = 0; x < _nodesCount; x++) {
                distRow[x] = int.MaxValue;
            }

            dist[y] = distRow;

            var prevRow = new int[_nodesCount];
            for (var x = 0; x < _nodesCount; x++) {
                prevRow[x] = int.MinValue;
            }

            prev[y] = prevRow;
        }

        // NOTE: edge (u, v) = (nodeIndex, newNodeIndex)
        // > for each edge (u, v) do
        // >     dist[u][v] ← w(u, v)  // The weight of the edge (u, v)
        // >     prev[u][v] ← u
        nodeIndex = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = nodes[y][x];
                if (node == 0) {
                    continue;
                }

                foreach (var dir in Utils.Directions) {
                    if (!GraphNode.Has(node, dir)) {
                        continue;
                    }

                    var newPos = new Vector2Int(x, y) + dir.AsOffset();
                    var newNodeIndex = pos2NodeIndex[newPos];
                    dist[nodeIndex][newNodeIndex] = 1;
                    prev[nodeIndex][newNodeIndex] = nodeIndex;
                }

                nodeIndex += 1;
            }
        }

        // NOTE: vertex v = nodeIndex
        // > for each vertex v do
        // >     dist[v][v] ← 0
        // >     prev[v][v] ← v
        nodeIndex = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = nodes[y][x];
                if (node == 0) {
                    continue;
                }

                dist[nodeIndex][nodeIndex] = 0;
                prev[nodeIndex][nodeIndex] = nodeIndex;

                nodeIndex += 1;
            }
        }

        // Standard Floyd-Warshall
        // for k from 1 to |V|
        //     for i from 1 to |V|
        //         for j from 1 to |V|
        //             if dist[i][j] > dist[i][k] + dist[k][j] then
        //                 dist[i][j] ← dist[i][k] + dist[k][j]
        //                 prev[i][j] ← prev[k][j]
        for (var k = 0; k < _nodesCount; k++) {
            for (var j = 0; j < _nodesCount; j++) {
                for (var i = 0; i < _nodesCount; i++) {
                    var ij = dist[i][j];
                    var ik = dist[i][k];
                    var kj = dist[k][j];

                    if (
                        ik != int.MaxValue
                        && kj != int.MaxValue
                        && ij > ik + kj
                    ) {
                        dist[i][j] = ik + kj;
                        prev[i][j] = prev[k][j];
                    }
                }
            }
        }

        return new(dist, prev, nodeIndex2Pos, pos2NodeIndex);
    }
}

internal class CalculatedGraphPathData {
    public readonly int[][] Dist;
    public readonly int[][] Prev;
    public readonly Dictionary<int, Vector2Int> NodeIndex2Pos;
    public readonly Dictionary<Vector2Int, int> Pos2NodeIndex;

    public CalculatedGraphPathData(
        int[][] dist,
        int[][] prev,
        Dictionary<int, Vector2Int> nodeIndex2Pos,
        Dictionary<Vector2Int, int> pos2NodeIndex
    ) {
        Dist = dist;
        Prev = prev;
        NodeIndex2Pos = nodeIndex2Pos;
        Pos2NodeIndex = pos2NodeIndex;
    }
}
}
