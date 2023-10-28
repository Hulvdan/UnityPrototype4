using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public class InitialMapProvider : MonoBehaviour {
    [FoldoutGroup("Dependencies", true)]
    [SerializeField]
    [Required]
    Tilemap _movementSystemTilemap;

    [FoldoutGroup("Dependencies", true)]
    [SerializeField]
    [Required]
    TileBase _roadTile;

    [FoldoutGroup("Dependencies", true)]
    [SerializeField]
    [Required]
    TileBase _stationHorizontalTile;

    [FoldoutGroup("Dependencies", true)]
    [SerializeField]
    [Required]
    TileBase _stationVerticalTile;

    Map _map;

    public void Init(Map map) {
        _map = map;
    }

    public List<List<ElementTile>> LoadElementTiles() {
        var elementTiles = new List<List<ElementTile>>(_map.sizeY);

        for (var y = 0; y < _map.sizeY; y++) {
            var row = new List<ElementTile>(_map.sizeX);

            for (var x = 0; x < _map.sizeX; x++) {
                var tilemapTile = _movementSystemTilemap.GetTile(new Vector3Int(x, y));

                ElementTile tile;
                if (tilemapTile == _roadTile) {
                    tile = ElementTile.Road;
                }
                else if (tilemapTile == _stationHorizontalTile) {
                    tile = new ElementTile(ElementTileType.Station, 0);
                }
                else if (tilemapTile == _stationVerticalTile) {
                    tile = new ElementTile(ElementTileType.Station, 1);
                }
                else {
                    tile = ElementTile.None;
                }

                row.Add(tile);
            }

            elementTiles.Add(row);
        }

        return elementTiles;
    }
}
}
