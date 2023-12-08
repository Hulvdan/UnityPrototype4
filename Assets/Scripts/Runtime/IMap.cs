using System.Collections.Generic;
using System.Reactive.Subjects;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using UnityEngine;

namespace BFG.Runtime {
public interface IMap {
    List<List<ElementTile>> elementTiles { get; }
    List<List<TerrainTile>> terrainTiles { get; }

    Subject<Vector2Int> onElementTileChanged { get; }

    List<Building> buildings { get; }
    List<GraphSegment> segments { get; }
    List<List<List<MapResource>>> mapResources { get; }

    Subject<E_HumanCreated> onHumanCreated { get; }
    Subject<E_CityHallCreatedHuman> onCityHallCreatedHuman { get; }
    Subject<E_HumanReachedCityHall> onHumanReachedCityHall { get; }

    Subject<E_HumanMovedToTheNextTile> onHumanMovedToTheNextTile { get; }

    Subject<E_HumanStartedPickingUpResource> onHumanStartedPickingUpResource { get; }
    Subject<E_HumanPickedUpResource> onHumanPickedUpResource { get; }
    Subject<E_HumanStartedPlacingResource> onHumanStartedPlacingResource { get; }
    Subject<E_HumanPlacedResource> onHumanPlacedResource { get; }

    Subject<E_BuildingPlaced> onBuildingPlaced { get; }

    Subject<E_HumanStartedConstructingBuilding> OnHumanStartedConstructingBuilding { get; }
    Subject<E_HumanConstructedBuilding> OnHumanConstructedBuilding { get; }

    void TryBuild(Vector2Int pos, ItemToBuild item);
    bool CanBePlaced(Vector2Int pos, ItemToBuildType itemType);
    bool IsBuildable(int x, int y);
    bool IsBuildable(Vector2Int pos);

    PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        bool avoidHarvestableResources
    );

    void OnResourcePlacedInsideBuilding(MapResource res, Building building);
    void OnBuildingConstructed(Building building, Human constructor);
}
}
