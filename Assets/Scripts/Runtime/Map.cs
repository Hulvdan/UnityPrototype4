using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace BFG.Runtime {
public struct Tile {
    public string Name;
}

public class Map : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Stone, Trees, Ore, Buildings, Rails
    // 3 - Animated (carts, trains, people, horses)
    // 4 - Particles (smoke, footstep dust etc)
    //
    // Stone, Tree, Ore:

    [Header("Dependencies")]
    [SerializeField]
    Grid _grid;

    [SerializeField]
    TileBase _tileGrass;

    [SerializeField]
    GameObject _tilemapPrefab;

    [Header("Configuration")]
    // [SerializeField]
    // int _randomSeed;
    [SerializeField]
    [Min(1)]
    int _mapSizeX = 10;

    [SerializeField]
    [Min(1)]
    int _mapSizeY = 10;

    List<List<int>> _tileHeights;
    List<List<Tile>> _tiles;

    void Awake() {
        RegenerateTilemap();
    }

    [Button("RegenerateTilemap")]
    void RegenerateTilemap() {
        _tiles = new List<List<Tile>>();
        _tileHeights = new List<List<int>>();

        for (var y = 0; y < _mapSizeY; y++) {
            var tiles = new List<Tile>();
            var tileHeights = new List<int>();

            for (var x = 0; x < _mapSizeX; x++) {
                tiles.Add(new Tile { Name = "grass" });
                tileHeights.Add(Random.Range(0, 1f) > .5f ? 0 : 1);
            }

            _tiles.Add(tiles);
            _tileHeights.Add(tileHeights);
        }

        DeleteOldTilemaps();
        RegenerateTilemapGameObject();
    }

    void DeleteOldTilemaps() {
        foreach (Transform child in _grid.transform) {
            if (child.gameObject.name.StartsWith(TerrainTilemapNameTemplate)) {
                child.gameObject.SetActive(false);
            }
        }
    }

    void RegenerateTilemapGameObject() {
        var maxHeight = 0;
        for (var y = 0; y < _mapSizeY; y++) {
            for (var x = 0; x < _mapSizeX; x++) {
                maxHeight = Math.Max(maxHeight, _tileHeights[y][x]);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrainTilemap = Instantiate(_tilemapPrefab, _grid.transform);
            terrainTilemap.name = TerrainTilemapNameTemplate + i;
            terrainTilemap.transform.position = new Vector3(0, -i / 1000f, 0);
            terrainMaps.Add(terrainTilemap.GetComponent<Tilemap>());
        }

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _mapSizeY; y++) {
                for (var x = 0; x < _mapSizeX; x++) {
                    if (h == 0) {
                        continue;
                    }

                    if (h == 0
                        || _tileHeights[y][x] == h
                        || (y > 0 && _tileHeights[y - 1][x] == h)) {
                        terrainMaps[h].SetTile(new Vector3Int(x, y, 0), _tileGrass);
                    }
                }
            }
        }
    }
}
}
