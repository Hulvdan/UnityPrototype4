using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode {
public class TestItemTransportationGraph {
    MockMapSize MockMapSize_FromElementTiles(List<List<ElementTile>> graph) {
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
            ParseAsElementTiles(
                ".B",
                "Cr"
            ),
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
            ParseAsElementTiles(
                "..",
                "Cr"
            ),
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2AdjacentBuildings_0Segments() {
        Test(
            ParseAsElementTiles(
                "..",
                "CB"
            ),
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_4AdjacentBuildings_0Segments() {
        Test(
            ParseAsElementTiles(
                "BB",
                "CB"
            ),
            0
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_2Segments() {
        Test(
            ParseAsElementTiles(
                ".B",
                "CF"
            ),
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_1Road_3Segments() {
        Test(
            ParseAsElementTiles(
                "FB",
                "Cr"
            ),
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_3Segments() {
        Test(
            ParseAsElementTiles(
                "FF",
                "CB"
            ),
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_4Segments() {
        Test(
            ParseAsElementTiles(
                "FB",
                "CF"
            ),
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrrFrB() {
        Test(ParseAsElementTiles("CrrFrB"), 2);
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrrrrB() {
        Test(ParseAsElementTiles("CrrrrB"), 1);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex3() {
        Test(
            ParseAsElementTiles(
                "rrrrrr",
                "CrrrrB"
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex4() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rrr.",
                "BrrCrB",
                "...r..",
                "...B.."
            ),
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex5() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rFr.",
                "BrrCrB",
                "...r..",
                "...B.."
            ),
            5
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex6() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rrr.",
                "CrrFrB"
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex7() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rrr.",
                "CrrFrB",
                "...r..",
                "...B.."
            ),
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex8() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "...r..",
                "BrrCrB",
                "...r..",
                "...B.."
            ),
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex9() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rrr.",
                "BrrCrB",
                "..rrr.",
                "...B.."
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex10() {
        Test(
            ParseAsElementTiles(
                "...B..",
                "..rrr.",
                "BrrCrB",
                "..rr..",
                "...B.."
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex11() {
        Test(
            ParseAsElementTiles(
                "...B...",
                "..rrrr.",
                "CrrSSrB",
                "....rr."
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex12() {
        Test(
            ParseAsElementTiles(
                "...B...",
                "..rFrr.",
                "CrrSSrB",
                "....rr."
            ),
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex13() {
        Test(
            ParseAsElementTiles(
                "...B...",
                "..rFFr.",
                "CrrSSrB",
                "....rr."
            ),
            6
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex14() {
        Test(
            ParseAsElementTiles(
                "...B...",
                "..rrFr.",
                "CrrSSrB",
                "....rr."
            ),
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex15() {
        Test(
            ParseAsElementTiles(
                "CrF",
                ".rB"
            ),
            2
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex16() {
        Test(
            ParseAsElementTiles(
                "CrFr",
                ".rSS"
            ),
            3
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex17() {
        Test(
            ParseAsElementTiles(
                "Crrr",
                ".rSS"
            ),
            1
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex18() {
        Test(
            ParseAsElementTiles(
                ".B.",
                "CFB",
                ".B."
            ),
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex19() {
        Test(
            ParseAsElementTiles(
                "..B..",
                "..r..",
                "CrFrB",
                "..r..",
                "..B.."
            ),
            4
        );
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CrB() {
        Test(ParseAsElementTiles("CrB"), 1);
    }

    [Test]
    [Timeout(1)]
    public void Test_Line_CFB() {
        Test(ParseAsElementTiles("CFB"), 2);
    }

    [Test]
    public void Test_PlacingRoad_1() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            ".F",
            "Cr"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        Assert.IsTrue(elementTiles.ElementTiles[1][0].Type == ElementTileType.None);
        elementTiles.ElementTiles[1][0] = ElementTile.Road;

        var result = ItemTransportationGraph.OnRoadPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(0, 1),
            segments
        );
        Assert.AreEqual(0, result.DeletedSegments.Count);
        Assert.AreEqual(1, result.AddedSegments.Count);
    }

    [Test]
    public void Test_PlacingRoad_2() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            "Cr"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        Assert.IsTrue(elementTiles.ElementTiles[1][0].Type == ElementTileType.None);
        elementTiles.ElementTiles[1][0] = ElementTile.Road;

        var result = ItemTransportationGraph.OnRoadPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(0, 1),
            segments
        );
        Assert.AreEqual(0, result.DeletedSegments.Count);
        Assert.AreEqual(1, result.AddedSegments.Count);
    }

    [Test]
    public void Test_PlacingRoad_3() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            "CF"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        Assert.IsTrue(elementTiles.ElementTiles[1][0].Type == ElementTileType.None);
        elementTiles.ElementTiles[1][0] = ElementTile.Road;

        var result = ItemTransportationGraph.OnRoadPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(0, 1),
            segments
        );
        Assert.AreEqual(0, result.DeletedSegments.Count);
        Assert.AreEqual(1, result.AddedSegments.Count);
    }

    [Test]
    public void Test_PlacingFlag_1() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            ".r",
            "Cr"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        elementTiles.ElementTiles[1][1] = ElementTile.Flag;

        var result = ItemTransportationGraph.OnFlagPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(1, 1),
            segments
        );
        Assert.AreEqual(1, result.DeletedSegments.Count);
        Assert.AreEqual(2, result.AddedSegments.Count);
    }

    [Test]
    public void Test_PlacingFlag_2() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            ".r",
            "CF"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        elementTiles.ElementTiles[1][1] = ElementTile.Flag;

        var result = ItemTransportationGraph.OnFlagPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(1, 1),
            segments
        );
        Assert.AreEqual(1, result.DeletedSegments.Count);
        Assert.AreEqual(2, result.AddedSegments.Count);
    }

    [Test]
    public void Test_PlacingFlag_3() {
        var elementTiles = ParseAsElementTiles(
            ".B",
            "CF"
        );

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings
        );

        elementTiles.ElementTiles[1][0] = ElementTile.Flag;

        var result = ItemTransportationGraph.OnFlagPlaced(
            elementTiles.ElementTiles,
            MockMapSize_FromElementTiles(elementTiles.ElementTiles),
            elementTiles.Buildings,
            new(0, 1),
            segments
        );
        Assert.AreEqual(0, result.DeletedSegments.Count);
        Assert.AreEqual(2, result.AddedSegments.Count);
    }

    struct ParsedElementTiles {
        public List<Building> Buildings;
        public List<List<ElementTile>> ElementTiles;
    }

    ParsedElementTiles ParseAsElementTiles(params string[] strings) {
        strings = strings.Reverse().ToArray();
        var height = strings.Length;
        var width = strings[0].Length;

        foreach (var str in strings) {
            Assert.AreEqual(str.Length, width);
        }

        var buildings = new List<Building>();

        Building buildingSawmill = null;

        var tiles = new List<List<ElementTile>>();
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

            tiles.Add(row);
        }

        return new() {
            Buildings = buildings,
            ElementTiles = tiles,
        };
    }

    void Test(
        ParsedElementTiles data,
        int expectedSegmentsCount,
        List<GraphSegment> expectedGraphSegments = null
    ) {
        var result = ItemTransportationGraph.BuildGraphSegments(
            data.ElementTiles,
            MockMapSize_FromElementTiles(data.ElementTiles),
            data.Buildings
        );

        Assert.AreEqual(expectedSegmentsCount, result.Count);

        if (expectedGraphSegments == null) {
            return;
        }

        expectedGraphSegments.Sort();
        result.Sort();

        Assert.IsTrue(expectedGraphSegments.SequenceEqual(result));
    }
}
}
