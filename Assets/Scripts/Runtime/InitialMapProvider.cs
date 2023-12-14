using System.Collections.Generic;
using BFG.Runtime.Entities;
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

    IMap _map;
    IMapSize _mapSize;

    public void Init(IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
    }

    public List<List<ElementTile>> LoadElementTiles() {
        var elementTiles = new List<List<ElementTile>>(_mapSize.height);

        for (var y = 0; y < _mapSize.height; y++) {
            var row = new List<ElementTile>(_mapSize.width);

            for (var x = 0; x < _mapSize.width; x++) {
                var pos = new Vector2Int(x, y);
                var tilemapTile = _movementSystemTilemap.GetTile((Vector3Int)pos);

                var isCityHall = false;
                Building foundBuilding = null;
                foreach (var building in _map.buildings) {
                    if (building.pos == pos) {
                        if (building.scriptable.type == BuildingType.SpecialCityHall) {
                            isCityHall = true;
                            foundBuilding = building;
                        }

                        break;
                    }
                }

                ElementTile tile;
                if (tilemapTile == _roadTile) {
                    tile = ElementTile.ROAD;
                }
                else if (isCityHall) {
                    tile = new(ElementTileType.Building, foundBuilding);
                }
                else {
                    tile = ElementTile.NONE;
                }

                row.Add(tile);
            }

            elementTiles.Add(row);
        }

        return elementTiles;
    }
}
}
