using System;
using System.Collections.Generic;
using BFG.Runtime.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Building", order = 1)]
public class ScriptableBuilding : ScriptableObject, IScriptableBuilding {
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
    [ShowIf("@_type == BuildingType.Produce")]
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

    [SerializeField]
    List<RequiredResourceToBuild> _requiredResourcesToBuild;

    [SerializeField]
    [Min(0.01f)]
    float _buildingDuration = 4f;

    [SerializeField]
    Vector2Int _workingAreaSize = -Vector2Int.one;

    [Header("Behaviour")]
    [SerializeField]
    List<BuildingBehaviourGo> _buildingBehaviourGos = new();

    public BuildingType type => _type;

    public float constructionDuration => _buildingDuration;

    public ScriptableResource harvestableResource {
        get {
            if (_type != BuildingType.Harvest) {
                Debug.LogError("WTF?");
            }

            return _harvestableResource;
        }
    }

    public int tilesRadius => _tilesRadius;

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
    public float itemProcessingDuration => _itemProcessingDuration;

    public TileBase tile => _tile;
    public Vector2Int size => _size;
    public Vector2Int pickupableItemsCellOffset => _pickupableItemsCellOffset;

    public List<RequiredResourceToBuild> requiredResourcesToBuild => _requiredResourcesToBuild;

    public Vector2Int workingAreaSize {
        get {
            switch (type) {
                case BuildingType.Harvest:
                case BuildingType.Plant:
                case BuildingType.Fish:
                    Assert.IsTrue(_workingAreaSize.x > 0);
                    Assert.IsTrue(_workingAreaSize.y > 0);
                    return _workingAreaSize;
                case BuildingType.Produce:
                case BuildingType.SpecialCityHall:
                default:
                    throw new NotSupportedException();
            }
        }
    }

    List<BuildingBehaviour> _buildingBehaviours;

    public List<BuildingBehaviour> behaviours {
        get {
            if (_buildingBehaviours == null) {
                _buildingBehaviours = new();
                foreach (var beh in _buildingBehaviourGos) {
                    _buildingBehaviours.Add(beh.ToBuildingBehaviour());
                }
            }

            return _buildingBehaviours;
        }
    }
}
}
