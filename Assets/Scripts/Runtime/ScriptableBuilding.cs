using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Building", order = 1)]
public class ScriptableBuilding : ScriptableObject {
    [SerializeField]
    string _codename;

    [SerializeField]
    string _harvestTileCodename;

    [SerializeField]
    [Min(0)]
    int _cellsRadius;

    [SerializeField]
    TileBase _tile;

    public string codename => _codename;
    public string harvestTileCodename => _harvestTileCodename;
    public int cellsRadius => _cellsRadius;

    public TileBase tile => _tile;
}
}
