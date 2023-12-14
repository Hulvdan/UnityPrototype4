#nullable enable
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class HumanDestination {
    public HumanDestination(
        HumanDestinationType type_,
        Vector2Int pos_,
        Vector2Int? fishingTile,
        Vector2Int? buildingTile
    ) {
        type = type_;
        pos = pos_;
        _fishingTile = fishingTile;
        _buildingTile = buildingTile;
    }

    public HumanDestinationType type { get; }
    public Vector2Int pos { get; }

    public Vector2Int fishingTile {
        get {
            Assert.AreEqual(type, HumanDestinationType.Fishing);
            Assert.AreNotEqual(_fishingTile, null);
            return _fishingTile!.Value;
        }
    }

    public Vector2Int buildingTile {
        get {
            Assert.AreEqual(type, HumanDestinationType.Fishing);
            Assert.AreNotEqual(_buildingTile, null);
            return _buildingTile!.Value;
        }
    }

    readonly Vector2Int? _fishingTile;
    readonly Vector2Int? _buildingTile;
}
}
