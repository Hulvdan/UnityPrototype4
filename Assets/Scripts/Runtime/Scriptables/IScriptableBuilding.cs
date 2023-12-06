using System.Collections.Generic;
using BFG.Runtime.Entities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public interface IScriptableBuilding {
    BuildingType type { get; }
    ScriptableResource harvestableResource { get; }
    int tilesRadius { get; }
    int storeItemsAmount { get; }
    int produceItemsAmount { get; }
    List<Vector2> storedItemPositions { get; }
    List<Vector2> producedItemsPositions { get; }
    List<ScriptableResource> takes { get; }
    ScriptableResource produces { get; }
    float ItemProcessingDuration { get; }
    TileBase tile { get; }
    Vector2Int size { get; }
    Vector2Int pickupableItemsCellOffset { get; }
    string name { get; }
    List<RequiredResourceToBuild> requiredResourcesToBuild { get; }
    float BuildingDuration { get; }
}
}
