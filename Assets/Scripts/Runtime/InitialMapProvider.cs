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
    IMapSize _mapSize;

    public void Init(IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
    }

    public List<List<ElementTile>> LoadElementTiles() {
        var elementTiles = new List<List<ElementTile>>(_mapSize.sizeY);

        for (var y = 0; y < _mapSize.sizeY; y++) {
            var row = new List<ElementTile>(_mapSize.sizeX);

            for (var x = 0; x < _mapSize.sizeX; x++) {
                var tilemapTile = _movementSystemTilemap.GetTile(new(x, y));

                var isStables = false;
                var isCityHall = false;
                Building foundBuilding = null;
                foreach (var building in _map.buildings) {
                    if (building.scriptable.type != BuildingType.SpecialStable) {
                        continue;
                    }

                    if (building.posX == x && building.posY == y) {
                        if (building.scriptable.type == BuildingType.SpecialCityHall) {
                            isCityHall = true;
                            foundBuilding = building;
                        }
                        else if (building.scriptable.type == BuildingType.SpecialStable) {
                            isStables = true;
                        }

                        break;
                    }
                }

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
                else if (isStables) {
                    tile = new(ElementTileType.Stables, 0);
                }
                else if (isCityHall) {
                    tile = new(ElementTileType.Building, foundBuilding);
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
