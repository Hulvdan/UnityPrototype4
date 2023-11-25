using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BFG.Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Graphs {
public class Graph : IEquatable<Graph>, IComparable<Graph> {
    const int DEV_NUMBER_OF_BUILD_PATH_ITERATIONS = 256;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Node(Vector2Int pos) {
        return Node(pos.x, pos.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Node(int x, int y) {
        Assert.IsTrue(Contains(x, y));
        Assert.IsTrue(_offset != null);
        return Nodes[y - _offset.Value.y][x - _offset.Value.x];
    }

    public void SetDirection(Vector2Int pos, Direction direction, bool value = true) {
        SetDirection(pos.x, pos.y, direction, value);
    }

    public void SetDirection(int x, int y, Direction direction, bool value = true) {
        if (_offset == null) {
            _offset = new Vector2Int(x, y);

            var node = GraphNode.SetDirection(0, direction, value);
            Nodes = new() { new() { node } };

            if (node != 0) {
                _nodesCount += 1;
            }
        }
        else {
            ResizeIfNeeded(x, y);

            var node = Nodes[y - _offset.Value.y][x - _offset.Value.x];

            var nodeIsZeroButWontBeAfter = node == 0 && value;
            var nodeIsNotZeroButWillBe = !value && node != 0 &&
                                         GraphNode.SetDirection(node, direction, false) == 0;
            if (nodeIsZeroButWontBeAfter) {
                _nodesCount += 1;
            }
            else if (nodeIsNotZeroButWillBe) {
                _nodesCount -= 1;
            }

            node = GraphNode.SetDirection(node, direction, value);
            Nodes[y - _offset.Value.y][x - _offset.Value.x] = node;
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

        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        Assert.IsTrue(Contains(originPos));
        Assert.IsTrue(Contains(destinationPos));

        return BuildPath(
            originPos,
            destinationPos,
            _offset.Value,
            GetData()
        );
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
        var nodeIndex2Pos = data.nodeIndex2Pos;
        var pos2NodeIndex = data.pos2NodeIndex;
        var prev = data.prev;

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

        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        if (_centers != null) {
            return _centers;
        }

        var data = GetData();
        var dist = data.dist;
        var nodeIndex2Pos = data.nodeIndex2Pos;

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

        Assert.IsTrue(_offset.HasValue);
        foreach (var i in centerNodeIndices) {
            centerNodePositions.Add(nodeIndex2Pos[i] + _offset.Value);
        }

        _centers = centerNodePositions;
        return centerNodePositions;
    }

    public bool IsUndirected() {
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = Nodes[y][x];

                foreach (var dir in Utils.Directions) {
                    if (!GraphNode.Has(node, dir)) {
                        continue;
                    }

                    var offset = dir.AsOffset();
                    var newX = x + offset.x;
                    var newY = y + offset.y;
                    if (newX < 0 || newY < 0 || newX >= width || newY >= height) {
                        return false;
                    }

                    var adjacentNode = Nodes[newY][newX];
                    var opposite = GraphNode.Has(adjacentNode, dir.Opposite());
                    if (!opposite) {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public override string ToString() {
        var res = "";
        for (var y = 0; y < height; y++) {
            var row = Nodes[height - y - 1];
            foreach (var node in row) {
                res += GraphNode.Repr(node);
            }

            res += "\n";
        }

        if (height > 0) {
            res = res.TrimEnd('\n');
        }

        return res;
    }

    public bool Equals(Graph other) {
        if (ReferenceEquals(null, other)) {
            return false;
        }

        if (ReferenceEquals(this, other)) {
            return true;
        }

        return _nodesCount.Equals(other._nodesCount)
               && Nullable.Equals(_offset, other._offset)
               && Utils.GoodFuken2DListEquals(Nodes, other.Nodes);
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

    public int CompareTo(Graph other) {
        if (_offset == null && other._offset != null) {
            return 1;
        }

        if (_offset != null && other._offset == null) {
            return -1;
        }

        if (_offset != null && other._offset != null) {
            if (_offset.Value.x > other._offset.Value.x) {
                return 1;
            }

            if (_offset.Value.x < other._offset.Value.x) {
                return -1;
            }

            if (_offset.Value.y > other._offset.Value.y) {
                return 1;
            }

            if (_offset.Value.y < other._offset.Value.y) {
                return -1;
            }
        }

        if (height > other.height) {
            return 1;
        }

        if (height < other.height) {
            return 1;
        }

        for (var y = 0; y < height; y++) {
            if (Nodes[y].Count > other.Nodes[y].Count) {
                return 1;
            }

            if (Nodes[y].Count > other.Nodes[y].Count) {
                return -1;
            }
        }

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < Nodes[y].Count; x++) {
                if (Nodes[y][x] > other.Nodes[y][x]) {
                    return 1;
                }

                if (Nodes[y][x] < other.Nodes[y][x]) {
                    return -1;
                }
            }
        }

        return 0;
    }

    public override int GetHashCode() {
        return HashCode.Combine(_offset, Nodes);
    }

    void ResizeIfNeeded(int x, int y) {
        Assert.IsTrue(_offset.HasValue);
        Assert.IsTrue(height > 0);
        Assert.IsTrue(width > 0);

        if (y < _offset.Value.y) {
            var newNodes = new List<List<byte>>();

            var addedRowsCount = _offset.Value.y - y;

            for (var yy = 0; yy < addedRowsCount; yy++) {
                var row = new List<byte> { Capacity = width };
                for (var xx = 0; xx < width; xx++) {
                    row.Add(new());
                }

                newNodes.Add(row);
            }

            foreach (var row in Nodes) {
                newNodes.Add(row);
            }

            Nodes = newNodes;
        }

        if (y >= _offset.Value.y + height) {
            var addedRowsCount = y - height - _offset.Value.y + 1;
            var oldWidth = width;

            for (var i = 0; i < addedRowsCount; i++) {
                var newRow = new List<byte> { Capacity = oldWidth };
                for (var j = 0; j < oldWidth; j++) {
                    newRow.Add(new());
                }

                Nodes.Add(newRow);
            }
        }

        if (x < _offset.Value.x) {
            var oldWidth = width;
            var addedColumnsCount = _offset.Value.x - x;
            var newWidth = width + addedColumnsCount;

            for (var i = 0; i < height; i++) {
                var newRow = new List<byte> { Capacity = newWidth };

                for (var xx = 0; xx < addedColumnsCount; xx++) {
                    newRow.Add(new());
                }

                for (var xx = 0; xx < oldWidth; xx++) {
                    newRow.Add(Nodes[i][xx]);
                }

                Nodes[i] = newRow;
            }
        }

        if (x >= _offset.Value.x + width) {
            var addedColumnsCount = x - width - _offset.Value.x + 1;

            foreach (var row in Nodes) {
                for (var i = 0; i < addedColumnsCount; i++) {
                    row.Add(new());
                }
            }
        }

        _offset = new Vector2Int(
            Math.Min(x, _offset.Value.x),
            Math.Min(y, _offset.Value.y)
        );
    }

    public bool Contains(int x, int y) {
        return y >= _offset.Value.y
               && y < _offset.Value.y + height
               && x >= _offset.Value.x
               && x < _offset.Value.x + width;
    }

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public Vector2Int Offset {
        get {
            if (!_offset.HasValue) {
                Debug.LogError("WTF?");
                return Vector2Int.zero;
            }

            return _offset.Value;
        }
    }

    public List<List<byte>> Nodes { get; private set; } = new();

    public int height => Nodes.Count;
    public int width => Nodes[0].Count;

    public static class Tests {
        public static List<List<byte>> GetNodes(Graph graph) {
            return graph.Nodes;
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

    public int Cost(Vector2Int origin, Vector2Int destination) {
        Assert.AreNotEqual(Node(origin), (byte)0);
        Assert.AreNotEqual(Node(destination), (byte)0);

        var data = GetData();
        var iOrigin = data.pos2NodeIndex[origin];
        var iDestination = data.pos2NodeIndex[destination];

        return data.dist[iOrigin][iDestination];
    }

    CalculatedGraphPathData RecalculateData() {
        var nodeIndex2Pos = new Dictionary<int, Vector2Int>();
        var pos2NodeIndex = new Dictionary<Vector2Int, int>();

        var nodeIndex = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = Nodes[y][x];
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
                var node = Nodes[y][x];
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
                var node = Nodes[y][x];
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

                    if (ik != int.MaxValue && kj != int.MaxValue) {
                        if (ij > ik + kj) {
                            dist[i][j] = ik + kj;
                            prev[i][j] = prev[k][j];
                        }
                    }
                }
            }
        }

        return new(dist, prev, nodeIndex2Pos, pos2NodeIndex);
    }
}

internal class CalculatedGraphPathData {
    public int[][] dist;
    public int[][] prev;
    public Dictionary<int, Vector2Int> nodeIndex2Pos;
    public Dictionary<Vector2Int, int> pos2NodeIndex;

    public CalculatedGraphPathData(
        int[][] dist,
        int[][] prev,
        Dictionary<int, Vector2Int> nodeIndex2Pos,
        Dictionary<Vector2Int, int> pos2NodeIndex
    ) {
        this.dist = dist;
        this.prev = prev;
        this.nodeIndex2Pos = nodeIndex2Pos;
        this.pos2NodeIndex = pos2NodeIndex;
    }
}
}
