using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Building", order = 1)]
public class ScriptableBuilding : ScriptableObject {
    [SerializeField]
    string _codename;

    [SerializeField]
    [EnumToggleButtons]
    BuildingType _type;

    [SerializeField]
    [ShowIf("_type", BuildingType.Harvest)]
    string _harvestResourceCodename;

    [SerializeField]
    [ShowIf("_type", BuildingType.Harvest)]
    [Min(0)]
    int _cellsRadius;

    [SerializeField]
    [ShowIf("_type", BuildingType.Store)]
    [Min(1)]
    int _storeItemsAmount = 1;

    [SerializeField]
    [PreviewField]
    TileBase _tile;

    public string codename => _codename;
    public string harvestResourceCodename => _harvestResourceCodename;
    public int cellsRadius => _cellsRadius;
    public int storeItemsAmount => _storeItemsAmount;

    public TileBase tile => _tile;
}
}
