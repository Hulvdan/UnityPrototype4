using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
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

    [SerializeField]
    [ShowIf("_type", BuildingType.Harvest)]
    [Min(0)]
    int _cellsRadius;

    [SerializeField]
    [ShowIf("_type", BuildingType.Store)]
    [Min(1)]
    int _storeItemsAmount = 1;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    List<ScriptableResource> _takes;

    [SerializeField]
    [ShowIf("_type", BuildingType.Produce)]
    [Required]
    ScriptableResource _produces;

    [SerializeField]
    [PreviewField]
    TileBase _tile;

    public ScriptableResource harvestableResource => _harvestableResource;
    public int cellsRadius => _cellsRadius;
    public int storeItemsAmount => _storeItemsAmount;
    public List<ScriptableResource> takes => _takes;
    public ScriptableResource produces => _produces;

    public TileBase tile => _tile;
}
}
