using System.Collections.Generic;
using BFG.Runtime;
using NUnit.Framework;

namespace Tests.EditMode {
public class TestGraph {
    [Test]
    public void Test_GraphNode() {
        var node = new GraphNode();
        Assert.IsFalse(node.right);
        Assert.IsFalse(node.up);
        Assert.IsFalse(node.left);
        Assert.IsFalse(node.down);

        node.right = true;
        Assert.IsTrue(node.right);
        Assert.IsFalse(node.up);
        Assert.IsFalse(node.left);
        Assert.IsFalse(node.down);

        node.up = true;
        Assert.IsTrue(node.right);
        Assert.IsTrue(node.up);
        Assert.IsFalse(node.left);
        Assert.IsFalse(node.down);

        node.left = true;
        Assert.IsTrue(node.right);
        Assert.IsTrue(node.up);
        Assert.IsTrue(node.left);
        Assert.IsFalse(node.down);

        node.down = true;
        Assert.IsTrue(node.right);
        Assert.IsTrue(node.up);
        Assert.IsTrue(node.left);
        Assert.IsTrue(node.down);

        var node1 = new GraphNode(new[] { false, false, false, false });
        Assert.IsFalse(node1.right);
        Assert.IsFalse(node1.up);
        Assert.IsFalse(node1.left);
        Assert.IsFalse(node1.down);
        var node2 = new GraphNode(new[] { true, false, false, false });
        Assert.IsTrue(node2.right);
        Assert.IsFalse(node2.up);
        Assert.IsFalse(node2.left);
        Assert.IsFalse(node2.down);
        var node3 = new GraphNode(new[] { false, true, false, false });
        Assert.IsFalse(node3.right);
        Assert.IsTrue(node3.up);
        Assert.IsFalse(node3.left);
        Assert.IsFalse(node3.down);
        var node4 = new GraphNode(new[] { false, false, true, false });
        Assert.IsFalse(node4.right);
        Assert.IsFalse(node4.up);
        Assert.IsTrue(node4.left);
        Assert.IsFalse(node4.down);
        var node5 = new GraphNode(new[] { false, false, false, true });
        Assert.IsFalse(node5.right);
        Assert.IsFalse(node5.up);
        Assert.IsFalse(node5.left);
        Assert.IsTrue(node5.down);
    }

    [Test]
    [Timeout(1)]
    public void Test_1() {
        // ╶╵╴╷┼
        // ┌┐└┘─│
        // ├ ┬ ┴ ┤
        Test(
            new[] {
                "╶╴",
            },
            new() {
                new() {
                    new(new[] { true, false, false, false }),
                    new(new[] { false, false, true, false }),
                },
            }
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2() {
        Test(
            new[] {
                "┌┐",
                "└┘",
            },
            new() {
                new() {
                    new(new[] { true, true, false, false }),
                    new(new[] { false, true, true, false }),
                },
                new() {
                    new(new[] { true, false, false, true }),
                    new(new[] { false, false, true, true }),
                },
            }
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_3() {
        var graph = new Graph();
        graph.SetDirection(1, 1, Direction.Right);

        var actual = Graph.Tests.GetNodes(graph);
        Assert.AreEqual(
            new List<List<GraphNode>> {
                new() {
                    new(new[] { true, false, false, false }),
                },
            },
            actual
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_4() {
        var graph = new Graph();
        graph.SetDirection(1, 1, Direction.Right);
        graph.SetDirection(0, 0, Direction.Left);

        var actual = Graph.Tests.GetNodes(graph);
        Assert.AreEqual(
            new List<List<GraphNode>> {
                new() {
                    new(new[] { false, false, true, false }),
                    new(),
                },
                new() {
                    new(),
                    new(new[] { true, false, false, false }),
                },
            },
            actual
        );
    }

    Graph FromStrings(string[] strings) {
        var height = strings.Length;
        Assert.IsTrue(height > 0);
        var width = strings[0].Length;
        foreach (var str in strings) {
            Assert.AreEqual(width, str.Length);
        }

        var graph = new Graph();
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                switch (strings[height - y - 1][x]) {
                    case '╶':
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '╵':
                        graph.SetDirection(x, y, Direction.Up);
                        break;
                    case '╴':
                        graph.SetDirection(x, y, Direction.Left);
                        break;
                    case '╷':
                        graph.SetDirection(x, y, Direction.Down);
                        break;
                    case '┌':
                        graph.SetDirection(x, y, Direction.Down);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '┐':
                        graph.SetDirection(x, y, Direction.Left);
                        graph.SetDirection(x, y, Direction.Down);
                        break;
                    case '└':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '┘':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Left);
                        break;
                    case '─':
                        graph.SetDirection(x, y, Direction.Left);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '│':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Down);
                        break;
                    case '├':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Down);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '┬':
                        graph.SetDirection(x, y, Direction.Left);
                        graph.SetDirection(x, y, Direction.Down);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '┴':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Left);
                        graph.SetDirection(x, y, Direction.Right);
                        break;
                    case '┤':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Down);
                        graph.SetDirection(x, y, Direction.Left);
                        break;
                    case '┼':
                        graph.SetDirection(x, y, Direction.Up);
                        graph.SetDirection(x, y, Direction.Down);
                        graph.SetDirection(x, y, Direction.Right);
                        graph.SetDirection(x, y, Direction.Left);
                        break;
                    case '.':
                        break;
                    default:
                        Assert.IsTrue(false);
                        break;
                }
            }
        }

        return graph;
    }

    void Test(string[] strings, List<List<GraphNode>> expectedNodesGraph) {
        var graph = FromStrings(strings);
        var actual = Graph.Tests.GetNodes(graph);
        Assert.AreEqual(expectedNodesGraph, actual);
    }
}
}
