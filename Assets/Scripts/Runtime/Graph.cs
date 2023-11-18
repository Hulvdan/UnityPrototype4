using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime {
public class Graph {
    Vector2Int? _offset;
    List<List<GraphNode>> _nodes = new();

    public void SetDirection(Vector2Int pos, Direction direction) {
        SetDirection(pos.x, pos.y, direction);
    }

    public void SetDirection(int x, int y, Direction direction) {
        if (_offset == null) {
            _offset = new Vector2Int(x, y);

            var node = new GraphNode();
            node.SetDirection(true, (byte)direction);
            _nodes = new() { new() { node } };
        }
        else {
            ResizeIfNeeded(x, y);

            var node = _nodes[y - _offset.Value.y][x - _offset.Value.x];
            node.SetDirection(true, (byte)direction);
            _nodes[y - _offset.Value.y][x - _offset.Value.x] = node;
        }
    }

    void ResizeIfNeeded(int x, int y) {
        Assert.IsTrue(_offset.HasValue);
        Assert.IsTrue(_nodes.Count > 0);
        Assert.IsTrue(_nodes[0].Count > 0);

        if (y < _offset.Value.y) {
            var newNodes = new List<List<GraphNode>>();

            var width = _nodes[0].Count;
            var addedRowsCount = _offset.Value.y - y;

            for (var yy = 0; yy < addedRowsCount; yy++) {
                var row = new List<GraphNode> { Capacity = width };
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
                var newRow = new List<GraphNode> { Capacity = oldWidth };
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
                var newRow = new List<GraphNode> { Capacity = newWidth };

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

    public override string ToString() {
        var res = "";
        foreach (var row in _nodes) {
            foreach (var node in row) {
                res += node.Repr();
            }

            res += "\n";
        }

        if (_nodes.Count > 0) {
            res = res.TrimEnd('\n');
        }

        return res;
    }

    public static class Tests {
        public static List<List<GraphNode>> GetNodes(Graph graph) {
            return graph._nodes;
        }
    }
}
}
