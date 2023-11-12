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
        var dt = _gameManager.dt;
        UpdateHumans(dt);
        UpdateBuildings(dt);
        _horseCompoundSystem.UpdateDt(dt);
    }

    void OnValidate() {
        _humanTotalHarvestingDuration = _humanHeadingDuration
                                        + _humanHarvestingDuration
                                        + _humanHeadingToTheStoreBuildingDuration
                                        + _humanReturningBackDuration;
    }

    public List<TopBarResource> resources { get; } = new();

    public Subject<Vector2Int> onElementTileChanged { get; } = new();

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

        onElementTileChanged.OnNext(pos);
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

    public bool CellContainsPickupableItems(Vector2Int hoveredTile) {
        foreach (var building in buildings) {
            if (
                building.scriptableBuilding.type != BuildingType.Produce
                || building.producedResources.Count <= 0
                || !building.Contains(hoveredTile)
            ) {
                continue;
            }

            var itemsPos = building.position +
                           building.scriptableBuilding.pickupableItemsCellOffset;
            if (hoveredTile == itemsPos) {
                return true;
            }
        }

        return false;
    }

    public void CollectItems(Vector2Int hoveredTile) {
        foreach (var building in buildings) {
            if (
                building.scriptableBuilding.type != BuildingType.Produce
                || building.producedResources.Count <= 0
                || !building.Contains(hoveredTile)
            ) {
                continue;
            }

            var itemsPos = building.position +
                           building.scriptableBuilding.pickupableItemsCellOffset;
            if (hoveredTile != itemsPos) {
                continue;
            }

            var res = building.producedResources[0].script;
            var oldAmount = resources.Find(x => x.Resource == res).Amount;
            var newAmount = oldAmount + building.producedResources.Count;
            resources.Find(x => x.Resource == res).Amount = newAmount;

            onProducedResourcesPickedUp.OnNext(new() {
                Resources = building.producedResources,
                Position = itemsPos,
            });
            building.producedResources.Clear();

            onResourceChanged.OnNext(new() {
                Resource = res,
                OldAmount = oldAmount,
                NewAmount = newAmount,
            });
        }
    }

    public void OnCreateHorse(HorseCreateData data) {
        SpendResources(data.RequiredResources);
        var horse = _horseCompoundSystem.CreateTrain(0, data.Building.position, Direction.Down);
        horse.AddDestination(new() {
            Type = HorseDestinationType.Default,
            Pos = data.Building.position,
        });

        _horseCompoundSystem.TrySetNextDestinationAndBuildPath(horse);
    }

    public int sizeY => _mapSizeY;
    public int sizeX => _mapSizeX;

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < sizeX && y >= 0 && y < sizeY;
    }

    void SpendResources(List<Tuple<int, ScriptableResource>> resourcesToSpend) {
        foreach (var res in resourcesToSpend) {
            foreach (var ress in resources) {
                if (ress.Resource != res.Item2) {
                    continue;
                }

                var oldAmount = ress.Amount;
                ress.Amount = Math.Max(ress.Amount - res.Item1, 0);

                onResourceChanged.OnNext(new() {
                    Resource = res.Item2,
                    OldAmount = oldAmount,
                    NewAmount = ress.Amount,
                });
                break;
            }
        }
    }

    void UpdateBuildings(float dt) {
        foreach (var building in buildings) {
            var scriptableBuilding = building.scriptableBuilding;
            if (scriptableBuilding.type == BuildingType.Produce) {
                if (!building.IsProcessing) {
                    if (building.storedResources.Count > 0 && building.CanStartProcessing()) {
                        building.IsProcessing = true;
                        building.ProcessingElapsed = 0;

                        var res = building.storedResources[0];
                        building.storedResources.RemoveAt(0);

                        onBuildingStartedProcessing.OnNext(new() {
                            Resource = res,
                            Building = building,
                        });
                    }
                }

                if (building.IsProcessing) {
                    building.ProcessingElapsed += dt;

                    if (building.ProcessingElapsed >= scriptableBuilding.ItemProcessingDuration) {
                        building.IsProcessing = false;
                        Produce(building);
                    }
                }
            }
        }
    }

    void Produce(Building building) {
        Debug.Log("Produced!");
        var res = building.scriptableBuilding.produces;
        var resourceObj = new ResourceObj(Guid.NewGuid(), res);
        building.producedResources.Add(resourceObj);

        onBuildingProducedItem.OnNext(new() {
            Resource = resourceObj,
            ProducedAmount = 1,
            Building = building,
        });
    }

    public void InitDependencies(GameManager gameManager) {
        _gameManager = gameManager;
        _horseCompoundSystem.InitDependencies(gameManager);
    }

    public void Init() {
        _initialMapProvider.Init(this, this);

        RegenerateTilemap();

        foreach (var res in _topBarResources) {
            resources.Add(new() { Amount = 0, Resource = res });
        }

        elementTiles = _initialMapProvider.LoadElementTiles();
        OnTerrainTilesRegenerated?.Invoke();

        _horseCompoundSystem.Init(this, this);

        // CreateHuman(_buildings[0]);
        CreateHuman(_buildings[0]);
        CreateHuman(_buildings[1]);
        CreateHuman(_buildings[1]);
    }

    void GiveResource(ScriptableResource resource1, int amount) {
        var resource = resources.Find(x => x.Resource == resource1);
        resource.Amount += amount;
        onResourceChanged.OnNext(
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

    public Subject<E_HumanCreated> onHumanCreated { get; } = new();
    public Subject<E_HumanStateChanged> onHumanStateChanged { get; } = new();
    public Subject<E_HumanPickedUpResource> onHumanPickedUpResource { get; } = new();
    public Subject<E_HumanPlacedResource> onHumanPlacedResource { get; } = new();

    public Subject<E_TrainCreated> onTrainCreated { get; } = new();
    public Subject<E_TrainNodeCreated> onTrainNodeCreated { get; } = new();

    public Subject<E_TrainPickedUpResource> onTrainPickedUpResource { get; } = new();
    public Subject<E_TrainPushedResource> onTrainPushedResource { get; } = new();

    public Subject<E_BuildingStartedProcessing> onBuildingStartedProcessing { get; } = new();
    public Subject<E_BuildingProducedItem> onBuildingProducedItem { get; } = new();
    public Subject<E_ProducedResourcesPickedUp> onProducedResourcesPickedUp { get; } = new();

    public Subject<E_TopBarResourceChanged> onResourceChanged { get; } = new();

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
        onHumanCreated.OnNext(new(human));
    }

    void UpdateHumans(float dt) {
        foreach (var human in _humans) {
            if (human.state != HumanState.Idle) {
                human.harvestingElapsed += dt;

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

        var res = human.harvestBuilding.scriptableBuilding.harvestableResource;
        onHumanPickedUpResource.OnNext(
            new() {
                Human = human,
                Resource = new(Guid.NewGuid(), res),
                ResourceTilePosition = pos,
                PickedUpAmount = 1,
                RemainingAmountPercent = tile.ResourceAmount / (float)_maxForestAmount,
            }
        );

        if (tile.ResourceAmount <= 0) {
            tile.Resource = null;
        }

        if (tile.ResourceAmount < 0) {
            Debug.LogError("WTF tile.ResourceAmount < 0 ?");
        }
    }

    void PlaceResource(Human human) {
        var scriptableResource = human.harvestBuilding.scriptableBuilding.harvestableResource;
        var resource = new ResourceObj(Guid.NewGuid(), scriptableResource);
        human.storeBuilding.storedResources.Add(resource);

        onHumanPlacedResource.OnNext(
            new() {
                Amount = 1,
                Human = human,
                StoreBuilding = human.storeBuilding,
                Resource = resource,
            }
        );
    }

    void ChangeHumanState(Human human, HumanState newState) {
        if (newState == human.state) {
            return;
        }

        var oldState = human.state;
        human.state = newState;
        onHumanStateChanged.OnNext(new(human, oldState, newState));
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
            if (building.scriptableBuilding.type != BuildingType.Store) {
                continue;
            }

            if (!Intersect(expandedDimensions, building.rect)) {
                continue;
            }

            if (building.storedResources.Count > 0) {
                return true;
            }
        }

        return false;
    }

    bool Intersect(RectInt rect1, RectInt rect2) {
        return rect1.xMin < rect2.xMax
               && rect1.xMax > rect2.xMin
               && rect1.yMin < rect2.yMax
               && rect1.yMax > rect2.yMin;
    }

    RectInt ExpandStationDimensions(RectInt dimensions) {
        return new() {
            xMin = Math.Max(0, dimensions.xMin - _horsesStationItemsGatheringRadius),
            yMin = Math.Max(0, dimensions.yMin - _horsesStationItemsGatheringRadius),
            xMax = Math.Min(sizeX, dimensions.xMax + _horsesStationItemsGatheringRadius),
            yMax = Math.Min(sizeY, dimensions.yMax + _horsesStationItemsGatheringRadius),
        };
    }

    RectInt GetStationDimensions(Vector2Int pos) {
        var tile = elementTiles[pos.y][pos.x];
        if (tile.Type != ElementTileType.Station) {
            Debug.LogError("WTF?");
            return new(pos.x, pos.y, 1, 1);
        }

        var width = 1;
        var height = 1;
        var leftX = pos.x;
        var bottomY = pos.y;
        if (tile.Rotation == 0) {
            for (var x = pos.x - 1; x >= 0; x--) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                width += 1;
                leftX = x;
            }

            for (var x = pos.x + 1; x < sizeX; x++) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                width += 1;
            }
        }
        else if (tile.Rotation == 1) {
            for (var y = pos.y - 1; y >= 0; y--) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 1) {
                    break;
                }

                bottomY = y;
                height += 1;
            }

            for (var y = pos.y + 1; y < sizeY; y++) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 1) {
                    break;
                }

                height += 1;
            }
        }
        else {
            Debug.LogError("WTF?");
        }

        return new(leftX, bottomY, width, height);
    }

    public void PickRandomItemForTheTrain(HorseTrain horse) {
        TrainNode foundNode = null;
        foreach (var node in horse.nodes) {
            if (node.canStoreResourceCount > node.storedResources.Count) {
                foundNode = node;
                break;
            }
        }

        if (foundNode == null) {
            Debug.LogError("WTF?");
            return;
        }

        if (horse.CurrentDestination.HasValue == false) {
            Debug.LogError("WTF?");
            return;
        }

        var pos = horse.CurrentDestination.Value.Pos;
        var dimensions = GetStationDimensions(pos);
        var expandedDimensions = ExpandStationDimensions(dimensions);

        var shuffledBuildings = buildings.ToArray();
        Utils.Shuffle(shuffledBuildings, _random);

        Building foundBuilding = null;
        ResourceObj foundResource = null;
        var foundResourceIndex = -1;
        var resourceSlotPosition = Vector2.zero;
        foreach (var building in shuffledBuildings) {
            if (building.scriptableBuilding.type != BuildingType.Store) {
                continue;
            }

            if (!Intersect(expandedDimensions, building.rect)) {
                continue;
            }

            if (building.storedResources.Count == 0) {
                continue;
            }

            foundResourceIndex = building.storedResources.Count - 1;
            foundBuilding = building;
            foundResource = building.storedResources[foundResourceIndex];
            resourceSlotPosition = building.scriptableBuilding.storedItemPositions[
                                       foundResourceIndex % building.scriptableBuilding
                                           .storedItemPositions.Count] +
                                   building.position;
            building.storedResources.RemoveAt(foundResourceIndex);
            break;
        }

        if (foundResource == null) {
            Debug.LogError("WTF?");
            return;
        }

        foundNode.storedResources.Add(foundResource);

        onTrainPickedUpResource.OnNext(new() {
            Train = horse,
            TrainNode = foundNode,
            PickedUpAmount = 1,
            Building = foundBuilding,
            Resource = foundResource,
            ResourceSlotPosition = resourceSlotPosition,
        });
    }

    public bool AreThereAvailableSlotsTheTrainCanPassResourcesTo(HorseTrain horse) {
        if (horse.CurrentDestination.HasValue == false) {
            Debug.LogError("WTF?");
            return false;
        }

        var pos = horse.CurrentDestination.Value.Pos;
        var dimensions = GetStationDimensions(pos);
        var expandedDimensions = ExpandStationDimensions(dimensions);

        foreach (var building in buildings) {
            if (building.scriptableBuilding.type != BuildingType.Produce) {
                continue;
            }

            if (!Intersect(expandedDimensions, building.rect)) {
                continue;
            }

            if (building.CanStoreResource()) {
                return true;
            }
        }

        return false;
    }

    public void PickRandomSlotForTheTrainToPassItemTo(HorseTrain horse) {
        if (horse.CurrentDestination.HasValue == false) {
            Debug.LogError("WTF?");
            return;
        }

        var pos = horse.CurrentDestination.Value.Pos;
        var dimensions = GetStationDimensions(pos);
        var expandedDimensions = ExpandStationDimensions(dimensions);

        var shuffledBuildings = buildings.ToArray();
        Utils.Shuffle(shuffledBuildings, _random);

        Building foundBuilding = null;
        foreach (var building in buildings) {
            if (building.scriptableBuilding.type != BuildingType.Produce) {
                continue;
            }

            if (!Intersect(expandedDimensions, building.rect)) {
                continue;
            }

            if (building.CanStoreResource()) {
                foundBuilding = building;
                break;
            }
        }

        if (foundBuilding == null) {
            Debug.LogError("WTF?");
            return;
        }

        TrainNode foundNode = null;
        foreach (var node in horse.nodes) {
            if (node.storedResources.Count > 0) {
                foundNode = node;
                break;
            }
        }

        if (foundNode == null) {
            Debug.LogError("WTF?");
            return;
        }

        var foundResourceIndex = foundNode.storedResources.Count - 1;
        var foundResource = foundNode.storedResources[foundResourceIndex];
        var res = foundBuilding.StoreResource(foundResource);
        foundNode.storedResources.RemoveAt(foundResourceIndex);

        onTrainPushedResource.OnNext(new() {
            Train = horse,
            TrainNode = foundNode,
            PickedUpAmount = 1,
            Building = foundBuilding,
            Resource = foundResource,
            StoreResourceResult = res,
        });
    }

    #endregion
}
}
