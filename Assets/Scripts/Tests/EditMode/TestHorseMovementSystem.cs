using System.Collections.Generic;
using BFG.Runtime;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class TestHorseMovementSystem {
    [Test]
    [Timeout(1)]
    public void TestFindPath() {
        var s = new HorseMovementSystem();
        var graph = new MovementGraphCell[,] {
            {
                new(true, false, false, false),
                new(false, false, false, false)
            }, {
                new(false, true, false, false),
                new(false, false, false, false)
            }
        };
        var r = s.FindPath(Vector2Int.zero, Vector2Int.one, ref graph);
        Assert.IsTrue(r.Success);
        Assert.AreEqual(
            r.Path,
            new List<Vector2Int> { new(0, 0), new(0, 1), new(1, 1) }
        );
    }

    [Test]
    [Timeout(1)]
    public void TestFindPath_ShouldFail() {
    }
}
