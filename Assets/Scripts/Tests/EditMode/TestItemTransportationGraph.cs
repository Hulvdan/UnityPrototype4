using System;
using System.Collections.Generic;
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
    public void Test_2Buildings_1Road_With_1Segment() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
            },
            new() {
                ElementTile.None,
                new(ElementTileType.Building, anotherBuilding),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall, anotherBuilding }
        );

        Assert.AreEqual(1, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_1Building_1Road_0Segments() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall }
        );

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_AdjacentBuildings_1Segment() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                new(ElementTileType.Building, anotherBuilding),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall }
        );

        Assert.AreEqual(1, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_2Segments() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Flag,
            },
            new() {
                ElementTile.None,
                new(ElementTileType.Building, anotherBuilding),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall }
        );

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_1Road_1Segment() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
            },
            new() {
                ElementTile.Flag,
                new(ElementTileType.Building, anotherBuilding),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall }
        );

        Assert.AreEqual(3, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_4Segments() {
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Flag,
            },
            new() {
                ElementTile.Flag,
                new(ElementTileType.Building, anotherBuilding),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph,
            MockMapSize_FromGraph(graph),
            new() { cityHall }
        );

        Assert.AreEqual(4, result.Count);
    }
}
}
