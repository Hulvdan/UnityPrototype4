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
    public void Test_2Buildings_1Road_With_1Segment_2Vertexes() {
        // .B
        // Cr
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].Vertexes.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_1Building_1Road_0Segments() {
        // ..
        // Cr
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));

        var buildings = new List<Building> {
            cityHall,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2AdjacentBuildings_0Segments() {
        // ..
        // CB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 0));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_4AdjacentBuildings_0Segments() {
        // BB
        // CB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var building10 = MakeBuilding(BuildingType.Produce, new(1, 0));
        var building01 = MakeBuilding(BuildingType.Produce, new(0, 1));
        var building11 = MakeBuilding(BuildingType.Produce, new(1, 1));

        var buildings = new List<Building> {
            cityHall,
            building10,
            building01,
            building11,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                new(ElementTileType.Building, building10),
            },
            new() {
                new(ElementTileType.Building, building01),
                new(ElementTileType.Building, building11),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(0, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_2Segments() {
        // .B
        // CF
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_1Flag_1Road_3Segments() {
        // rB
        // CF
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(3, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_3Segments() {
        // FF
        // CB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 0));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                new(ElementTileType.Building, anotherBuilding),
            },
            new() {
                ElementTile.Flag,
                ElementTile.Flag,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(3, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_2Buildings_2Flags_4Segments() {
        // FB
        // CF
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var anotherBuilding = MakeBuilding(BuildingType.Produce, new(1, 1));

        var buildings = new List<Building> {
            cityHall,
            anotherBuilding,
        };

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
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(4, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex1() {
        // CrrFrB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var building50 = MakeBuilding(BuildingType.Produce, new(5, 0));

        var buildings = new List<Building> {
            cityHall,
            building50,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Flag,
                ElementTile.Road,
                new(ElementTileType.Building, building50),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex2() {
        // CrrrrB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var building50 = MakeBuilding(BuildingType.Produce, new(5, 0));

        var buildings = new List<Building> {
            cityHall,
            building50,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                new(ElementTileType.Building, building50),
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].Vertexes.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex3() {
        // rrrrrr
        // CrrrrB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var building50 = MakeBuilding(BuildingType.Produce, new(5, 0));

        var buildings = new List<Building> {
            cityHall,
            building50,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                new(ElementTileType.Building, building50),
            },
            new() {
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result[0].Vertexes.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex4() {
        // ...B..
        // ..rrr.
        // BrrCrB
        // ...r..
        // ...B..
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 2));
        var building02 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building52 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building34 = MakeBuilding(BuildingType.Produce, new(3, 4));
        var building30 = MakeBuilding(BuildingType.Produce, new(3, 0));

        var buildings = new List<Building> {
            cityHall,
            building02,
            building52,
            building34,
            building30,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building30),
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                new(ElementTileType.Building, building02),
                ElementTile.Road,
                ElementTile.Road,
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                new(ElementTileType.Building, building52),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building34),
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex5() {
        // ...B..
        // ..rFr.
        // BrrCrB
        // ...r..
        // ...B..
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 2));
        var building02 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building52 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building34 = MakeBuilding(BuildingType.Produce, new(3, 4));
        var building30 = MakeBuilding(BuildingType.Produce, new(3, 0));

        var buildings = new List<Building> {
            cityHall,
            building02,
            building52,
            building34,
            building30,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building30),
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                new(ElementTileType.Building, building02),
                ElementTile.Road,
                ElementTile.Road,
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                new(ElementTileType.Building, building52),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.Flag,
                ElementTile.Road,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building34),
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(5, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex6() {
        // ...B..
        // ..rrr.
        // CrrFrB
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 0));
        var building50 = MakeBuilding(BuildingType.Produce, new(5, 0));
        var building32 = MakeBuilding(BuildingType.Produce, new(3, 2));

        var buildings = new List<Building> {
            cityHall,
            building50,
            building32,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Flag,
                ElementTile.Road,
                new(ElementTileType.Building, building50),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building32),
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(4, result[0].Vertexes.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex7() {
        // ...B..
        // ..rrr.
        // CrrFrB
        // ...r..
        // ...B..
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 2));
        var building52 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building34 = MakeBuilding(BuildingType.Produce, new(3, 4));
        var building30 = MakeBuilding(BuildingType.Produce, new(3, 0));

        var buildings = new List<Building> {
            cityHall,
            building52,
            building34,
            building30,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building30),
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Flag,
                ElementTile.Road,
                new(ElementTileType.Building, building52),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.Road,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building34),
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(2, result.Count);
    }

    [Test]
    [Timeout(1)]
    public void Test_Complex8() {
        // ...B..
        // ...r..
        // BrrCrB
        // ...r..
        // ...B..
        var cityHall = MakeBuilding(BuildingType.SpecialCityHall, new(0, 2));
        var building02 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building52 = MakeBuilding(BuildingType.Produce, new(5, 2));
        var building34 = MakeBuilding(BuildingType.Produce, new(3, 4));
        var building30 = MakeBuilding(BuildingType.Produce, new(3, 0));

        var buildings = new List<Building> {
            cityHall,
            building02,
            building52,
            building34,
            building30,
        };

        var graph = new List<List<ElementTile>> {
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building30),
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                new(ElementTileType.Building, building02),
                ElementTile.Road,
                ElementTile.Road,
                new(ElementTileType.Building, cityHall),
                ElementTile.Road,
                new(ElementTileType.Building, building52),
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                ElementTile.Road,
                ElementTile.None,
                ElementTile.None,
            },
            new() {
                ElementTile.None,
                ElementTile.None,
                ElementTile.None,
                new(ElementTileType.Building, building34),
                ElementTile.None,
                ElementTile.None,
            },
        };

        var result = ItemTransportationGraph.BuildGraphSegments(
            graph, MockMapSize_FromGraph(graph), buildings
        );

        Assert.AreEqual(4, result.Count);
    }
}
}
