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

    Subject<E_HumanTransporterCreated> onHumanTransporterCreated { get; }
    Subject<E_CityHallCreatedHuman> onCityHallCreatedHuman { get; }
    Subject<E_HumanReachedCityHall> onHumanReachedCityHall { get; }

    Subject<E_HumanTransporterMovedToTheNextTile> onHumanTransporterMovedToTheNextTile { get; }

    Subject<E_HumanTransportedStartedPickingUpResource> onHumanTransporterStartedPickingUpResource {
        get;
    }

    Subject<E_HumanTransporterPickedUpResource> onHumanTransporterPickedUpResource { get; }

    Subject<E_HumanTransporterStartedPlacingResource> onHumanTransporterStartedPlacingResource {
        get;
    }

    Subject<E_HumanTransporterPlacedResource> onHumanTransporterPlacedResource { get; }

    Subject<E_BuildingPlaced> onBuildingPlaced { get; }

    Subject<E_BuildingStartedProcessing> onBuildingStartedProcessing { get; }
    Subject<E_BuildingProducedItem> onBuildingProducedItem { get; }

    void TryBuild(Vector2Int pos, SelectedItem item);
    bool CanBePlaced(Vector2Int pos, SelectedItemType itemType);
    bool IsBuildable(int x, int y);
    bool IsBuildable(Vector2Int pos);

    PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        bool avoidHarvestableResources
    );
}
}
