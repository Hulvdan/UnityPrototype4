using System.Collections.Generic;
using System.Reactive.Subjects;
using UnityEngine;

namespace BFG.Runtime {
public interface IMap {
    List<Building> buildings { get; }
    List<List<ElementTile>> elementTiles { get; }
    List<List<TerrainTile>> terrainTiles { get; }

    Subject<Vector2Int> onElementTileChanged { get; }

    Subject<E_HumanCreated> onHumanCreated { get; }
    Subject<E_HumanStateChanged> onHumanStateChanged { get; }
    Subject<E_HumanPickedUpResource> onHumanPickedUpResource { get; }
    Subject<E_HumanPlacedResource> onHumanPlacedResource { get; }

    Subject<E_TrainCreated> onTrainCreated { get; }
    Subject<E_TrainNodeCreated> onTrainNodeCreated { get; }
    Subject<E_TrainPickedUpResource> onTrainPickedUpResource { get; }
    Subject<E_TrainPushedResource> onTrainPushedResource { get; }

    Subject<E_BuildingStartedProcessing> onBuildingStartedProcessing { get; }
    Subject<E_BuildingProducedItem> onBuildingProducedItem { get; }

    Subject<E_TopBarResourceChanged> onResourceChanged { get; }

    void TryBuild(Vector2Int hoveredTile, SelectedItem gameManagerSelectedItem);

    bool AreThereAvailableResourcesForTheTrain(HorseTrain horse);
    void PickRandomItemForTheTrain(HorseTrain horse);
    bool AreThereAvailableSlotsTheTrainCanPassResourcesTo(HorseTrain horse);
    void PickRandomSlotForTheTrainToPassItemTo(HorseTrain horse);
    bool IsBuildable(int x, int y);
    bool IsBuildable(Vector2Int pos);
}
}
