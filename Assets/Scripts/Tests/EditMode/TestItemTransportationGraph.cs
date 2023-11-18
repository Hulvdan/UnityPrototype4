using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode {
public class TestItemTransportationGraph {
    MockMapSize MockMapSize_FromGraph(List<List<ElementTile>> graph) {
        return new(graph[0].Count, graph.Count);
    }

    Building MakeBuilding(BuildingType type, Vector2Int pos) {
        return new(Guid.NewGuid(), new MockScriptableBuilding(type), pos, 1);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Road_1Segment() {
        var expectedGraph = new Graph();
        expectedGraph.SetDirection(0, 0, Direction.Right);
        expectedGraph.SetDirection(1, 0, Direction.Left);
        expectedGraph.SetDirection(1, 0, Direction.Up);
        expectedGraph.SetDirection(1, 1, Direction.Down);

        Test(
            new[] {
                ".B",
                "Cr",
            },
            1,
            new() {
                new(
                    new() {
                        new(new(), new(0, 0)),
                        new(new(), new(1, 1)),
                    },
                    new() {
                        new(0, 0),
                        new(1, 0),
                        new(1, 1),
                    },
                    expectedGraph
                ),
            }
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_1Building_1Road_0Segments() {
        Test(
            new[] {
                "..",
                "Cr",
            },
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2AdjacentBuildings_0Segments() {
        Test(
            new[] {
                "..",
                "CB",
            },
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_4AdjacentBuildings_0Segments() {
        Test(
            new[] {
                "BB",
                "CB",
            },
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_2Segments() {
        Test(
            new[] {
                ".B",
                "CF",
            },
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_1Road_3Segments() {
        Test(
            new[] {
                "FB",
                "Cr",
            },
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_3Segments() {
        Test(
            new[] {
                "FF",
                "CB",
            },
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_4Segments() {
        Test(
            new[] {
                "FB",
                "CF",
            },
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrrFrB() {
        Test(
            new[] {
                "CrrFrB",
            },
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrrrrB() {
        Test(
            new[] {
                "CrrrrB",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex3() {
        Test(
            new[] {
                "rrrrrr",
                "CrrrrB",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex4() {
        Test(
            new[] {
                "...B..",
                "..rrr.",
                "BrrCrB",
                "...r..",
                "...B..",
            },
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex5() {
        Test(
            new[] {
                "...B..",
                "..rFr.",
                "BrrCrB",
                "...r..",
                "...B..",
            },
            5
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex6() {
        Test(
            new[] {
                "...B..",
                "..rrr.",
                "CrrFrB",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex7() {
        Test(
            new[] {
                "...B..",
                "..rrr.",
                "CrrFrB",
                "...r..",
                "...B..",
            },
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex8() {
        Test(
            new[] {
                "...B..",
                "...r..",
                "BrrCrB",
                "...r..",
                "...B..",
            },
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex9() {
        Test(
            new[] {
                "...B..",
                "..rrr.",
                "BrrCrB",
                "..rrr.",
                "...B..",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex10() {
        Test(
            new[] {
                "...B..",
                "..rrr.",
                "BrrCrB",
                "..rr..",
                "...B..",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex11() {
        Test(
            new[] {
                "...B...",
                "..rrrr.",
                "CrrSSrB",
                "....rr.",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex12() {
        Test(
            new[] {
                "...B...",
                "..rFrr.",
                "CrrSSrB",
                "....rr.",
            },
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex13() {
        Test(
            new[] {
                "...B...",
                "..rFFr.",
                "CrrSSrB",
                "....rr.",
            },
            6
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex14() {
        Test(
            new[] {
                "...B...",
                "..rrFr.",
                "CrrSSrB",
                "....rr.",
            },
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex15() {
        Test(
            new[] {
                "CrF",
                ".rB",
            },
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex16() {
        Test(
            new[] {
                "CrFr",
                ".rSS",
            },
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex17() {
        Test(
            new[] {
                "Crrr",
                ".rSS",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrB() {
        Test(
            new[] {
                "CrB",
            },
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CFB() {
        Test(
            new[] {
                "CFB",
            },
            2
        );
    }

    void Test(
        string[] strings, int expectedSegmentsCount, List<GraphSegment> expectedGraphSegments = null
    ) {
        strings = strings.Reverse().ToArray();
        var height = strings.Length;
        var width = strings[0].Length;

        foreach (var str in strings) {
            Assert.AreEqual(str.Length, width);
        }

        var buildings = new List<Building>();

        Building buildingSawmill = null;

        var graph = new List<List<ElementTile>>();
        for (var y = 0; y < height; y++) {
            var row = new List<ElementTile>();
            for (var x = 0; x < width; x++) {
                ElementTile tile;
                switch (strings[y][x]) {
                    case 'C':
                        var building = MakeBuilding(BuildingType.SpecialCityHall, new(x, y));
                        buildings.Add(building);
                        tile = new(ElementTileType.Building, building);
                        break;
                    case 'B':
                        var building2 = MakeBuilding(BuildingType.Produce, new(x, y));
                        buildings.Add(building2);
                        tile = new(ElementTileType.Building, building2);
                        break;
                    case 'S':
                        if (buildingSawmill == null) {
                            buildingSawmill = MakeBuilding(BuildingType.Produce, new(x, y));
                            buildings.Add(buildingSawmill);
                        }

                        tile = new(ElementTileType.Building, buildingSawmill);
                        break;
                    case 'r':
                        tile = ElementTile.Road;
                        break;
                    case 'F':
                        tile = ElementTile.Flag;
                        break;
                    case '.':
                        tile = ElementTile.None;
                        break;
                    default:
                        Assert.IsTrue(false);
                        continue;
                }

                row.Add(tile);
            }

            graph.Add(row);
        }

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );
        Assert.AreEqual(expectedSegmentsCount, result.Count);

        // if (expectedGraphSegments == null) {
        //     return;
        // }
        //
        // expectedGraphSegments.Sort();
        // result.Sort();
        //
        // Assert.AreEqual(expectedGraphSegments, result);
    }
}
}
