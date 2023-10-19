using System.Collections.Generic;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BFG.Runtime {
public struct Tile {
    public string Name;
    public bool HasForest;
}

public class Map : MonoBehaviour {
    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Stone, Trees, Ore, Buildings, Rails
    // 3 - Animated (carts, trains, people, horses)
    // 4 - Particles (smoke, footstep dust etc)
    //
    // Stone, Tree, Ore:

    [Header("Configuration")]
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

    [Header("Setup")]
    [SerializeField]
    public UnityEvent OnTilesRegenerated;

    [SerializeField]
    List<Building> _buildings;

    [SerializeField]
    List<Human> _humans;

    public int sizeY => _mapSizeY;
    public int sizeX => _mapSizeX;

    // NOTE(Hulvdan): Indexes start from the bottom left corner and go to the top right one
    public List<List<Tile>> tiles { get; private set; }
    public List<List<int>> tileHeights { get; private set; }

    public List<Building> buildings => _buildings;
    public List<Human> humans => _humans;

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
        tiles = new List<List<Tile>>();
        tileHeights = new List<List<int>>();

        // NOTE(Hulvdan): Generating tiles
        for (var y = 0; y < _mapSizeY; y++) {
            var tilesRow = new List<Tile>();
            var tileHeightsRow = new List<int>();

            for (var x = 0; x < _mapSizeX; x++) {
                var tile = new Tile {
                    Name = "grass",
                    // HasForest = false
                    HasForest = MakeSomeNoise2D(_randomSeed, x, y, _forestNoiseScale) >
                                _forestThreshold
                };
                tilesRow.Add(tile);
                // var randomH = Random.Range(0, _maxHeight + 1);
                var randomH = MakeSomeNoise2D(_randomSeed, x, y, _terrainHeightNoiseScale) *
                              (_maxHeight + 1);
                tileHeightsRow.Add(Mathf.Min(_maxHeight, (int)randomH));
            }

            tiles.Add(tilesRow);
            tileHeights.Add(tileHeightsRow);
        }

        for (var y = 0; y < _mapSizeY; y++) {
            for (var x = 0; x < _mapSizeX; x++) {
                if (y == 0 || tileHeights[y][x] > tileHeights[y - 1][x]) {
                    var s = tiles[y][x];
                    s.Name = "cliff";
                    s.HasForest = false;
                    tiles[y][x] = s;
                }
            }
        }

        OnTilesRegenerated?.Invoke();
    }

    float MakeSomeNoise2D(int seed, int x, int y, float scale) {
        Noise.Seed = seed;
        return Noise.CalcPixel2D(x, y, scale) / 255f;
    }
}
}
