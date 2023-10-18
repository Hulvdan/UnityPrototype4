using System;
using System.Collections.Generic;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public struct Tile {
    public string Name;
    public bool HasForest;
}

public class Map : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    const string BuildingsTilemapNameTemplate = "Gen-Buildings";
    const string ResourcesTilemapNameTemplate = "Gen-Resources";

    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Stone, Trees, Ore, Buildings, Rails
    // 3 - Animated (carts, trains, people, horses)
    // 4 - Particles (smoke, footstep dust etc)
    //
    // Stone, Tree, Ore:

    [Header("Dependencies")]
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

    [Header("Random")]
    [SerializeField]
    int _randomSeed;

    [SerializeField]
    [Min(0)]
    float _terrainHeightNoiseScale = 1;

    [SerializeField]
    [Min(0)]
    float _forestNoiseScale = 1;

    [SerializeField]
    [Range(0, 1)]
    float _forestThreshold = 1f;

    [SerializeField]
    [Min(0)]
    int _maxHeight = 1;

    List<List<int>> _tileHeights;
    List<List<Tile>> _tiles;

    void Awake() {
        RegenerateTilemap();
    }

    [Button("Regen With New Seed")]
    void RegenerateTilemapWithNewSeed() {
        _randomSeed += 1;
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
                var tile = new Tile {
                    Name = "grass",
                    HasForest = MakeSomeNoise2D(_randomSeed, x, y, _forestNoiseScale) >
                                _forestThreshold
                };
                tiles.Add(tile);

                var randomH = MakeSomeNoise2D(_randomSeed, x, y, _terrainHeightNoiseScale) *
                              (_maxHeight + 1);
                tileHeights.Add(Mathf.Min(_maxHeight, (int)randomH));
            }

            _tiles.Add(tiles);
            _tileHeights.Add(tileHeights);
        }

        DeleteOldTilemaps();
        RegenerateTilemapGameObject();
    }

    float MakeSomeNoise2D(int seed, int x, int y, float scale) {
        Noise.Seed = seed;
        return Noise.CalcPixel2D(x, y, scale) / 255f;
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
            var terrain = GenerateTilemap(i, -i / 1000f, TerrainTilemapNameTemplate);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        var buildings = GenerateTilemap(0, -maxHeight - 1 / 1000f, BuildingsTilemapNameTemplate)
            .GetComponent<Tilemap>();
        terrainMaps.Add(buildings);

        var resources = GenerateTilemap(0, -maxHeight - 1 / 1000f, ResourcesTilemapNameTemplate)
            .GetComponent<Tilemap>();
        terrainMaps.Add(resources);

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _mapSizeY; y++) {
                for (var x = 0; x < _mapSizeX; x++) {
                    if (h == 0
                        || _tileHeights[y][x] == h
                        || (y > 0 && _tileHeights[y - 1][x] == h)) {
                        terrainMaps[h].SetTile(new Vector3Int(x, y, 0), _tileGrass);
                    }

                    if (h == 0
                        && (
                            _tiles[y][x].HasForest
                            || (y < _tiles.Count - 1 && _tiles[y + 1][x].HasForest)
                        )
                        && TileIsNotACliff(x, y)
                       ) {
                        resources.SetTile(new Vector3Int(x, y + 1, 0), _tileForest);
                    }
                }
            }
        }
    }

    bool TileIsNotACliff(int x, int y) {
        if (y >= _tiles.Count - 1) {
            return true;
        }

        return _tileHeights[y][x] < _tileHeights[y + 1][x];
    }

    GameObject GenerateTilemap(int i, float y, string nameTemplate) {
        var terrainTilemap = Instantiate(_tilemapPrefab, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.position = new Vector3(0, y, 0);
        return terrainTilemap;
    }
}
}
