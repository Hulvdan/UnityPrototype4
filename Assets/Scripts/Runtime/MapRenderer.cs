using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public class MapRenderer : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    const string BuildingsTilemapNameTemplate = "Gen-Buildings";
    const string ResourcesTilemapNameTemplate = "Gen-Resources";

    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

    [SerializeField]
    [Required]
    Grid _grid;

    [SerializeField]
    [Required]
    TileBase _tileGrass;

    [SerializeField]
    TileBase _tileRoad;

    [SerializeField]
    [Required]
    TileBase _tileForest;

    [SerializeField]
    [Required]
    TileBase _tileForestTop;

    [SerializeField]
    [Required]
    GameObject _tilemapPrefab;

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    [SerializeField]
    [Required]
    TileBase _debugTileWalkable;

    [SerializeField]
    [Required]
    TileBase _debugTileUnwalkable;

    public void ResetRenderer() {
        DeleteOldTilemaps();
        RegenerateTilemapGameObject();
        RegenerateDebugTilemapGameObject();
    }

    void DeleteOldTilemaps() {
        foreach (Transform child in _grid.transform) {
            if (child.gameObject.name.StartsWith(TerrainTilemapNameTemplate)
                || child.gameObject.name.StartsWith(BuildingsTilemapNameTemplate)
                || child.gameObject.name.StartsWith(ResourcesTilemapNameTemplate)) {
                child.gameObject.SetActive(false);
            }
        }
    }

    void RegenerateTilemapGameObject() {
        var maxHeight = 0;
        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                maxHeight = Math.Max(maxHeight, _map.tileHeights[y][x]);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain = GenerateTilemap(i, -i / 1000f, TerrainTilemapNameTemplate);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        var buildings = GenerateTilemap(0, -(maxHeight + 1) / 1000f, BuildingsTilemapNameTemplate)
            .GetComponent<Tilemap>();
        terrainMaps.Add(buildings);

        var resources = GenerateTilemap(0, -(maxHeight + 2) / 1000f, ResourcesTilemapNameTemplate)
            .GetComponent<Tilemap>();
        terrainMaps.Add(resources);

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _map.sizeY; y++) {
                if (h > 0 && y == _map.sizeY) {
                    continue;
                }

                for (var x = 0; x < _map.sizeX; x++) {
                    if (
                        h == 0
                        || _map.tileHeights[y][x] >= h
                        || (y > 0 && _map.tileHeights[y - 1][x] == h)
                    ) {
                        terrainMaps[h].SetTile(new Vector3Int(x, y, 0), _tileGrass);
                    }
                }
            }
        }

        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                if (_map.tiles[y][x].HasForest) {
                    resources.SetTile(new Vector3Int(x, y, 0), _tileForest);
                    resources.SetTile(new Vector3Int(x, y + 1, 0), _tileForestTop);
                }
            }
        }
    }

    void RegenerateDebugTilemapGameObject() {
        _debugTilemap.ClearAllTiles();

        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                bool walkable;
                if (y >= _map.sizeY) {
                    walkable = false;
                }
                else {
                    walkable = !TileIsACliff(x, y);
                }

                _debugTilemap.SetTile(new Vector3Int(x, y, 0),
                    walkable ? _debugTileWalkable : _debugTileUnwalkable);
            }
        }
    }

    bool TileIsACliff(int x, int y) {
        return _map.tiles[y][x].Name == "cliff";
    }

    GameObject GenerateTilemap(int i, float y, string nameTemplate) {
        var terrainTilemap = Instantiate(_tilemapPrefab, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.localPosition = new Vector3(0, y, 0);
        return terrainTilemap;
    }
}
}
