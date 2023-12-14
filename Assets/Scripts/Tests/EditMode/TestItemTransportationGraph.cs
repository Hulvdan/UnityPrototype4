using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
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
        expectedGraph.Mark(0, 0, Direction.Right);
        expectedGraph.Mark(1, 0, Direction.Left);
        expectedGraph.Mark(1, 0, Direction.Up);
        expectedGraph.Mark(1, 1, Direction.Down);
        expectedGraph.FinishBuilding();

        Test(
            ParseAsElementTiles(
                ".B",
                "Cr"
            ),
            1,
            new() {
                new(
                    new() {
                        new(new(0, 0)),
                        new(new(1, 1)),
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
    public void Test_RoadPlaced_1() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            ".F",
            "Cr"
        );

        Assert.IsTrue(elementTiles.elementTiles[1][0].type == ElementTileType.None);
        elementTiles.elementTiles[1][0] = ElementTile.ROAD;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.RoadPlaced, new(0, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(1, result.addedSegments.Count);
    }

    [Test]
    public void Test_RoadPlaced_2() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            "Cr"
        );

        Assert.IsTrue(elementTiles.elementTiles[1][0].type == ElementTileType.None);
        elementTiles.elementTiles[1][0] = ElementTile.ROAD;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.RoadPlaced, new(0, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(1, result.addedSegments.Count);
    }

    [Test]
    public void Test_RoadPlaced_3() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            "CF"
        );

        Assert.IsTrue(elementTiles.elementTiles[1][0].type == ElementTileType.None);
        elementTiles.elementTiles[1][0] = ElementTile.ROAD;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.RoadPlaced, new(0, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(1, result.addedSegments.Count);
    }

    [Test]
    public void Test_RoadPlaced_4() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".rr",
            "Crr"
        );

        var mapSizeMock = MockMapSize_FromElementTiles(elementTiles.elementTiles);
        {
            elementTiles.elementTiles[0][1] = ElementTile.FLAG;
            var result = ItemTransportationGraph.OnTilesUpdated(
                elementTiles.elementTiles,
                mapSizeMock,
                elementTiles.buildings,
                new() { new(TileUpdatedType.FlagPlaced, new(1, 0)) },
                segments
            );
            Assert.AreEqual(0, result.deletedSegments.Count);
            Assert.AreEqual(1, result.addedSegments.Count);
            UpdateSegments(result, segments);
        }

        {
            elementTiles.elementTiles[1][2] = ElementTile.FLAG;
            var result = ItemTransportationGraph.OnTilesUpdated(
                elementTiles.elementTiles,
                mapSizeMock,
                elementTiles.buildings,
                new() { new(TileUpdatedType.FlagPlaced, new(2, 1)) },
                segments
            );
            Assert.AreEqual(0, result.deletedSegments.Count);
            Assert.AreEqual(2, result.addedSegments.Count);
            UpdateSegments(result, segments);
        }

        {
            elementTiles.elementTiles[1][0] = ElementTile.ROAD;
            var result = ItemTransportationGraph.OnTilesUpdated(
                elementTiles.elementTiles,
                mapSizeMock,
                elementTiles.buildings,
                new() { new(TileUpdatedType.RoadPlaced, new(0, 1)) },
                segments
            );
            Assert.AreEqual(1, result.deletedSegments.Count);
            Assert.AreEqual(1, result.addedSegments.Count);
            UpdateSegments(result, segments);
        }
    }

    void UpdateSegments(
        ItemTransportationGraph.OnTilesUpdatedResult result,
        List<GraphSegment> segments
    ) {
        foreach (var segment in result.deletedSegments) {
            segments.RemoveAt(segments.FindIndex(i => i.id == segment.id));
        }

        foreach (var segment in result.addedSegments) {
            segments.Add(segment);
        }
    }

    [Test]
    public void Test_FlagPlaced_1() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            ".r",
            "Cr"
        );

        elementTiles.elementTiles[1][1] = ElementTile.FLAG;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.FlagPlaced, new(1, 1)) },
            segments
        );
        Assert.AreEqual(1, result.deletedSegments.Count);
        Assert.AreEqual(2, result.addedSegments.Count);
    }

    [Test]
    public void Test_FlagPlaced_2() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            ".r",
            "CF"
        );

        elementTiles.elementTiles[1][1] = ElementTile.FLAG;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.FlagPlaced, new(1, 1)) },
            segments
        );
        Assert.AreEqual(1, result.deletedSegments.Count);
        Assert.AreEqual(2, result.addedSegments.Count);
    }

    [Test]
    public void Test_FlagPlaced_3() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            "CF"
        );

        elementTiles.elementTiles[1][0] = ElementTile.FLAG;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.FlagPlaced, new(0, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(2, result.addedSegments.Count);
    }

    [Test]
    public void Test_BuildingPlaced_1() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            "CF"
        );

        var building = MakeBuilding(BuildingType.Produce, new(0, 1));
        elementTiles.buildings.Add(building);
        elementTiles.elementTiles[1][0] = new(ElementTileType.Building, building);

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.BuildingPlaced, new(0, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(0, result.addedSegments.Count);
    }

    [Test]
    public void Test_BuildingPlaced_2() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            "..",
            "CF"
        );

        var building = MakeBuilding(BuildingType.Produce, new(1, 1));
        elementTiles.buildings.Add(building);
        elementTiles.elementTiles[1][1] = new(ElementTileType.Building, building);

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.BuildingPlaced, new(1, 1)) },
            segments
        );
        Assert.AreEqual(0, result.deletedSegments.Count);
        Assert.AreEqual(1, result.addedSegments.Count);
    }

    [Test]
    public void Test_BuildingRemoved_1() {
        var (elementTiles, segments) = MakeTilesWithInitialCalculation(
            ".B",
            "CF"
        );

        elementTiles.buildings.RemoveAt(1);
        elementTiles.elementTiles[1][1] = ElementTile.NONE;

        var result = ItemTransportationGraph.OnTilesUpdated(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings,
            new() { new(TileUpdatedType.BuildingRemoved, new(1, 1)) },
            segments
        );
        Assert.AreEqual(1, result.deletedSegments.Count);
        Assert.AreEqual(0, result.addedSegments.Count);
    }

    struct ParsedElementTiles {
        public List<Building> buildings;
        public List<List<ElementTile>> elementTiles;
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
                        tile = ElementTile.ROAD;
                        break;
                    case 'F':
                        tile = ElementTile.FLAG;
                        break;
                    case '.':
                        tile = ElementTile.NONE;
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
            buildings = buildings,
            elementTiles = tiles,
        };
    }

    Tuple<ParsedElementTiles, List<GraphSegment>> MakeTilesWithInitialCalculation(
        params string[] strings
    ) {
        var elementTiles = ParseAsElementTiles(strings);

        var segments = ItemTransportationGraph.BuildGraphSegments(
            elementTiles.elementTiles,
            MockMapSize_FromElementTiles(elementTiles.elementTiles),
            elementTiles.buildings
        );

        return new(elementTiles, segments);
    }

    void Test(
        ParsedElementTiles data,
        int expectedSegmentsCount,
        List<GraphSegment> expectedGraphSegments = null
    ) {
        var result = ItemTransportationGraph.BuildGraphSegments(
            data.elementTiles,
            MockMapSize_FromElementTiles(data.elementTiles),
            data.buildings
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
