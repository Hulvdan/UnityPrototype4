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

    IMap _map;
    IMapContains _mapContains;

    public void Init(IMap map, IMapContains mapContains) {
        _map = map;
        _mapContains = mapContains;
    }

    public List<List<ElementTile>> LoadElementTiles() {
        var elementTiles = new List<List<ElementTile>>(_mapContains.sizeY);

        for (var y = 0; y < _mapContains.sizeY; y++) {
            var row = new List<ElementTile>(_mapContains.sizeX);

            for (var x = 0; x < _mapContains.sizeX; x++) {
                var tilemapTile = _movementSystemTilemap.GetTile(new(x, y));

                ElementTile tile;
                if (tilemapTile == _roadTile) {
                    tile = ElementTile.Road;
                }
                else if (tilemapTile == _stationHorizontalTile) {
                    tile = new(ElementTileType.Station, 0);
                }
                else if (tilemapTile == _stationVerticalTile) {
                    tile = new(ElementTileType.Station, 1);
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
