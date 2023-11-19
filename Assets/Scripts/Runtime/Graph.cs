using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class Graph : IEquatable<Graph>, IComparable<Graph> {
    public void SetDirection(
        Vector2Int pos, Direction direction, bool value = true
    ) {
        SetDirection(pos.x, pos.y, direction, value);
    }

    public void SetDirection(int x, int y, Direction direction, bool value = true) {
        if (_offset == null) {
            _offset = new Vector2Int(x, y);

            var node = GraphNode.SetDirection(0, direction, value);
            _nodes = new() { new() { node } };

            _nodesCount += 1;
        }
        else {
            ResizeIfNeeded(x, y);

            var node = _nodes[y - _offset.Value.y][x - _offset.Value.x];

            if (!GraphNode.Has(node, direction) && value) {
                _nodesCount += 1;
            }
            else if (GraphNode.Has(node, direction) && !value) {
                _nodesCount -= 1;
            }

            node = GraphNode.SetDirection(node, direction, value);
            _nodes[y - _offset.Value.y][x - _offset.Value.x] = node;
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

        Assert.IsTrue(_nodes.Count > 0);
        Assert.IsTrue(_nodes[0].Count > 0);

        Assert.IsTrue(ContainsNode(originPos));
        Assert.IsTrue(ContainsNode(destinationPos));

        var height = _nodes.Count;
        var width = _nodes[0].Count;

        var nodeIndex2Pos = new Dictionary<int, Vector2Int>();
        var pos2IndexNode = new Dictionary<Vector2Int, int>();

        var nodeIndex = 0;
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = _nodes[y][x];
                if (node == 0) {
                    continue;
                }

                nodeIndex2Pos.Add(nodeIndex, new(x, y));
                pos2IndexNode.Add(new(x, y), nodeIndex);
                nodeIndex += 1;
            }
        }

        // NOTE: |V| = _nodesCount
        // > let dist be a |V| × |V| array of minimum distances initialized to ∞ (infinity)
        // > let prev be a |V| × |V| array of minimum distances initialized to null
        var dist = new float[_nodesCount][];
        var prev = new int[_nodesCount][];
        for (var y = 0; y < _nodesCount; y++) {
            var distRow = new float[_nodesCount];
            for (var x = 0; x < _nodesCount; x++) {
                distRow[x] = float.PositiveInfinity;
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
                var node = _nodes[y][x];
                if (node == 0) {
                    continue;
                }

                for (Direction dir = 0; dir < (Direction)4; dir++) {
                    if (!GraphNode.Has(node, dir)) {
                        continue;
                    }

                    var newPos = new Vector2Int(x, y) + dir.AsOffset();
                    var newNodeIndex = pos2IndexNode[newPos];
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
                var node = _nodes[y][x];
                if (node == 0) {
                    continue;
                }

                dist[nodeIndex][nodeIndex] = 0;
                prev[nodeIndex][nodeIndex] = nodeIndex;

                nodeIndex += 1;
            }
        }

        // for k from 1 to |V|
        //     for i from 1 to |V|
        //         for j from 1 to |V|
        //             if dist[i][j] > dist[i][k] + dist[k][j]
        //                 dist[i][j] ← dist[i][k] + dist[k][j]
        //                 prev[i][j] ← prev[k][j]
        //             end if
        for (var k = 0; k < _nodesCount; k++) {
            for (var i = 0; i < _nodesCount; i++) {
                for (var j = 0; j < _nodesCount; j++) {
                    var ij = dist[i][j];
                    var ik = dist[i][k];
                    var kj = dist[k][j];

                    if (ij > ik + kj) {
                        dist[i][j] = ik + kj;
                        prev[i][j] = prev[k][j];
                    }
                }
            }
        }

        return BuildPath(originPos, destinationPos, pos2IndexNode, nodeIndex2Pos, prev);
    }

    static List<Vector2Int> BuildPath(
        Vector2Int originPos,
        Vector2Int destinationPos,
        IReadOnlyDictionary<Vector2Int, int> pos2IndexNode,
        IReadOnlyDictionary<int, Vector2Int> nodeIndex2Pos,
        IReadOnlyList<IReadOnlyList<int>> prev
    ) {
        // procedure Path(u, v)
        //     if prev[u][v] = null then
        //         return []
        //     path ← [v]
        //     while u ≠ v
        //         v ← prev[u][v]
        //         path.prepend(v)
        //     return path
        Assert.IsTrue(pos2IndexNode.ContainsKey(originPos));
        Assert.IsTrue(pos2IndexNode.ContainsKey(destinationPos));
        var originNodeIndex = pos2IndexNode[originPos];
        var destinationNodeIndex = pos2IndexNode[destinationPos];

        var path = new List<Vector2Int> { destinationPos };
        while (originNodeIndex != destinationNodeIndex) {
            var i = prev[originNodeIndex][destinationNodeIndex];
            Assert.IsTrue(i != int.MinValue);

            destinationNodeIndex = i;
            path.Add(nodeIndex2Pos[destinationNodeIndex]);
        }

        path.Reverse();
        return path;
    }

    #endregion

    public List<Vector2Int> GetCenters() {
        return new();
    }

    public List<Vector2Int> GetCentroids() {
        return new();
    }

    public bool IsUndirected() {
        Assert.IsTrue(_nodes.Count > 0);
        Assert.IsTrue(_nodes[0].Count > 0);

        var height = _nodes.Count;
        var width = _nodes[0].Count;

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                var node = _nodes[y][x];

                for (Direction dir = 0; dir < (Direction)4; dir++) {
                    if (!GraphNode.Has(node, dir)) {
                        continue;
                    }

                    var offset = dir.AsOffset();
                    var newX = x + offset.x;
                    var newY = y + offset.y;
                    if (newX < 0 || newY < 0 || newX >= width || newY >= height) {
                        return false;
                    }

                    var adjacentNode = _nodes[newY][newX];
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
        foreach (var row in _nodes) {
            foreach (var node in row) {
                res += GraphNode.Repr(node);
            }

            res += "\n";
        }

        if (_nodes.Count > 0) {
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

        return Nullable.Equals(_offset, other._offset) && Equals(_nodes, other._nodes);
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

        if (_nodes.Count > other._nodes.Count) {
            return 1;
        }

        if (_nodes.Count < other._nodes.Count) {
            return 1;
        }

        for (var y = 0; y < _nodes.Count; y++) {
            if (_nodes[y].Count > other._nodes[y].Count) {
                return 1;
            }

            if (_nodes[y].Count > other._nodes[y].Count) {
                return -1;
            }
        }

        for (var y = 0; y < _nodes.Count; y++) {
            for (var x = 0; x < _nodes[y].Count; x++) {
                if (_nodes[y][x] > other._nodes[y][x]) {
                    return 1;
                }

                if (_nodes[y][x] < other._nodes[y][x]) {
                    return -1;
                }
            }
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

        return Equals((Graph)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine(_offset, _nodes);
    }

    void ResizeIfNeeded(int x, int y) {
        Assert.IsTrue(_offset.HasValue);
        Assert.IsTrue(_nodes.Count > 0);
        Assert.IsTrue(_nodes[0].Count > 0);

        if (y < _offset.Value.y) {
            var newNodes = new List<List<byte>>();

            var width = _nodes[0].Count;
            var addedRowsCount = _offset.Value.y - y;

            for (var yy = 0; yy < addedRowsCount; yy++) {
                var row = new List<byte> { Capacity = width };
                for (var xx = 0; xx < width; xx++) {
                    row.Add(new());
                }

                newNodes.Add(row);
            }

            foreach (var row in _nodes) {
                newNodes.Add(row);
            }

            _nodes = newNodes;
        }

        if (y >= _nodes.Count) {
            var addedRowsCount = y - _nodes.Count + 1;
            var oldWidth = _nodes[0].Count;

            for (var i = 0; i < addedRowsCount; i++) {
                var newRow = new List<byte> { Capacity = oldWidth };
                for (var j = 0; j < oldWidth; j++) {
                    newRow.Add(new());
                }

                _nodes.Add(newRow);
            }
        }

        if (x < _offset.Value.x) {
            var oldWidth = _nodes[0].Count;
            var addedColumnsCount = _offset.Value.x - x;
            var newWidth = _nodes[0].Count + addedColumnsCount;

            for (var i = 0; i < _nodes.Count; i++) {
                var newRow = new List<byte> { Capacity = newWidth };

                for (var xx = 0; xx < addedColumnsCount; xx++) {
                    newRow.Add(new());
                }

                for (var xx = 0; xx < oldWidth; xx++) {
                    newRow.Add(_nodes[i][xx]);
                }

                _nodes[i] = newRow;
            }
        }

        if (x >= _nodes[0].Count) {
            var addedColumnsCount = x - _nodes[0].Count + 1;

            foreach (var row in _nodes) {
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

    bool ContainsNode(Vector2Int pos) {
        var y = pos.y;
        var x = pos.x;
        return y >= _offset.Value.y
               && y < _nodes.Count
               && x >= _offset.Value.x
               && x < _nodes[0].Count;
    }

    public static class Tests {
        public static List<List<byte>> GetNodes(Graph graph) {
            return graph._nodes;
        }
    }

    Vector2Int? _offset;
    List<List<byte>> _nodes = new();
    int _nodesCount;
}
}
