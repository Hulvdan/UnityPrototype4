#nullable enable
using System;
using UnityEngine;

namespace BFG.Runtime.Entities {
[Serializable]
public class HumanDestinationData {
    public HumanDestinationType Type;
    public Vector2Int? FishingTile;
    public Vector2Int? BuildingTile;
}
}
