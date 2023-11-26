#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using BFG.Core;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
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
    public UnityEvent OnTerrainTilesRegenerated = null!;

    [FormerlySerializedAs("_buildings")]
    [FoldoutGroup("Setup", true)]
    [SerializeField]
    List<BuildingGO> _buildingGameObjects = null!;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    readonly List<Human> _humans = new();

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    ScriptableResource _logResource = null!;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    ScriptableResource _planksResource = null!;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    List<ScriptableResource> _topBarResources = null!;

    [FoldoutGroup("Setup", true)]
    [SerializeField]
    [Required]
    InitialMapProvider _initialMapProvider = null!;

    [SerializeField]
    [Required]
    ScriptableBuilding _lumberjacksHouse = null!;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    bool _hideEditorLogs;

    [FormerlySerializedAs("_compoundSystem")]
    [FormerlySerializedAs("_movementSystemInterface")]
    [FoldoutGroup("Horse Movement System", true)]
    [SerializeField]
    [Required]
    HorseCompoundSystem _horseCompoundSystem = null!;

    [FormerlySerializedAs("_horsesGatheringAroundStationRadius")]
    [FoldoutGroup("Horse Movement System", true)]
    [SerializeField]
    [Min(1)]
    int _horsesStationItemsGatheringRadius = 2;

    GameManager _gameManager = null!;

    [FoldoutGroup("Humans", true)]
    [ShowInInspector]
    [ReadOnly]
    float _humanTotalHarvestingDuration;

    [FoldoutGroup("Humans", true)]
    [ShowInInspector]
    [ReadOnly]
    [Min(0.01f)]
    float _humanTransporterMovingOneCellDuration = 1f;

    Random _random = null!;

    public List<GraphSegment> segments { get; } = new();

    readonly Dictionary<Guid, List<GraphSegment>> _segmentLinks = new();

    void Awake() {
        _random = new((int)Time.time);
    }

    void Update() {
        var dt = _gameManager.dt;
        _itemTransportationSystem.PathfindItemsInQueue();
        UpdateHumans(dt);
        UpdateHumanTransporters(dt);
        UpdateBuildings(dt);
        // _horseCompoundSystem.UpdateDt(dt);
    }

    void OnValidate() {
        _humanTotalHarvestingDuration = _humanHeadingDuration
                                        + _humanHarvestingDuration
                                        + _humanHeadingToTheStoreBuildingDuration
                                        + _humanReturningBackDuration;
    }

    public List<TopBarResource> resources { get; } = new();

    // List<MapResource> _mapResources = new();

    public Subject<Vector2Int> onElementTileChanged { get; } = new();
    public Subject<E_BuildingPlaced> onBuildingPlaced { get; } = new();

    // NOTE(Hulvdan): Indexes start from the bottom left corner and go to the top right one
    public List<List<ElementTile>> elementTiles { get; private set; } = null!;
    public List<List<TerrainTile>> terrainTiles { get; private set; } = null!;

    public List<Building> buildings { get; private set; } = new();

    public List<List<List<MapResource>>> mapResources { get; private set; } = null!;

    public void Init() {
        _initialMapProvider.Init(this, this);
        _humanTransporterController = new(this, this, cityHall, _itemTransportationSystem);

        resources.Clear();
        foreach (var res in _topBarResources) {
            resources.Add(new() { Amount = 0, Resource = res });
        }

        if (Application.isPlaying) {
            RegenerateTilemap();
            OnTerrainTilesRegenerated.Invoke();
        }

        // _horseCompoundSystem.Init(this, this);

        // CreateHuman(_buildings[0]);
        // CreateHuman(_buildings[0]);
        // CreateHuman(_buildings[1]);
        // CreateHuman(_buildings[1]);

        mapResources = new() { Capacity = height };
        for (var y = 0; y < height; y++) {
            var row = new List<List<MapResource>> { Capacity = width };
            for (var x = 0; x < width; x++) {
                row.Add(new());
            }

            mapResources.Add(row);
        }

        for (var i = 0; i < 15; i++) {
            AddMapResource(cityHall, _planksResource);
        }

        _itemTransportationSystem = new(this, this);

        if (Application.isPlaying) {
            TryBuild(new(7, 7), new() { Type = SelectedItemType.Road });
            TryBuild(new(6, 7), new() { Type = SelectedItemType.Road });
            TryBuild(new(6, 7), new() { Type = SelectedItemType.Flag });
            TryBuild(new(6, 6), new() { Type = SelectedItemType.Road });
            TryBuild(
                new(5, 6),
                new() {
                    Type = SelectedItemType.Building,
                    Building = _lumberjacksHouse,
                }
            );

            // TryBuild(new(7, 7), new() { Type = SelectedItemType.Road });
            // TryBuild(
            //     new(6, 7),
            //     new() {
            //         Type = SelectedItemType.Building,
            //         Building = _lumberjacksHouse,
            //     }
            // );
        }
    }

    void AddMapResource(Building building, ScriptableResource scriptable) {
        mapResources[building.posY][building.posX].Add(new(cityHall.pos, scriptable));
    }

    public void InitDependencies(GameManager gameManager) {
        _gameManager = gameManager;
        // _horseCompoundSystem.InitDependencies(gameManager);

        buildings = _buildingGameObjects.Select(i => i.IntoBuilding()).ToList();
    }

    public void TryBuild(Vector2Int pos, SelectedItem item) {
        if (!Contains(pos)) {
            return;
        }

        using var _ = Tracing.Scope();

        if (item.Type == SelectedItemType.Road) {
            var road = elementTiles[pos.y][pos.x];
            road.Type = ElementTileType.Road;
            elementTiles[pos.y][pos.x] = road;

            onElementTileChanged.OnNext(pos);

            var res = ItemTransportationGraph.OnTilesUpdated(
                elementTiles,
                this,
                buildings,
                new() { new(TileUpdatedType.RoadPlaced, pos) },
                segments
            );
            UpdateSegments(res);
        }
        else if (item.Type == SelectedItemType.Station) {
            var tile = elementTiles[pos.y][pos.x];
            tile.Type = ElementTileType.Station;
            tile.Rotation = _gameManager.selectedItemRotation == 0 ? 1 : 0;
            elementTiles[pos.y][pos.x] = tile;

            onElementTileChanged.OnNext(pos);
        }
        else if (item.Type == SelectedItemType.Building) {
            var building = new Building(new(), item.Building, pos, 0);
            buildings.Add(building);

            for (var dy = 0; dy < building.scriptable.size.y; dy++) {
                for (var dx = 0; dx < building.scriptable.size.x; dx++) {
                    elementTiles[pos.y + dy][pos.x + dx] = new(ElementTileType.Building, building);
                }
            }

            foreach (var resource in building.scriptable.requiredResourcesToBuild) {
                for (var i = 0; i < resource.Number; i++) {
                    building.ResourcesToBook.Add(new() {
                        ID = Guid.NewGuid(),
                        Building = building,
                        BookingType = MapResourceBookingType.Construction,
                        Priority = 1,
                        Scriptable = resource.Scriptable,
                    });
                }
            }

            onBuildingPlaced.OnNext(new() { Building = building });

            var res = ItemTransportationGraph.OnTilesUpdated(
                elementTiles,
                this,
                buildings,
                new() { new(TileUpdatedType.BuildingPlaced, pos) },
                segments
            );

            UpdateBuilding_NotConstructed(0, building);
            UpdateSegments(res);
        }
        else if (item.Type == SelectedItemType.Flag) {
            if (elementTiles[pos.y][pos.x].Type != ElementTileType.Road) {
                return;
            }

            elementTiles[pos.y][pos.x] = ElementTile.Flag;
            onElementTileChanged.OnNext(pos);

            var res = ItemTransportationGraph.OnTilesUpdated(
                elementTiles,
                this,
                buildings,
                new() { new(TileUpdatedType.FlagPlaced, pos) },
                segments
            );
            UpdateSegments(res);
        }
    }

    public bool CanBePlaced(Vector2Int pos, SelectedItemType itemType) {
        if (!Contains(pos.x, pos.y)) {
            Debug.LogError("WTF?");
            return false;
        }

        switch (itemType) {
            case SelectedItemType.Road:
            case SelectedItemType.Building:
                return IsBuildable(pos);
            case SelectedItemType.Flag:
                return elementTiles[pos.y][pos.x].Type == ElementTileType.Road;
            default:
                return false;
        }
    }

    void UpdateSegments(ItemTransportationGraph.OnTilesUpdatedResult res) {
        using var _ = Tracing.Scope();

        if (!_hideEditorLogs) {
            Debug.Log($"{res.AddedSegments.Count} segments added, {res.DeletedSegments} deleted");
        }

        var humansMovingToCityHall = 0;
        foreach (var human in _humanTransporters) {
            var state = HumanTransporter_MovingInTheWorld_Controller.State.MovingToTheCityHall;
            if (human.stateMovingInTheWorld == state) {
                humansMovingToCityHall++;
            }
        }

        Stack<Tuple<GraphSegment?, HumanTransporter>> humansThatNeedNewSegment =
            new(res.DeletedSegments.Count + humansMovingToCityHall);
        foreach (var human in _humanTransporters) {
            var state = HumanTransporter_MovingInTheWorld_Controller.State.MovingToTheCityHall;
            if (human.stateMovingInTheWorld == state) {
                humansThatNeedNewSegment.Push(new(null, human));
            }
        }

        foreach (var segment in res.DeletedSegments) {
            segments.RemoveAt(segments.FindIndex(i => i.ID == segment.ID));

            var human = segment.AssignedHuman;
            if (human != null) {
                human.segment = null;
                segment.AssignedHuman = null;
                humansThatNeedNewSegment.Push(new(segment, human));
            }

            foreach (var linkedSegment in segment.LinkedSegments) {
                linkedSegment.Unlink(segment);
            }

            _itemTransportationSystem.OnSegmentDeleted(segment);
        }

        if (!_hideEditorLogs) {
            Debug.Log(
                $"{humansThatNeedNewSegment.Count} HumanTransporters need to find new segments"
            );
        }

        foreach (var segment in res.AddedSegments) {
            segments.Add(segment);

            foreach (var segmentToLink in segments) {
                if (ReferenceEquals(segment, segmentToLink)) {
                    continue;
                }

                // Mb there Graph.CollidesWith(other.Graph) is needed for optimization
                if (segmentToLink.HasSomeOfTheSameVertexes(segment)) {
                    segment.Link(segmentToLink);
                    segmentToLink.Link(segment);
                }
            }
        }

        _itemTransportationSystem.PathfindItemsInQueue();
        Tracing.Log("_itemTransportationSystem.PathfindItemsInQueue()");

        foreach (var segment in res.AddedSegments) {
            if (humansThatNeedNewSegment.Count == 0) {
                CreateHuman_Transporter(cityHall, segment);
            }
            else {
                var (oldSegment, human) = humansThatNeedNewSegment.Pop();
                human.segment = segment;
                segment.AssignedHuman = human;
                _humanTransporterController.OnSegmentChanged(human, oldSegment);
            }
        }

        foreach (var (oldSegment, human) in humansThatNeedNewSegment) {
            _humanTransporterController.OnSegmentChanged(human, oldSegment);
        }

        // Assert that segments don't have tiles with identical directions
        for (var i = 0; i < segments.Count; i++) {
            for (var j = 0; j < segments.Count; j++) {
                if (i == j) {
                    continue;
                }

                var g1 = segments[i].Graph;
                var g2 = segments[j].Graph;
                for (var y = 0; y < g1.height; y++) {
                    for (var x = 0; x < g1.width; x++) {
                        // ReSharper disable once InconsistentNaming
                        var g1x = x + g1.Offset.x;
                        // ReSharper disable once InconsistentNaming
                        var g1y = y + g1.Offset.y;
                        if (!g2.Contains(g1x, g1y)) {
                            continue;
                        }

                        // ReSharper disable once InconsistentNaming
                        var g2y = g1y - g2.Offset.y;
                        // ReSharper disable once InconsistentNaming
                        var g2x = g1x - g2.Offset.x;
                        var node = g2.Nodes[g2y][g2x];

                        Assert.AreEqual(node & g1.Nodes[y][x], 0);
                    }
                }
            }
        }
    }

    public PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        bool avoidHarvestableResources
    ) {
        if (source == destination) {
            return new() {
                Path = new() { Capacity = 0 },
                Success = true,
            };
        }

        var queue = new Queue<Vector2Int>();
        queue.Enqueue(new(source.x, source.y));

        var minX = source.x;
        var minY = source.y;
        var maxX = source.x;
        var maxY = source.y;
        var t = elementTiles[source.y][source.x];
        t.BFS_Visited = true;
        elementTiles[source.y][source.x] = t;

        while (queue.Count > 0) {
            var pos = queue.Dequeue();
            // var tile = elementTiles[pos.y][pos.x];

            foreach (var dir in Utils.Directions) {
                var offset = dir.AsOffset();
                var newY = pos.y + offset.y;
                var newX = pos.x + offset.x;
                if (!Contains(newX, newY)) {
                    continue;
                }

                var mTile = elementTiles[newY][newX];
                if (mTile.BFS_Visited) {
                    continue;
                }

                if (avoidHarvestableResources) {
                    var terrainTile = terrainTiles[newY][newX];
                    if (terrainTile.ResourceAmount > 0) {
                        continue;
                    }
                }

                var newPos = new Vector2Int(newX, newY);

                mTile.BFS_Visited = true;
                mTile.BFS_Parent = pos;
                elementTiles[newY][newX] = mTile;

                minX = Math.Min(newX, minX);
                maxX = Math.Max(newX, maxX);
                minY = Math.Min(newY, minY);
                maxY = Math.Max(newY, maxY);

                if (newPos == destination) {
                    var res = BuildPath(elementTiles, newPos);
                    ClearBFSCache(minY, maxY, minX, maxX);
                    return res;
                }

                queue.Enqueue(newPos);
            }
        }

        ClearBFSCache(minY, maxY, minX, maxX);
        return new(false, null);
    }

    // ReSharper disable once InconsistentNaming
    void ClearBFSCache(int minY, int maxY, int minX, int maxX) {
        for (var y = minY; y <= maxY; y++) {
            for (var x = minX; x <= maxX; x++) {
                var node = elementTiles[y][x];
                node.BFS_Parent = null;
                node.BFS_Visited = false;
                elementTiles[y][x] = node;
            }
        }
    }

    static PathFindResult BuildPath(
        List<List<ElementTile>> graph,
        Vector2Int destination
    ) {
        var res = new List<Vector2Int> { destination };

        while (graph[destination.y][destination.x].BFS_Parent != null) {
            res.Add(graph[destination.y][destination.x].BFS_Parent!.Value);
            destination = graph[destination.y][destination.x].BFS_Parent!.Value;
        }

        res.Reverse();
        return new(true, res);
    }

    public bool IsBuildable(int x, int y) {
        if (!Contains(x, y)) {
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
                building.scriptable.type != BuildingType.Produce
                || building.producedResources.Count <= 0
                || !building.Contains(hoveredTile)
            ) {
                continue;
            }

            var itemsPos = building.pos +
                           building.scriptable.pickupableItemsCellOffset;
            if (hoveredTile == itemsPos) {
                return true;
            }
        }

        return false;
    }

    public void CollectItems(Vector2Int hoveredTile) {
        foreach (var building in buildings) {
            if (
                building.scriptable.type != BuildingType.Produce
                || building.producedResources.Count <= 0
                || !building.Contains(hoveredTile)
            ) {
                continue;
            }

            var itemsPos = building.pos +
                           building.scriptable.pickupableItemsCellOffset;
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
        var horse = _horseCompoundSystem.CreateTrain(0, data.Building.pos, Direction.Down);
        horse.AddDestination(new() {
            Type = HorseDestinationType.Default,
            Pos = data.Building.pos,
        });

        _horseCompoundSystem.TrySetNextDestinationAndBuildPath(horse);
    }

    public int height => _mapSizeY;
    public int width => _mapSizeX;

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
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
            if (building.BuildingProgress < 1) {
                UpdateBuilding_NotConstructed(dt, building);
            }
            else {
                UpdateBuilding_Production(dt, building);
            }
        }
    }

    void UpdateBuilding_NotConstructed(float dt, Building building) {
        if (building.ResourcesToBook.Count > 0) {
            _itemTransportationSystem.Add_ResourcesToBook(building.ResourcesToBook);
            building.ResourcesToBook.Clear();
        }
    }

    void UpdateBuilding_Production(float dt, Building building) {
        var scriptableBuilding = building.scriptable;

        if (scriptableBuilding.type == BuildingType.Produce) {
            if (!building.IsProducing) {
                if (building.storedResources.Count > 0 && building.CanStartProcessing()) {
                    building.IsProducing = true;
                    building.ProducingElapsed = 0;

                    var res = building.storedResources[0];
                    building.storedResources.RemoveAt(0);

                    onBuildingStartedProcessing.OnNext(new() {
                        Resource = res,
                        Building = building,
                    });
                }
            }

            if (building.IsProducing) {
                building.ProducingElapsed += dt;

                if (building.ProducingElapsed >= scriptableBuilding.ItemProcessingDuration) {
                    building.IsProducing = false;
                    Produce(building);
                }
            }
        }
    }

    void Produce(Building building) {
        Debug.Log("Produced!");
        var res = building.scriptable.produces;
        var resourceObj = new ResourceObj(Guid.NewGuid(), res);
        building.producedResources.Add(resourceObj);

        onBuildingProducedItem.OnNext(new() {
            Resource = resourceObj,
            ProducedAmount = 1,
            Building = building,
        });
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

    Building cityHall => buildings.Find(i => i.scriptable.type == BuildingType.SpecialCityHall);

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

    readonly List<HumanTransporter> _humanTransporters = new();

    public float humanTransporterMovingOneCellDuration => _humanTransporterMovingOneCellDuration;

    #endregion

    #region Events

    public Subject<E_HumanCreated> onHumanCreated { get; } = new();
    public Subject<E_HumanTransporterCreated> onHumanTransporterCreated { get; } = new();
    public Subject<E_HumanStateChanged> onHumanStateChanged { get; } = new();
    public Subject<E_HumanPickedUpResource> onHumanPickedUpResource { get; } = new();
    public Subject<E_HumanPlacedResource> onHumanPlacedResource { get; } = new();

    public Subject<E_HumanReachedCityHall> onHumanReachedCityHall { get; } = new();

    public Subject<E_HumanTransportedStartedPickingUpResource>
        onHumanTransporterStartedPickingUpResource { get; } =
        new();

    public Subject<E_HumanTransporterPickedUpResource> onHumanTransporterPickedUpResource { get; } =
        new();

    public Subject<E_HumanTransporterStartedPlacingResource>
        onHumanTransporterStartedPlacingResource { get; } =
        new();

    public Subject<E_HumanTransporterPlacedResource> onHumanTransporterPlacedResource { get; } =
        new();

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
        OnTerrainTilesRegenerated.Invoke();
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

        elementTiles = _initialMapProvider.LoadElementTiles();

        var cityHalls = buildings.FindAll(i => i.scriptable.type == BuildingType.SpecialCityHall);
        foreach (var building in cityHalls) {
            var pos = building.pos;
            elementTiles[pos.y][pos.x] = new(ElementTileType.Building, building);
        }
    }

    float MakeSomeNoise2D(int seed, int x, int y, float scale) {
        Noise.Seed = seed;
        return Noise.CalcPixel2D(x, y, scale) / 255f;
    }

    #endregion

    #region HumanSystem_Behaviour

    void CreateHuman_Transporter(Building building, GraphSegment segment) {
        var human = new HumanTransporter(Guid.NewGuid(), segment, building.pos);
        segment.AssignedHuman = human;
        _humanTransporters.Add(human);

        _humanTransporterController.SetState(human, HumanTransporterState.MovingInTheWorld);
        onHumanTransporterCreated.OnNext(new() { Human = human });
    }

    void CreateHuman(Building building) {
        var human = new Human(Guid.NewGuid(), building, building.pos);
        _humans.Add(human);
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
                        human.movingFrom = human.storeBuilding!.pos;
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
                        var pos = human.harvestTilePosition!.Value;
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
        var pos = human.harvestTilePosition!.Value;
        var tile = terrainTiles[pos.y][pos.x];
        tile.ResourceAmount -= 1;

        var res = human.building!.scriptable.harvestableResource;
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
        var scriptableResource = human.building!.scriptable.harvestableResource;
        var resource = new ResourceObj(Guid.NewGuid(), scriptableResource);
        human.storeBuilding!.storedResources.Add(resource);

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
        var r = human.building!.scriptable.tilesRadius;
        var leftInclusive = Math.Max(0, human.building.posX - r);
        var rightInclusive = Math.Min(width - 1, human.building.posX + r);
        var topInclusive = Math.Min(height - 1, human.building.posY + r);
        var bottomInclusive = Math.Max(0, human.building.posY - r);

        var resource = human.building.scriptable.harvestableResource;
        var yy = Enumerable.Range(bottomInclusive, topInclusive - bottomInclusive + 1).ToArray();
        var xx = Enumerable.Range(leftInclusive, rightInclusive - leftInclusive + 1).ToArray();

        Utils.Shuffle(yy, _random);
        Utils.Shuffle(xx, _random);

        Building? storeBuildingCandidate = null;
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
            if (building.scriptable.type != BuildingType.Store) {
                continue;
            }

            if (building.isBooked) {
                continue;
            }

            if (building.storedResources.Count >= building.scriptable.storeItemsAmount) {
                continue;
            }

            var isWithinRadius = leftInclusive <= building.pos.x
                                 && building.pos.x <= rightInclusive
                                 && bottomInclusive <= building.pos.y
                                 && building.pos.y <= topInclusive;
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
        human.position = Vector2.Lerp(human.building!.pos, human.harvestTilePosition!.Value, mt);
    }

    void UpdateHumanHarvesting(Human human) {
    }

    void UpdateHumanHeadingToTheStoreBuilding(Human human) {
        var stateElapsed = human.harvestingElapsed
                           - _humanHeadingDuration
                           - _humanHarvestingDuration;
        var t = stateElapsed / _humanHeadingToTheStoreBuildingDuration;
        var mt = _humanMovementCurve.Evaluate(t);
        human.position = Vector2.Lerp(
            human.harvestTilePosition!.Value, human.storeBuilding!.pos, mt
        );
    }

    void UpdateHumanHeadingBackToTheHarvestBuilding(Human human) {
        var stateElapsed = human.harvestingElapsed
                           - _humanHeadingDuration
                           - _humanHarvestingDuration
                           - _humanHeadingToTheStoreBuildingDuration;
        var t = stateElapsed / _humanReturningBackDuration;
        var mt = _humanMovementCurve.Evaluate(t);
        human.position = Vector2.Lerp(human.movingFrom, human.building!.pos, mt);
    }

    void UpdateHumanTransporters(float dt) {
        // ReSharper disable once InconsistentNaming
        const int GUARD_MAX_ITERATIONS_COUNT = 256;

        var humansToRemove = new List<HumanTransporter>();
        foreach (var human in _humanTransporters) {
            if (human.movingTo != null) {
                human.movingElapsed += dt;

                var iteration = 0;
                while (
                    iteration++ < 10 * GUARD_MAX_ITERATIONS_COUNT
                    && human.movingTo != null
                    && human.movingElapsed > humanTransporterMovingOneCellDuration
                ) {
                    using var _ = Tracing.Scope();
                    Tracing.Log("Human reached the next tile");

                    human.movingElapsed -= humanTransporterMovingOneCellDuration;

                    human.pos = human.movingTo.Value;
                    human.movingFrom = human.pos;
                    human.PopMovingTo();

                    _humanTransporterController.OnHumanMovedToTheNextTile(human);
                }

                Assert.IsTrue(iteration < 10 * GUARD_MAX_ITERATIONS_COUNT);
                if (iteration >= GUARD_MAX_ITERATIONS_COUNT) {
                    Debug.LogWarning("WTF?");
                }

                human.movingNormalized = Mathf.Min(
                    1, human.movingElapsed / _humanTransporterMovingOneCellDuration
                );
            }

            _humanTransporterController.Update(human, dt);
            var state = HumanTransporter_MovingInTheWorld_Controller.State.MovingToTheCityHall;
            if (
                human.stateMovingInTheWorld == state
                && human.pos == cityHall.pos
                && human.movingTo == null
            ) {
                humansToRemove.Add(human);
            }
        }

        foreach (var human in humansToRemove) {
            onHumanReachedCityHall.OnNext(new() { Human = human });
            _humanTransporters.RemoveAt(_humanTransporters.FindIndex(i => i == human));
        }
    }

    #endregion

    #region ItemTransportationSystem

    ItemTransportationSystem _itemTransportationSystem = null!;
    HumanTransporter_Controller _humanTransporterController = null!;

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
            if (building.scriptable.type != BuildingType.Store) {
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
            xMax = Math.Min(width, dimensions.xMax + _horsesStationItemsGatheringRadius),
            yMax = Math.Min(height, dimensions.yMax + _horsesStationItemsGatheringRadius),
        };
    }

    RectInt GetStationDimensions(Vector2Int pos) {
        var tile = elementTiles[pos.y][pos.x];
        if (tile.Type != ElementTileType.Station) {
            Debug.LogError("WTF?");
            return new(pos.x, pos.y, 1, 1);
        }

        var w = 1;
        var h = 1;
        var leftX = pos.x;
        var bottomY = pos.y;
        if (tile.Rotation == 0) {
            for (var x = pos.x - 1; x >= 0; x--) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                w += 1;
                leftX = x;
            }

            for (var x = pos.x + 1; x < width; x++) {
                var newTile = elementTiles[pos.y][x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 0) {
                    break;
                }

                w += 1;
            }
        }
        else if (tile.Rotation == 1) {
            for (var y = pos.y - 1; y >= 0; y--) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 1) {
                    break;
                }

                bottomY = y;
                h += 1;
            }

            for (var y = pos.y + 1; y < height; y++) {
                var newTile = elementTiles[y][pos.x];
                if (newTile.Type != ElementTileType.Station || newTile.Rotation != 1) {
                    break;
                }

                h += 1;
            }
        }
        else {
            Debug.LogError("WTF?");
        }

        return new(leftX, bottomY, w, h);
    }

    public void PickRandomItemForTheTrain(HorseTrain horse) {
        TrainNode? foundNode = null;
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

        Building? foundBuilding = null;
        ResourceObj? foundResource = null;
        var foundResourceIndex = -1;
        var resourceSlotPosition = Vector2.zero;
        foreach (var building in shuffledBuildings) {
            if (building.scriptable.type != BuildingType.Store) {
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
            resourceSlotPosition = building.scriptable.storedItemPositions[
                                       foundResourceIndex % building.scriptable
                                           .storedItemPositions.Count] +
                                   building.pos;
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
            if (building.scriptable.type != BuildingType.Produce) {
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

        Building? foundBuilding = null;
        foreach (var building in buildings) {
            if (building.scriptable.type != BuildingType.Produce) {
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

        TrainNode? foundNode = null;
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
