using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Building", order = 1)]
public class Building : ScriptableObject {
    [SerializeField]
    string _codename;

    [SerializeField]
    string _harvestTileCodename;

    [SerializeField]
    TileBase _tile;

    public string codename => _codename;
    public string harvestTileCodename => _harvestTileCodename;

    public TileBase tile => _tile;
}
}
