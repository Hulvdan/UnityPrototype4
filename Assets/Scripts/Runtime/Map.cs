using System;
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
    [FoldoutGroup("Map", true)]
    [SerializeField]
    [Min(1)]
    int _mapSizeX = 10;

    [FoldoutGroup("Map", true)]
    [SerializeField]
    [Min(1)]
    int _mapSizeY = 10;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    [Min(0)]
    float _humanHeadingDuration;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    [Min(0)]
    float _humanHarvestingDuration;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    [Min(0)]
    float _humanReturningBackDuration;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    int _randomSeed;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    [Min(0)]
    float _terrainHeightNoiseScale = 1;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    [Min(0)]
    float _forestNoiseScale = 1;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    [Range(0, 1)]
    float _forestThreshold = 1f;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    [Min(0)]
    int _maxHeight = 1;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    public UnityEvent OnTilesRegenerated;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<Building> _buildings;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<Human> _humans;

    [FoldoutGroup("Humans", true)]
    [ShowInInspector]
    [ReadOnly]
    float _humanTotalHarvestingDuration;

    public int sizeY => _mapSizeY;
    public int sizeX => _mapSizeX;

    // NOTE(Hulvdan): Indexes start from the bottom left corner and go to the top right one
    public List<List<Tile>> tiles { get; private set; }
    public List<List<int>> tileHeights { get; private set; }

    public List<Building> buildings => _buildings;
    public List<Human> humans => _humans;

    public float humanHeadingDuration => _humanHeadingDuration;
    public float humanHarvestingDuration => _humanHarvestingDuration;
    public float humanReturningBackDuration => _humanReturningBackDuration;
    public float humanTotalHarvestingDuration => _humanTotalHarvestingDuration;

    void Awake() {
        RegenerateTilemap();
    }

    void Start() {
        CreateHuman();
    }

    void Update() {
        UpdateHumans();
    }

    void OnValidate() {
        _humanTotalHarvestingDuration = _humanHeadingDuration
                                        + _humanHarvestingDuration
                                        + _humanReturningBackDuration;
    }

    public event Action<HumanCreatedData> OnHumanCreated = delegate { };

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

    #region HumanSystem

    void CreateHuman() {
        var human = new Human(Guid.NewGuid(), _buildings[0], _buildings[0].position);
        _humans.Add(human);
        OnHumanCreated?.Invoke(new HumanCreatedData(human));
    }

    void UpdateHumans() {
        foreach (var human in _humans) {
            switch (human.state) {
                case HumanState.Idle:
                    UpdateHumanIdle(human);
                    break;
                case HumanState.HeadingToTheTarget:
                    UpdateHumanHeadingToTheTarget(human);
                    break;
                case HumanState.Harvesting:
                    UpdateHumanHarvesting(human);
                    break;
                case HumanState.HeadingBackToTheBuilding:
                    UpdateHumanHeadingBackToTheBuilding(human);
                    break;
            }

            if (human.state == HumanState.Idle) {
            }
        }
    }

    void UpdateHumanIdle(Human human) {
        var r = human.building.scriptableBuilding.cellsRadius;
        var leftInclusive = Math.Max(0, human.building.posX - r);
        var rightInclusive = Math.Min(sizeX - 1, human.building.posX + r);
        var topInclusive = Math.Min(sizeY - 1, human.building.posY + r);
        var bottomInclusive = Math.Max(0, human.building.posY - r);

        var cellName = human.building.scriptableBuilding.harvestTileCodename;
        for (var y = bottomInclusive; y <= topInclusive; y++) {
            for (var x = leftInclusive; x <= rightInclusive; x++) {
                if (tiles[y][x].Name == cellName) {
                    human.positionTarget = new Vector2Int(x, y);
                    human.state = HumanState.HeadingToTheTarget;
                    break;
                }
            }
        }
    }

    void UpdateHumanHeadingToTheTarget(Human human) {
    }

    void UpdateHumanHarvesting(Human human) {
    }

    void UpdateHumanHeadingBackToTheBuilding(Human human) {
    }

    #endregion
}

public class HumanCreatedData {
    public Human Human;

    public HumanCreatedData(Human human) {
        Human = human;
    }
}
}
