using System.Collections.Generic;
using BFG.Runtime;
using NUnit.Framework;
using UnityEngine;

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

    Graph FromStrings(string[] strings) {
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
        var centers = graph.GetCenters();
        Assert.AreEqual(new(), centers);
    }

    [Test]
    public void Test_GetCenters_Empty2() {
        var graph = FromStrings(
            new[] {
                ".",
            }
        );
        var centers = graph.GetCenters();
        Assert.AreEqual(new(), centers);
    }

    [Test]
    public void Test_GetCenters_2() {
        var graph = FromStrings(
            new[] {
                "╶╴",
            }
        );

        var expected = new List<Vector2Int> { new(0, 0), new(1, 0) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparation);
        centers.Sort(Utils.StupidVector2IntComparation);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_2_Rotated() {
        var graph = FromStrings(
            new[] {
                "╷",
                "╵",
            }
        );

        var expected = new List<Vector2Int> { new(0, 0), new(0, 1) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparation);
        centers.Sort(Utils.StupidVector2IntComparation);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_3() {
        var graph = FromStrings(
            new[] {
                "╶─╴",
            }
        );

        var expected = new List<Vector2Int> { new(1, 0) };
        var centers = graph.GetCenters();

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_4_1() {
        var graph = FromStrings(
            new[] {
                "╶──╴",
            }
        );

        var expected = new List<Vector2Int> { new(1, 0), new(2, 0) };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparation);
        centers.Sort(Utils.StupidVector2IntComparation);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_4_2() {
        var graph = FromStrings(
            new[] {
                ".╷.",
                "╶┴╴",
            }
        );

        var expected = new List<Vector2Int> { new(1, 0) };
        var centers = graph.GetCenters();

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_GetCenters_40() {
        var graph = FromStrings(
            new[] {
                ".╷..",
                ".├┬╴",
                "╶┴┤.",
                "..╵.",
            }
        );

        var expected = new List<Vector2Int> {
            new(1, 1),
            new(2, 1),
            new(1, 2),
            new(2, 2),
        };
        var centers = graph.GetCenters();
        expected.Sort(Utils.StupidVector2IntComparation);
        centers.Sort(Utils.StupidVector2IntComparation);

        Assert.AreEqual(expected, centers);
    }

    [Test]
    public void Test_IsUndirected_Empty_1() {
        var graph = FromStrings(new string[] { });
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_Empty_2() {
        var graph = FromStrings(
            new[] {
                "",
            }
        );
        Assert.Throws<UnityEngine.Assertions.AssertionException>(() => graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_1() {
        var graph = new Graph();
        graph.SetDirection(0, 0, Direction.Right, false);

        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2() {
        var graph = FromStrings(
            new[] {
                "╶╴",
            }
        );
        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2_Oriented() {
        var graph = FromStrings(
            new[] {
                "╴╶",
            }
        );
        Assert.IsFalse(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_2_Rotated() {
        var graph = FromStrings(
            new[] {
                "╷",
                "╵",
            }
        );
        Assert.IsTrue(graph.IsUndirected());
    }

    [Test]
    public void Test_IsUndirected_10() {
        var graph = FromStrings(
            new[] {
                ".╷..",
                ".├┬╴",
                "╶┴┤.",
                "..╵.",
            }
        );

        Assert.IsTrue(graph.IsUndirected());
    }
}
}
