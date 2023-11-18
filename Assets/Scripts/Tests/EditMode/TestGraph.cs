using System.Collections.Generic;
using BFG.Runtime;
using NUnit.Framework;

namespace Tests.EditMode {
public class TestGraph {
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
                    GraphNode.Right,
                    GraphNode.Left,
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
                    (byte)(GraphNode.Right | GraphNode.Up),
                    (byte)(GraphNode.Left | GraphNode.Up),
                },
                new() {
                    (byte)(GraphNode.Right | GraphNode.Down),
                    (byte)(GraphNode.Left | GraphNode.Down),
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
            new List<List<byte>> {
                new() {
                    GraphNode.Right,
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
            new List<List<byte>> {
                new() {
                    GraphNode.Left,
                    0,
                },
                new() {
                    0,
                    GraphNode.Right,
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

    void Test(string[] strings, List<List<byte>> expectedNodesGraph) {
        var graph = FromStrings(strings);
        var actual = Graph.Tests.GetNodes(graph);
        Assert.AreEqual(expectedNodesGraph, actual);
    }
}
}
