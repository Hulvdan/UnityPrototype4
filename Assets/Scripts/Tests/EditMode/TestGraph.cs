using System.Collections.Generic;
using System.Linq;
using BFG.Core;
using BFG.Graphs;
using NUnit.Framework;
using UnityEngine;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace Tests.EditMode {
public class TestGraph {
    // ╶╵╴╷┼
    // ┌┐└┘─│
    // ├ ┬ ┴ ┤

    [Test]
    [Timeout(1)]
    public void Test_1() {
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

    Graph FromStrings(params string[] strings) {
        var graph = new Graph();

        var height = strings.Length;
        if (height == 0) {
            return graph;
        }

        var width = strings[0].Length;
        foreach (var str in strings) {
            Assert.AreEqual(width, str.Length);
        }

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

    [Test]
    public void Test_GetCenters_Empty1() {
        var strings = new string[] { };
        var graph = FromStrings(strings);

        Assert.Throws<AssertionException>(() => graph.GetCenters());
    }

    [Test]
    public void Test_GetCenters_Empty2() {
        var graph = FromStrings(".");

        Assert.Throws<AssertionException>(() => graph.GetCenters());
    }

    [Test]
    public void Test_GetCenters_2() {
        var graph = FromStrings("╶╴");

        var expected = new List<Vector2Int> { new(0, 0), new(1, 0) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparison);
        centers.Sort(Utils.StupidVector2IntComparison);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_2_Rotated() {
        var graph = FromStrings(
            "╷",
            "╵"
        );

        var expected = new List<Vector2Int> { new(0, 0), new(0, 1) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparison);
        centers.Sort(Utils.StupidVector2IntComparison);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_3() {
        var graph = FromStrings("╶─╴");

        var expected = new List<Vector2Int> { new(1, 0) };
        var centers = graph.GetCenters();

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_4_1() {
        var graph = FromStrings("╶──╴");

        var expected = new List<Vector2Int> { new(1, 0), new(2, 0) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparison);
        centers.Sort(Utils.StupidVector2IntComparison);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_4_2() {
        var graph = FromStrings(
            ".╷.",
            "╶┴╴"
        );

        var expected = new List<Vector2Int> { new(1, 0) };
        var centers = graph.GetCenters();

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_40() {
        var graph = FromStrings(
            ".╷..",
            ".├┬╴",
            "╶┴┤.",
            "..╵."
        );

        var expected = new List<Vector2Int> {
            new(1, 1),
            new(2, 1),
            new(1, 2),
            new(2, 2),
        };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparison);
        centers.Sort(Utils.StupidVector2IntComparison);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_WithOffset() {
        var graph = new Graph();
        graph.SetDirection(10, 10, Direction.Right);
        graph.SetDirection(11, 10, Direction.Right);
        graph.SetDirection(11, 10, Direction.Left);
        graph.SetDirection(12, 10, Direction.Left);

        var centers = graph.GetCenters();
        var expected = new List<Vector2Int> { new(11, 10) };

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_IsUndirected_Empty_1() {
        var graph = FromStrings();
        Assert.Throws<AssertionException>(() => graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_Empty_2() {
        var graph = FromStrings("");
        Assert.Throws<AssertionException>(() => graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_1() {
        var graph = new Graph();
        graph.SetDirection(0, 0, Direction.Right, false);

        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2() {
        var graph = FromStrings("╶╴");
        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2_Oriented() {
        var graph = FromStrings("╴╶");
        Assert.IsFalse(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2_Rotated() {
        var graph = FromStrings(
            "╷",
            "╵"
        );
        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_10() {
        var graph = FromStrings(
            ".╷..",
            ".├┬╴",
            "╶┴┤.",
            "..╵."
        );

        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_GetShortestPath_1() {
        var graph = FromStrings("╶╴");

        var actual = graph.GetShortestPath(new(0, 0), new(1, 0));
        var expected = new List<Vector2Int> { new(0, 0), new(1, 0) };
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Test_GetShortestPath_2() {
        var graph = FromStrings(
            ".╷..",
            ".├┬╴",
            "╶┴┤.",
            "..╵."
        );

        var actual = graph.GetShortestPath(new(0, 1), new(3, 2));
        var expected1 = new List<Vector2Int> {
            new(0, 1), new(1, 1), new(2, 1), new(2, 2), new(3, 2),
        };
        var expected2 = new List<Vector2Int> {
            new(0, 1), new(1, 1), new(1, 2), new(2, 2), new(3, 2),
        };
        Assert.IsTrue(
            expected1.SequenceEqual(actual)
            || expected2.SequenceEqual(actual)
        );
    }

    [Test]
    public void Test_GetShortestPath_3() {
        var graph = FromStrings(
            ".╷..",
            ".├┬╴",
            "╶┴┤.",
            "..╵."
        );

        var actual = graph.GetShortestPath(new(0, 1), new(1, 3));
        var expected = new List<Vector2Int> {
            new(0, 1), new(1, 1), new(1, 2), new(1, 3),
        };

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void Test_GetShortestPath_WithOffset() {
        var graph = new Graph();
        graph.SetDirection(10, 10, Direction.Right);
        graph.SetDirection(11, 10, Direction.Right);
        graph.SetDirection(11, 10, Direction.Left);
        graph.SetDirection(12, 10, Direction.Left);

        var actual = graph.GetShortestPath(new(10, 10), new(12, 10));
        var expected = new List<Vector2Int> {
            new(10, 10), new(11, 10), new(12, 10),
        };

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void Test_GetShortestPath_WithOffset_2() {
        var graph = new Graph();
        graph.SetDirection(8, 7, Direction.Down);
        graph.SetDirection(8, 6, Direction.Up);
        graph.SetDirection(8, 6, Direction.Right);
        graph.SetDirection(9, 6, Direction.Left);
        graph.SetDirection(9, 6, Direction.Right);
        graph.SetDirection(10, 6, Direction.Left);
        graph.SetDirection(10, 6, Direction.Right);
        graph.SetDirection(11, 6, Direction.Left);
        graph.SetDirection(13, 13, Direction.Right, false);

        var actual = graph.GetShortestPath(new(8, 7), new(11, 6));
        var expected = new List<Vector2Int> {
            new(8, 7), new(8, 6), new(9, 6), new(10, 6), new(11, 6),
        };

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void Test_GetShortestPath_WithOffset_3() {
        var graph = new Graph();
        graph.SetDirection(8, 7, Direction.Down);
        graph.SetDirection(8, 6, Direction.Up);
        graph.SetDirection(8, 6, Direction.Right);
        graph.SetDirection(9, 6, Direction.Left);
        graph.SetDirection(9, 6, Direction.Right);
        graph.SetDirection(10, 6, Direction.Left);
        graph.SetDirection(10, 6, Direction.Right);
        graph.SetDirection(11, 6, Direction.Left);

        var actual = graph.GetShortestPath(new(8, 7), new(11, 6));
        var expected = new List<Vector2Int> {
            new(8, 7), new(8, 6), new(9, 6), new(10, 6), new(11, 6),
        };

        Assert.That(actual, Is.EquivalentTo(expected));
    }

    [Test]
    public void Test_ContainsNode() {
        var graph = new Graph();
        graph.SetDirection(7, 10, Direction.Right);

        Assert.IsTrue(graph.ContainsNode(7, 10));
        Assert.IsFalse(graph.ContainsNode(8, 10));
        Assert.IsFalse(graph.ContainsNode(6, 10));
        Assert.IsFalse(graph.ContainsNode(7, 11));
        Assert.IsFalse(graph.ContainsNode(7, 9));
    }
}
}
