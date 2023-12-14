#nullable enable
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime.Entities {
[Serializable]
public class HumanDestinationData {
    [FormerlySerializedAs("Type")]
    public HumanDestinationType type;

    [FormerlySerializedAs("FishingTile")]
    public Vector2Int? fishingTile;

    [FormerlySerializedAs("BuildingTile")]
    public Vector2Int? buildingTile;
}
}
