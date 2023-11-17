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
        return new(Guid.NewGuid(), ScriptableBuilding.Tests.Build(type), pos);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Road_1Segment() {
        Test(
            new[] {
                ".B",
                "Cr",
            },
            1
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

    void Test(string[] strings, int expectedSegmentsCount) {
        strings = strings.Reverse().ToArray();
        var height = strings.Length;
        var width = strings[0].Length;

        var buildings = new List<Building>();

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
    }
}
}