using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = System.Random;

namespace BFG.Runtime {
public class Map : MonoBehaviour, IMap, IMapSize {
    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Trees, Stone, Ore
    // 3 - Roads, Rails
    // 4 - Humans, Stations, Buildings
    // 5 - Horses
    // 6 - To Be Implemented: Particles (smoke, footstep dust etc)

    [FoldoutGroup("Map", true)]
    [SerializeField]
    [Min(1)]
    int _mapSizeX = 10;

    [FoldoutGroup("Map", true)]
    [SerializeField]
    [Min(1)]
    int _mapSizeY = 10;

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
    [Min(1)]
    int _maxForestAmount = 1;

    [FoldoutGroup("Random", true)]
    [SerializeField]
    [Min(0)]
    int _maxHeight = 1;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    public UnityEvent OnTerrainTilesRegenerated;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<Building> _buildings;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<Human> _humans;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    ScriptableResource _logResource;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    List<ScriptableResource> _topBarResources;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    InitialMapProvider _initialMapProvider;

    [FormerlySerializedAs("_compoundSystem")]
    [FormerlySerializedAs("_movementSystemInterface")]
    [FoldoutGroup("Horse Movement System", true)]
    [SerializeField]
    [Required]
    HorseCompoundSystem _horseCompoundSystem;

    [FormerlySerializedAs("_horsesGatheringAroundStationRadius")]
    [FoldoutGroup("Horse Movement System", true)]
    [SerializeField]
    [Min(1)]
    int _horsesStationItemsGatheringRadius = 2;

    readonly List<TopBarResource> _resources = new();

    GameManager _gameManager;

    [FoldoutGroup("Humans", true)]
    [ShowInInspector]
    [ReadOnly]
    float _humanTotalHarvestingDuration;

    Random _random;

    void Awake() {
        _random = new((int)Time.time);
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

    public Subject<Vector2Int> OnElementTileChanged { get; } = new();

    // NOTE(Hulvdan): Indexes start from the bottom left corner and go to the top right one
    public List<List<ElementTile>> elementTiles { get; private set; }
    public List<List<TerrainTile>> terrainTiles { get; private set; }
    public List<Building> buildings => _buildings;

    public void TryBuild(Vector2Int pos, SelectedItem item) {
        if (!Contains(pos)) {
            return;
        }

        if (elementTiles[pos.y][pos.x].Type != ElementTileType.None) {
            return;
        }

        if (item == SelectedItem.Road) {
            var road = elementTiles[pos.y][pos.x];
            road.Type = ElementTileType.Road;
            elementTiles[pos.y][pos.x] = road;
        }
        else if (item == SelectedItem.Station) {
            var tile = elementTiles[pos.y][pos.x];
            tile.Type = ElementTileType.Station;
            tile.Rotation = _gameManager.selectedItemRotation == 0 ? 1 : 0;
            elementTiles[pos.y][pos.x] = tile;
        }
        else {
            return;
        }

        OnElementTileChanged.OnNext(pos);
    }

    public bool IsBuildable(int x, int y) {
        if (y >= sizeY) {
            Debug.LogError("WTF?");
            return false;
        }

        if (terrainTiles[y][x].Name == "cliff") {
            return false;
        }

        if (terrainTiles[y][x].Resource != null) {
            return false;
        }

        if (elementTiles[y][x].Type != ElementTileType.None) {
            return false;
        }

        foreach (var building in buildings) {
            if (building.Contains(x, y)) {
                return false;
            }
        }

        return true;
    }

    public bool IsBuildable(Vector2Int pos) {
        return IsBuildable(pos.x, pos.y);
    }

    public int sizeY => _mapSizeY;
    public int sizeX => _mapSizeX;

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < sizeX && y >= 0 && y < sizeY;
    }

    public void InitDependencies(GameManager gameManager) {
        _gameManager = gameManager;
    }

    public void Init() {
        _initialMapProvider.Init(this, this);

        RegenerateTilemap();

        foreach (var res in _topBarResources) {
            _resources.Add(new() { Amount = 0, Resource = res });
        }

        elementTiles = _initialMapProvider.LoadElementTiles();
        OnTerrainTilesRegenerated?.Invoke();

        _horseCompoundSystem.Init(this, this);

        CreateHuman(_buildings[0]);
        CreateHuman(_buildings[0]);
        CreateHuman(_buildings[1]);
        CreateHuman(_buildings[1]);
    }

    void GiveResource(ScriptableResource resource1, int amount) {
        var resource = _resources.Find(x => x.Resource == resource1);
        resource.Amount += amount;
        OnResourceChanged.OnNext(
            new() {
                NewAmount = resource.Amount,
                OldAmount = resource.Amount - amount,
                Resource = resource1,
            }
        );
    }

    #region HumanSystem_Attributes

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

    #endregion

    #region HumanSystem_Properties

    public List<Human> humans => _humans;
    public float humanHeadingDuration => _humanHeadingDuration;
    public float humanHarvestingDuration => _humanHarvestingDuration;
    public float humanReturningBackDuration => _humanReturningBackDuration;
    public float humanTotalHarvestingDuration => _humanTotalHarvestingDuration;

    #endregion

    #region Events

    public Subject<HumanCreatedData> OnHumanCreated { get; } = new();
    public Subject<HumanStateChangedData> OnHumanStateChanged { get; } = new();
    public Subject<HumanPickedUpResourceData> OnHumanPickedUpResource { get; } = new();
    public Subject<HumanPlacedResourceData> OnHumanPlacedResource { get; } = new();

    public Subject<TrainCreatedData> OnTrainCreated { get; } = new();
    public Subject<TrainNodeCreatedData> OnTrainNodeCreated { get; } = new();

    public Subject<TrainNodePickedUpResourceData> OnTrainPickedUpResource { get; } = new();
    public Subject<TopBarResourceChangedData> OnResourceChanged { get; } = new();

    #endregion

    #region MapGeneration

    [Button("Regen With New Seed")]
    void RegenerateTilemapWithNewSeed() {
        _randomSeed += 1;
        RegenerateTilemapAndFireEvent();
    }

    [Button("RegenerateTilemap")]
    void RegenerateTilemapAndFireEvent() {
        RegenerateTilemap();
        OnTerrainTilesRegenerated?.Invoke();
    }

    void RegenerateTilemap() {
        terrainTiles = new();

        // NOTE(Hulvdan): Generating tiles
        for (var y = 0; y < _mapSizeY; y++) {
            var row = new List<TerrainTile>();

            for (var x = 0; x < _mapSizeX; x++) {
                var forestK = MakeSomeNoise2D(_randomSeed, x, y, _forestNoiseScale);
                // var hasForest = false;
                var hasForest = forestK > _forestThreshold;
                var tile = new TerrainTile {
                    Name = "grass",
                    Resource = hasForest ? _logResource : null,
                    ResourceAmount = hasForest ? _maxForestAmount : 0,
                };

                // var randomH = Random.Range(0, _maxHeight + 1);
                var heightK = MakeSomeNoise2D(_randomSeed, x, y, _terrainHeightNoiseScale);
                var randomH = heightK * (_maxHeight + 1);
                tile.Height = Mathf.Min(_maxHeight, (int)randomH);

                row.Add(tile);
            }

            terrainTiles.Add(row);
        }

        for (var y = 0; y < _mapSizeY; y++) {
            for (var x = 0; x < _mapSizeX; x++) {
                var shouldMarkAsCliff =
                    y == 0
                    || terrainTiles[y][x].Height > terrainTiles[y - 1][x].Height;

                if (!shouldMarkAsCliff) {
                    continue;
                }

                var tile = terrainTiles[y][x];
                tile.Name = "cliff";
                tile.Resource = null;
                tile.ResourceAmount = 0;
                terrainTiles[y][x] = tile;
            }
        }
    }

    float MakeSomeNoise2D(int seed, int x, int y, float scale) {
        Noise.Seed = seed;
        return Noise.CalcPixel2D(x, y, scale) / 255f;
    }

    #endregion

    #region HumanSystem_Behaviour

    void CreateHuman(Building building) {
        var human = new Human(Guid.NewGuid(), building, building.position);
        _humans.Add(human);
        // OnHumanCreated?.Invoke(new HumanCreatedData(human));
        OnHumanCreated.OnNext(new(human));
    }

    void UpdateHumans() {
        foreach (var human in _humans) {
            if (human.state != HumanState.Idle) {
                human.harvestingElapsed += Time.deltaTime;

                var newState = human.state;
                if (human.harvestingElapsed >= _humanTotalHarvestingDuration) {
                    newState = HumanState.Idle;
                    if (human.state != newState) {
                        HumanFinishedHeadingBackToTheHarvestBuilding(human);

                        human.harvestingElapsed = 0;

                        HumanStartedIdle(human);
                    }
                }
                else if (
                    human.harvestingElapsed >=
                    _humanHeadingDuration
                    + _humanHarvestingDuration
                    + _humanHeadingToTheStoreBuildingDuration
                ) {
                    newState = HumanState.HeadingBackToTheHarvestBuilding;
                    if (human.state != newState) {
                        HumanFinishedHeadingToTheStoreBuilding(human);

                        PlaceResource(human);
                        human.movingFrom = human.storeBuilding.position;
                        human.storeBuilding.isBooked = false;
                        human.storeBuilding = null;

                        HumanStartedHeadingBackToTheHarvestBuilding(human);
                    }
                }
                else if (
                    human.harvestingElapsed >=
                    _humanHeadingDuration
                    + _humanHarvestingDuration
                ) {
                    newState = HumanState.HeadingToTheStoreBuilding;
                    if (human.state != newState) {
                        HumanFinishedHarvesting(human);

                        PickUpResource(human);
                        var pos = human.harvestTilePosition.Value;
                        terrainTiles[pos.y][pos.x].IsBooked = false;

                        HumanStartedHeadingToTheStoreBuilding(human);
                    }
                }
                else if (human.harvestingElapsed >= _humanHeadingDuration) {
                    newState = HumanState.Harvesting;
                    if (human.state != newState) {
                        HumanFinishedHeadingToTheHarvestTile(human);
                        HumanStartedHarvesting(human);
                    }
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

    void HumanStartedIdle(Human human) {
    }

    void HumanFinishedIdle(Human human) {
    }

    void HumanStartedHeadingToTheHarvestTile(Human human) {
    }

    void HumanFinishedHeadingToTheHarvestTile(Human human) {
    }

    void HumanStartedHarvesting(Human human) {
    }

    void HumanFinishedHarvesting(Human human) {
    }

    void HumanStartedHeadingToTheStoreBuilding(Human human) {
    }

    void HumanFinishedHeadingToTheStoreBuilding(Human human) {
    }

    void HumanStartedHeadingBackToTheHarvestBuilding(Human human) {
    }

    void HumanFinishedHeadingBackToTheHarvestBuilding(Human human) {
    }

    void PickUpResource(Human human) {
        var pos = human.harvestTilePosition.Value;
        var tile = terrainTiles[pos.y][pos.x];
        tile.ResourceAmount -= 1;

        OnHumanPickedUpResource.OnNext(
            new(
                human,
                human.harvestBuilding.scriptableBuilding.harvestableResource,
                pos,
                1,
                tile.ResourceAmount / (float)_maxForestAmount
            )
        );

        if (tile.ResourceAmount <= 0) {
            tile.Resource = null;
        }

        if (tile.ResourceAmount < 0) {
            Debug.LogError("WTF tile.ResourceAmount < 0 ?");
        }
    }

    void PlaceResource(Human human) {
        var tuple = new Tuple<ScriptableResource, int>(
            human.harvestBuilding.scriptableBuilding.harvestableResource, 1
        );
        human.storeBuilding.storedResources.Add(tuple);

        OnHumanPlacedResource.OnNext(
            new(
                1,
                human,
                human.storeBuilding,
                human.harvestBuilding.scriptableBuilding.harvestableResource
            )
        );
    }

    void ChangeHumanState(Human human, HumanState newState) {
        if (newState == human.state) {
            return;
        }

        var oldState = human.state;
        human.state = newState;
        OnHumanStateChanged.OnNext(new(human, oldState, newState));
    }

    void UpdateHumanIdle(Human human) {
        var r = human.harvestBuilding.scriptableBuilding.tilesRadius;
        var leftInclusive = Math.Max(0, human.harvestBuilding.posX - r);
        var rightInclusive = Math.Min(sizeX - 1, human.harvestBuilding.posX + r);
        var topInclusive = Math.Min(sizeY - 1, human.harvestBuilding.posY + r);
        var bottomInclusive = Math.Max(0, human.harvestBuilding.posY - r);

        var resource = human.harvestBuilding.scriptableBuilding.harvestableResource;
        var yy = Enumerable.Range(bottomInclusive, topInclusive - bottomInclusive + 1).ToArray();
        var xx = Enumerable.Range(leftInclusive, rightInclusive - leftInclusive + 1).ToArray();

        Utils.Shuffle(yy, _random);
        Utils.Shuffle(xx, _random);

        Building storeBuildingCandidate = null;
        Vector2Int? tileCandidate = null;

        var shouldBreak = false;
        foreach (var y in yy) {
            foreach (var x in xx) {
                if (!terrainTiles[y][x].IsBooked && terrainTiles[y][x].Resource == resource) {
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

            if (building.storedResources.Count >= building.scriptableBuilding.storeItemsAmount) {
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

            terrainTiles[y][x].IsBooked = true;
            storeBuildingCandidate.isBooked = true;

            HumanFinishedIdle(human);
            ChangeHumanState(human, HumanState.HeadingToTheHarvestTile);
            HumanStartedHeadingToTheHarvestTile(human);
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

    #region TrainSystem_Behaviour

    public bool AreThereAvailableResourcesForTheTrain(HorseTrain train) {
        if (train.CurrentDestination.HasValue == false) {
            Debug.LogError("WTF?");
            return false;
        }

        var pos = train.CurrentDestination.Value.Pos;
        var dimensions = GetStationDimensions(pos);
        var expandedDimensions = ExpandStationDimensions(dimensions);

        foreach (var building in buildings) {
            if (!BetterContains(expandedDimensions, building.position)) {
                continue;
            }

            if (building.storedResources.Count > 0) {
                return true;
            }
        }

        return false;
    }

    bool BetterContains(RectInt rect, Vector2Int pos) {
        return rect.xMin <= pos.x
               && rect.yMin <= pos.y
               && pos.x <= rect.xMax
               && pos.y <= rect.yMax;
    }

    RectInt ExpandStationDimensions(RectInt dimensions) {
        return new() {
            xMin = Math.Max(0, dimensions.xMin - _horsesStationItemsGatheringRadius),
            yMin = Math.Max(0, dimensions.yMin - _horsesStationItemsGatheringRadius),
            xMax = Math.Min(sizeX - 1, dimensions.xMax + _horsesStationItemsGatheringRadius),
            yMax = Math.Min(sizeY - 1, dimensions.yMax + _horsesStationItemsGatheringRadius),
        };
    }

    RectInt GetStationDimensions(Vector2Int pos) {
        var tile = elementTiles[pos.y][pos.x];
        if (tile.Type != ElementTileType.Station) {
            Debug.LogError("WTF?");
            return new() {
                xMin = pos.x,
                yMin = pos.y,
                xMax = pos.x,
                yMax = pos.y,
            };
        }

        var minStationX = pos.x;
        var maxStationX = pos.x;
        var minStationY = pos.y;
        var maxStationY = pos.y;
        if (tile.Rotation == 0) {
            for (var x = pos.x - 1; x >= 0; x--) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                minStationX = x;
            }

            for (var x = pos.x + 1; x < sizeX; x++) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                maxStationX = x;
            }
        }
        else if (tile.Rotation == 1) {
            for (var y = pos.y - 1; y >= 0; y--) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                minStationY = y;
            }

            for (var y = pos.y + 1; y < sizeY; y++) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                maxStationY = y;
            }
        }
        else {
            Debug.LogError("WTF?");
        }

        return new() {
            xMin = minStationX,
            yMin = minStationY,
            xMax = maxStationX,
            yMax = maxStationY,
        };
    }

    public void PickRandomItemForTheTrain(HorseTrain train) {
        TrainNode node1 = null;
        foreach (var node in train.nodes) {
            if (node.canStoreResourceCount > node.storedResources.Count) {
                node1 = node;
                break;
            }
        }

        if (node1 == null) {
            Debug.LogError("WTF?");
            return;
        }

        if (train.CurrentDestination.HasValue == false) {
            Debug.LogError("WTF?");
            return;
        }

        var pos = train.CurrentDestination.Value.Pos;
        var dimensions = GetStationDimensions(pos);
        var expandedDimensions = ExpandStationDimensions(dimensions);

        var shuffledBuildings = buildings.ToArray();
        Utils.Shuffle(shuffledBuildings, _random);

        Building building1 = null;
        ScriptableResource res = null;
        var resIndex = -1;
        foreach (var building in shuffledBuildings) {
            if (!BetterContains(expandedDimensions, building.position)) {
                continue;
            }

            if (building.storedResources.Count > 0) {
                resIndex = building.storedResources.Count - 1;
                building1 = building;
                res = building.storedResources[resIndex].Item1;
                building.storedResources.RemoveAt(resIndex);
                break;
            }
        }

        node1.storedResources.Add(new(res, 1));

        if (res == null) {
            Debug.LogError("WTF?");
            return;
        }

        OnTrainPickedUpResource.OnNext(new() {
            Train = train,
            TrainNode = node1,
            PickedUpAmount = 1,
            Building = building1,
            Resource = res,
            ResourceIndex = resIndex,
        });
    }

    public bool AreThereAvailableSlotsTheTrainCanPassResourcesTo(HorseTrain horse) {
        return false;
    }

    public void PickRandomSlotForTheTrainToPassItemTo(HorseTrain horse) {
    }

    #endregion
}

internal record StationDimensions {
    public int MaxX;
    public int MaxY;
    public int MinX;
    public int MinY;
}
}
