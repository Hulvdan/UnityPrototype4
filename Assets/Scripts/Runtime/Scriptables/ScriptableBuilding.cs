using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Building", order = 1)]
public class ScriptableBuilding : ScriptableObject {
    [SerializeField]
    [EnumToggleButtons]
    BuildingType _type;

    [SerializeField]
    [ShowIf("_type", BuildingType.Harvest)]
    ScriptableResource _harvestableResource;

    [FormerlySerializedAs("_cellsRadius")]
    [SerializeField]
    [ShowIf("_type", BuildingType.Harvest)]
    [Min(0)]
    int _tilesRadius;

    [SerializeField]
    [ShowIf("@_type == BuildingType.Store || _type == BuildingType.Produce")]
    [Min(1)]
    int _storeItemsAmount = 1;

    [SerializeField]
    [ShowIf("@_type == BuildingType.Store || _type == BuildingType.Produce")]
    List<Vector2> _storedItemPositions = new();

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    List<ScriptableResource> _takes;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    [Required]
    ScriptableResource _produces;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    [Min(1)]
    int _produceItemsAmount = 1;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    List<Vector2> _producedItemsPositions = new();

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    Vector2Int _pickupableItemsCellOffset = Vector2Int.zero;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    [Min(0.1f)]
    float _itemProcessingDuration = 1f;

    [SerializeField]
    [PreviewField]
    TileBase _tile;

    [SerializeField]
    [Min(1)]
    Vector2Int _size = Vector2Int.one;

    public BuildingType type => _type;

    public ScriptableResource harvestableResource {
        get {
            if (_type != BuildingType.Harvest) {
                Debug.LogError("WTF?");
            }

            return _harvestableResource;
        }
    }

    public int tilesRadius => _tilesRadius;

    public int storeItemsAmount {
        get {
            if (_type != BuildingType.Produce && _type != BuildingType.Store) {
                Debug.LogError("WTF?");
            }

            return _storeItemsAmount;
        }
    }

    public int produceItemsAmount {
        get {
            if (_type != BuildingType.Produce) {
                Debug.LogError("WTF?");
            }

            return _produceItemsAmount;
        }
    }

    public List<Vector2> storedItemPositions => _storedItemPositions;
    public List<Vector2> producedItemsPositions => _producedItemsPositions;

    public List<ScriptableResource> takes => _takes;
    public ScriptableResource produces => _produces;
    public float ItemProcessingDuration => _itemProcessingDuration;

    public TileBase tile => _tile;
    public Vector2Int size => _size;
    public Vector2Int pickupableItemsCellOffset => _pickupableItemsCellOffset;

    public static class Tests {
        public static ScriptableBuilding Build(BuildingType type) {
            return new() { _type = type };
        }
    }
}
}
