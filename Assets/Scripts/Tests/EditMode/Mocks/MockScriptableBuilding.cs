using System.Collections.Generic;
using BFG.Runtime;
using BFG.Runtime.Entities;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tests.EditMode {
internal class MockScriptableBuilding : IScriptableBuilding {
    public MockScriptableBuilding(BuildingType type) {
        this.type = type;
    }

    public BuildingType type { get; }
    public ScriptableResource harvestableResource { get; }
    public int tilesRadius { get; }
    public int storeItemsAmount { get; }
    public int produceItemsAmount { get; }
    public List<Vector2> storedItemPositions { get; }
    public List<Vector2> producedItemsPositions { get; }
    public List<ScriptableResource> takes { get; }
    public ScriptableResource produces { get; }
    public float ItemProcessingDuration { get; }
    public TileBase tile { get; }
    public Vector2Int size { get; }
    public Vector2Int pickupableItemsCellOffset { get; }
    public string name { get; }
    public List<RequiredResourceToBuild> requiredResourcesToBuild { get; } = new();
    public float ConstructionDuration { get; }
}
}
