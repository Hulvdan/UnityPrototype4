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
    Subject<TrainNodePickedUpResourceData> OnTrainPickedUpResource { get; }

    Subject<TopBarResourceChangedData> OnResourceChanged { get; }

    void TryBuild(Vector2Int hoveredTile, SelectedItem gameManagerSelectedItem);

    bool AreThereAvailableResourcesForTheTrain(HorseTrain horse);
    void PickRandomItemForTheTrain(HorseTrain horse);
    bool AreThereAvailableSlotsTheTrainCanPassResourcesTo(HorseTrain horse);
    void PickRandomSlotForTheTrainToPassItemTo(HorseTrain horse);
}

public class TrainCreatedData {
    public HorseTrain Horse;
}

public class TrainNodeCreatedData {
    public HorseTrain Horse;
    public bool IsLocomotive;
    public TrainNode Node;
}
}
