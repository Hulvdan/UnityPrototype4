#nullable enable
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class HumanDestination {
    public HumanDestination(
        HumanDestinationType type,
        Vector2Int pos,
        Vector2Int? fishingTile,
        Vector2Int? buildingTile
    ) {
        this.type = type;
        this.pos = pos;
        _fishingTile = fishingTile;
        _buildingTile = buildingTile;
    }

    public HumanDestinationType type { get; }
    public Vector2Int pos { get; }

    public Vector2Int fishingTile {
        get {
            Assert.AreEqual(type, HumanDestinationType.FishingCoast);
            Assert.AreNotEqual(_fishingTile, null);
            return _fishingTile!.Value;
        }
    }

    public Vector2Int buildingTile {
        get {
            Assert.AreEqual(type, HumanDestinationType.FishingCoast);
            Assert.AreNotEqual(_buildingTile, null);
            return _buildingTile!.Value;
        }
    }

    Vector2Int? _fishingTile;
    Vector2Int? _buildingTile;
}
}
