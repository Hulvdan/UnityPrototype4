using System;
using System.Collections.Generic;
using System.Linq;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace BFG.Runtime {
public class Resource {
    public int Amount;
    public string Codename;
}

public record ResourceChanged {
    public string Codename;
    public int NewAmount;
    public int OldAmount;
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
    float _humanHeadingToTheStoreBuildingDuration;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    [Min(0)]
    float _humanReturningBackDuration;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    AnimationCurve _humanMovementCurve = AnimationCurve.Linear(0, 0, 1, 1);

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

    Random _random;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<Human> _humans;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    ScriptableResource _logResource;

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
    readonly List<Resource> _resources = new();

    public float humanHeadingDuration => _humanHeadingDuration;
    public float humanHarvestingDuration => _humanHarvestingDuration;
    public float humanReturningBackDuration => _humanReturningBackDuration;
    public float humanTotalHarvestingDuration => _humanTotalHarvestingDuration;

    void GiveResource(string codename, int amount) {
        var resource = _resources.Find(x => x.Codename == codename);
        resource.Amount += amount;
        OnResourceChanged?.Invoke(
            new ResourceChanged {
                NewAmount = resource.Amount,
                OldAmount = resource.Amount - amount,
                Codename = resource.Codename
            }
        );
    }

    void Awake() {
        _random = new Random((int)Time.time);
        RegenerateTilemap();

        _resources.Add(new Resource { Amount = 0, Codename = "wood" });
        _resources.Add(new Resource { Amount = 0, Codename = "stone" });
        _resources.Add(new Resource { Amount = 0, Codename = "food" });
    }

    void Start() {
        CreateHuman(_buildings[0]);
        CreateHuman(_buildings[0]);
        CreateHuman(_buildings[1]);
        CreateHuman(_buildings[1]);
    }

    void Update() {
        UpdateHumans();
    }

    void OnValidate() {
        _humanTotalHarvestingDuration = _humanHeadingDuration
                                        + _humanHarvestingDuration
                                        + _humanHeadingToTheStoreBuildingDuration
                                        + _humanReturningBackDuration;
    }

    public event Action<HumanCreatedData> OnHumanCreated = delegate { };
    public event Action<HumanStateChangedData> OnHumanStateChanged = delegate { };
    public event Action<HumanHarvestedResourceData> OnHumanHarvestedResource = delegate { };
    public event Action<ResourceChanged> OnResourceChanged = delegate { };

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
                var forestK = MakeSomeNoise2D(_randomSeed, x, y, _forestNoiseScale);
                // var hasForest = false;
                var hasForest = forestK > _forestThreshold;
                var tile = new Tile {
                    Name = "grass",
                    resource = hasForest ? _logResource : null
                };
                tilesRow.Add(tile);
                // var randomH = Random.Range(0, _maxHeight + 1);
                var heightK = MakeSomeNoise2D(_randomSeed, x, y, _terrainHeightNoiseScale);
                var randomH = heightK * (_maxHeight + 1);
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
                    s.resource = null;
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

    void CreateHuman(Building building) {
        var human = new Human(Guid.NewGuid(), building, building.position);
        _humans.Add(human);
        OnHumanCreated?.Invoke(new HumanCreatedData(human));
    }

    void UpdateHumans() {
        foreach (var human in _humans) {
            if (human.state != HumanState.Idle) {
                human.harvestingElapsed += Time.deltaTime;

                var newState = human.state;
                if (human.harvestingElapsed >= _humanTotalHarvestingDuration) {
                    newState = HumanState.Idle;
                    human.harvestingElapsed = 0;

                    HarvestResource(human);
                }
                else if (human.harvestingElapsed >=
                         _humanHeadingDuration
                         + _humanHarvestingDuration
                         + _humanHeadingToTheStoreBuildingDuration) {
                    newState = HumanState.HeadingBackToTheHarvestBuilding;

                    if (human.state != newState) {
                        human.movingFrom = human.storeBuilding.position;
                        human.storeBuilding.isBooked = false;
                        human.storeBuilding = null;
                    }
                }
                else if (human.harvestingElapsed >=
                         _humanHeadingDuration
                         + _humanHarvestingDuration) {
                    newState = HumanState.HeadingToTheStoreBuilding;

                    if (human.harvestTilePosition.HasValue) {
                        var pos = human.harvestTilePosition.Value;
                        tiles[pos.y][pos.x].isBooked = false;
                    }
                }
                else if (human.harvestingElapsed >= _humanHeadingDuration) {
                    newState = HumanState.Harvesting;
                }

                ChangeHumanState(human, newState);
            }

            switch (human.state) {
                case HumanState.Idle:
                    UpdateHumanIdle(human);
                    break;
                case HumanState.HeadingToTheHarvestTile:
                    UpdateHumanHeadingToTheHarvestTile(human);
                    break;
                case HumanState.Harvesting:
                    UpdateHumanHarvesting(human);
                    break;
                case HumanState.HeadingToTheStoreBuilding:
                    UpdateHumanHeadingToTheStoreBuilding(human);
                    break;
                case HumanState.HeadingBackToTheHarvestBuilding:
                    UpdateHumanHeadingBackToTheHarvestBuilding(human);
                    break;
            }
        }
    }

    void HarvestResource(Human human) {
        var resourceCodename = "wood";
        var amount = 1;
        var where = human.harvestTilePosition.Value;

        GiveResource(resourceCodename, amount);

        OnHumanHarvestedResource?.Invoke(
            new HumanHarvestedResourceData(
                human,
                human.harvestBuilding.scriptableBuilding.harvestableResource,
                amount,
                where
            )
        );
    }

    void ChangeHumanState(Human human, HumanState newState) {
        if (newState == human.state) {
            return;
        }

        var oldState = human.state;
        human.state = newState;
        OnHumanStateChanged?.Invoke(new HumanStateChangedData(human, oldState, newState));
    }

    void UpdateHumanIdle(Human human) {
        var r = human.harvestBuilding.scriptableBuilding.cellsRadius;
        var leftInclusive = Math.Max(0, human.harvestBuilding.posX - r);
        var rightInclusive = Math.Min(sizeX - 1, human.harvestBuilding.posX + r);
        var topInclusive = Math.Min(sizeY - 1, human.harvestBuilding.posY + r);
        var bottomInclusive = Math.Max(0, human.harvestBuilding.posY - r);

        var resource = human.harvestBuilding.scriptableBuilding.harvestableResource;
        var yy = Enumerable.Range(bottomInclusive, 2 * r + 1).ToArray();
        var xx = Enumerable.Range(leftInclusive, 2 * r + 1).ToArray();

        Utils.Shuffle(yy, _random);
        Utils.Shuffle(xx, _random);

        Building storeBuildingCandidate = null;
        Vector2Int? tileCandidate = null;

        var shouldBreak = false;
        foreach (var y in yy) {
            foreach (var x in xx) {
                if (!tiles[y][x].isBooked && tiles[y][x].resource == resource) {
                    tileCandidate = new Vector2Int(x, y);
                    shouldBreak = true;
                    break;
                }
            }

            if (shouldBreak) {
                break;
            }
        }

        var buildingCopy = buildings.ToArray();
        Utils.Shuffle(buildingCopy, _random);
        foreach (var building in buildingCopy) {
            if (building.scriptableBuilding.type != BuildingType.Store) {
                continue;
            }

            if (building.isBooked) {
                continue;
            }

            var isWithinRadius = leftInclusive <= building.position.x
                                 && building.position.x <= rightInclusive
                                 && bottomInclusive <= building.position.y
                                 && building.position.y <= topInclusive;
            if (isWithinRadius) {
                storeBuildingCandidate = building;
                break;
            }
        }

        if (tileCandidate != null && storeBuildingCandidate != null) {
            var x = tileCandidate.Value.x;
            var y = tileCandidate.Value.y;
            human.harvestTilePosition = new Vector2Int(x, y);
            human.storeBuilding = storeBuildingCandidate;

            tiles[y][x].isBooked = true;
            storeBuildingCandidate.isBooked = true;

            ChangeHumanState(human, HumanState.HeadingToTheHarvestTile);
        }
    }

    void UpdateHumanHeadingToTheHarvestTile(Human human) {
        var t = human.harvestingElapsed / _humanHeadingDuration;
        var mt = _humanMovementCurve.Evaluate(t);
        human.position =
            Vector2.Lerp(human.harvestBuilding.position, human.harvestTilePosition.Value, mt);
    }

    void UpdateHumanHarvesting(Human human) {
    }

    void UpdateHumanHeadingToTheStoreBuilding(Human human) {
        var stateElapsed = human.harvestingElapsed
                           - _humanHeadingDuration
                           - _humanHarvestingDuration;
        var t = stateElapsed / _humanHeadingToTheStoreBuildingDuration;
        var mt = _humanMovementCurve.Evaluate(t);
        human.position =
            Vector2.Lerp(human.harvestTilePosition.Value, human.storeBuilding.position, mt);
    }

    void UpdateHumanHeadingBackToTheHarvestBuilding(Human human) {
        var stateElapsed = human.harvestingElapsed
                           - _humanHeadingDuration
                           - _humanHarvestingDuration
                           - _humanHeadingToTheStoreBuildingDuration;
        var t = stateElapsed / _humanReturningBackDuration;
        var mt = _humanMovementCurve.Evaluate(t);
        human.position =
            Vector2.Lerp(human.movingFrom, human.harvestBuilding.position, mt);
    }

    #endregion
}
}
