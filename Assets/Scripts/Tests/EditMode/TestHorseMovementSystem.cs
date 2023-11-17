using System.Collections.Generic;
using BFG.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode {
public class TestHorseMovementSystem {
    MockMapSize MockMapSize_FromGraph(List<List<MovementGraphTile>> graph) {
        return new(graph[0].Count, graph.Count);
    }

    [Test]
    [Timeout(1)]
    public void TestFindPath() {
        var system = new HorseMovementSystem();
        var graph = new List<List<MovementGraphTile>> {
            new() {
                new(false, true, false, false),
                new(false, false, false, false),
            },
            new() {
                new(true, false, false, false),
                new(false, false, false, false),
            },
        };
        system.Init(MockMapSize_FromGraph(graph), graph);

        var result = system.FindPath(Vector2Int.zero, Vector2Int.one, Direction.Up);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(
            result.Path,
            new List<Vector2Int> { new(0, 0), new(0, 1), new(1, 1) }
        );
    }

    [Test]
    [Timeout(1)]
    public void TestFindPath_Bigger() {
        var system = new HorseMovementSystem();
        var graph = new List<List<MovementGraphTile>> {
            new() {
                MovementGraphTile.MakeUpRight(),
                MovementGraphTile.MakeLeftRight(),
                MovementGraphTile.MakeUpLeft(),
                new(false, false, false, false),
            },
            new() {
                MovementGraphTile.MakeUpDown(),
                new(false, false, false, false),
                MovementGraphTile.MakeUpDownRight(),
                MovementGraphTile.MakeLeftRight(),
            },
            new() {
                MovementGraphTile.MakeDownRight(),
                MovementGraphTile.MakeLeftRight(),
                MovementGraphTile.MakeDownLeft(),
                new(false, false, false, false),
            },
        };

        system.Init(MockMapSize_FromGraph(graph), graph);

        var result = system.FindPath(new(2, 0), new(3, 1), Direction.Left);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(
            new List<Vector2Int> {
                new(2, 0),
                new(1, 0),
                new(0, 0),
                new(0, 1),
                new(0, 2),
                new(1, 2),
                new(2, 2),
                new(2, 1),
                new(3, 1),
            },
            result.Path
        );
    }

    // [Test]
    // [Timeout(1)]
    // public void TestFindPath_BiggerEdgeCase() {
    //     var system = new HorseMovementSystem();
    //     var graph = new[,] {
    //         {
    //             MovementGraphCell.MakeUpRight(),
    //             MovementGraphCell.MakeLeftRight(),
    //             MovementGraphCell.MakeUpLeft(),
    //             new(false, false, false, false)
    //         }, {
    //             MovementGraphCell.MakeUpDown(),
    //             new(false, false, false, false),
    //             MovementGraphCell.MakeUpDownRight(),
    //             MovementGraphCell.MakeLeftRight()
    //         }, {
    //             MovementGraphCell.MakeDownRight(),
    //             MovementGraphCell.MakeLeftRight(),
    //             MovementGraphCell.MakeDownLeft(),
    //             new(false, false, false, false)
    //         }
    //     };
    //     var result = system.FindPath(
    //         new Vector2Int(2, 1),
    //         new Vector2Int(3, 1),
    //         ref graph,
    //         Direction.Down
    //     );
    //
    //     Assert.IsTrue(result.Success);
    //     Assert.AreEqual(
    //         new List<Vector2Int> {
    //             new(2, 1),
    //             new(2, 0),
    //             new(1, 0),
    //             new(0, 0),
    //             new(0, 1),
    //             new(0, 2),
    //             new(1, 2),
    //             new(2, 2),
    //             new(2, 1),
    //             new(3, 1)
    //         },
    //         result.Path
    //     );
    // }

    [Test]
    [Timeout(1)]
    public void TestFindPath_ShouldFail() {
        var system = new HorseMovementSystem();
        var graph = new List<List<MovementGraphTile>> {
            new() {
                new(false, true, false, false),
                new(false, false, false, false),
            },
            new() {
                new(false, false, false, false),
                new(false, false, false, false),
            },
        };
        system.Init(MockMapSize_FromGraph(graph), graph);

        var result = system.FindPath(Vector2Int.zero, Vector2Int.one, Direction.Up);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(result.Path, null);
    }
}
}
