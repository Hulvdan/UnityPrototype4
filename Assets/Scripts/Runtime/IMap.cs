using System.Collections.Generic;
using System.Reactive.Subjects;
using UnityEngine;

namespace BFG.Runtime {
public interface IMap {
    List<Building> buildings { get; }
    List<List<ElementTile>> elementTiles { get; }
    List<List<TerrainTile>> terrainTiles { get; }

    Subject<Vector2Int> OnElementTileChanged { get; }

    Subject<HumanCreatedData> OnHumanCreated { get; }
    Subject<HumanStateChangedData> OnHumanStateChanged { get; }
    Subject<HumanPickedUpResourceData> OnHumanPickedUpResource { get; }
    Subject<HumanPlacedResourceData> OnHumanPlacedResource { get; }

    Subject<TrainCreatedData> OnTrainCreated { get; }
    Subject<TrainNodeCreatedData> OnTrainNodeCreated { get; }
    Subject<TrainPickedUpResourceData> OnTrainPickedUpResource { get; }
    Subject<TrainPushedResourceData> OnTrainPushedResource { get; }

    Subject<BuildingProducedItemData> OnBuildingProducedItem { get; }

    Subject<TopBarResourceChangedData> OnResourceChanged { get; }

    void TryBuild(Vector2Int hoveredTile, SelectedItem gameManagerSelectedItem);

    bool AreThereAvailableResourcesForTheTrain(HorseTrain horse);
    void PickRandomItemForTheTrain(HorseTrain horse);
    bool AreThereAvailableSlotsTheTrainCanPassResourcesTo(HorseTrain horse);
    void PickRandomSlotForTheTrainToPassItemTo(HorseTrain horse);
    bool IsBuildable(int x, int y);
    bool IsBuildable(Vector2Int pos);
}
}
