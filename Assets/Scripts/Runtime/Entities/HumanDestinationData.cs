#nullable enable
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime.Entities {
[Serializable]
public class HumanDestinationData {
    public HumanDestinationType type;

    public Vector2Int? fishingTile;

    public Vector2Int? buildingTile;
}
}
