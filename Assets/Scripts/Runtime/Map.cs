#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using BFG.Core;
using BFG.Runtime.Controllers.Human;
using BFG.Runtime.Entities;
using BFG.Runtime.Graphs;
using BFG.Runtime.Systems;
using Foundation.Architecture;
using Priority_Queue;
using SimplexNoise;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BFG.Runtime {
public class Map : MonoBehaviour, IMap, IMapSize {
    // Layers:
    // 1 - Terrain (depends on height)
    // 2 - Trees, Stone, Ore
    // 3 - Roads
    // 4 - Humans, Buildings
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
    List<BuildingGo> _buildingGameObjects = null!;

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
    InitialMapProvider _initialMapProvider = null!;

    [FoldoutGroup("Debug", true)]
    [SerializeField]
    bool _hideEditorLogs;

    GameManager _gameManager = null!;

    public List<GraphSegment> segments { get; } = new();

    void Update() {
        var dt = _gameManager.dt;
        _resourceTransportation.PathfindItemsInQueue();
        UpdateHumans(dt);
        UpdateBuildings(dt);
    }

    public Subject<Vector2Int> onElementTileChanged { get; } = new();
    public Subject<E_BuildingPlaced> onBuildingPlaced { get; } = new();

    // NOTE(Hulvdan): Indexes start from the bottom left corner and go to the top right one
    public List<List<ElementTile>> elementTiles { get; private set; } = null!;
    public List<List<TerrainTile>> terrainTiles { get; private set; } = null!;

    public List<Building> buildings { get; private set; } = new();

    public List<List<List<MapResource>>> mapResources { get; private set; } = null!;

    BuildingController _buildingController = null!;

    public void Init() {
        _initialMapProvider.Init(this, this);

        foreach (var building in buildings) {
            building.constructionElapsed = building.scriptable.ConstructionDuration;
        }

        if (Application.isPlaying) {
            RegenerateTilemap();
            OnTerrainTilesRegenerated.Invoke();
        }

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

        _resourceTransportation = new(this, this);
        var buildingDatabase = new BuildingDatabase(this, this);
        var humanDatabase = new HumanDatabase(this, this);
        _buildingController = new(buildingDatabase);
        _humanController = new(
            this, this, cityHall, _resourceTransportation, buildingDatabase, humanDatabase
        );

        if (Application.isPlaying) {
            // TryBuild(new(8, 8), new() { Type = SelectedItemType.Road });
            // TryBuild(new(9, 8), new() { Type = SelectedItemType.Road });
            // TryBuild(new(10, 8), new() { Type = SelectedItemType.Road });
            // TryBuild(new(10, 7), new() { Type = SelectedItemType.Road });
            // TryBuild(new(10, 6), new() { Type = SelectedItemType.Road });
            // TryBuild(new(10, 5), new() { Type = SelectedItemType.Road });
            // TryBuild(new(10, 4), new() { Type = SelectedItemType.Road });

            // TryBuild(new(7, 7), new() { Type = SelectedItemType.Road });
            // TryBuild(new(6, 7), new() { Type = SelectedItemType.Road });
            // TryBuild(new(6, 7), new() { Type = SelectedItemType.Flag });
            // TryBuild(new(6, 6), new() { Type = SelectedItemType.Road });
            // TryBuild(
            //     new(5, 6),
            //     new() {
            //         Type = SelectedItemType.Building,
            //         Building = _lumberjacksHouse,
            //     }
            // );

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

        buildings = _buildingGameObjects.Select(i => i.IntoBuilding()).ToList();
    }

    public void TryBuild(Vector2Int pos, ItemToBuild item) {
        if (!Contains(pos)) {
            return;
        }

        using var _ = Tracing.Scope();
        Tracing.Log($"Placing {item.Type} at {pos}");

        if (item.Type == ItemToBuildType.Road) {
            var road = elementTiles[pos.y][pos.x];
            road.type = ElementTileType.Road;
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
        else if (item.Type == ItemToBuildType.Building) {
            Assert.AreNotEqual(item.Building, null);
            var building = new Building(Guid.NewGuid(), item.Building!, pos, 0);
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

            UpdateBuilding_NotConstructed(building, 0);
            UpdateSegments(res);
        }
        else if (item.Type == ItemToBuildType.Flag) {
            if (elementTiles[pos.y][pos.x].type != ElementTileType.Road) {
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

        DomainEvents<E_ItemPlaced>.Publish(new() { Item = item.Type, Pos = pos });
    }

    public bool CanBePlaced(Vector2Int pos, ItemToBuildType itemType) {
        if (!Contains(pos.x, pos.y)) {
            Debug.LogError("WTF?");
            return false;
        }

        switch (itemType) {
            case ItemToBuildType.Road:
            case ItemToBuildType.Building:
                return IsBuildable(pos);
            case ItemToBuildType.Flag:
                return elementTiles[pos.y][pos.x].type == ElementTileType.Road;
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
        foreach (var human in _humans) {
            var state = MovingInTheWorld.State.MovingToTheCityHall;
            if (human.stateMovingInTheWorld == state) {
                humansMovingToCityHall++;
            }
        }

        Stack<Tuple<GraphSegment?, Human>> humansThatNeedNewSegment =
            new(res.DeletedSegments.Count + humansMovingToCityHall);
        foreach (var human in _humans) {
            var state = MovingInTheWorld.State.MovingToTheCityHall;
            if (human.stateMovingInTheWorld == state) {
                humansThatNeedNewSegment.Push(new(null, human));
            }
        }

        foreach (var segment in res.DeletedSegments) {
            segments.Remove(segment);

            var human = segment.AssignedHuman;
            if (human != null) {
                human.segment = null;
                segment.AssignedHuman = null;
                humansThatNeedNewSegment.Push(new(segment, human));
            }

            foreach (var linkedSegment in segment.LinkedSegments) {
                linkedSegment.Unlink(segment);
            }

            _resourceTransportation.OnSegmentDeleted(segment);

            if (segmentsThatNeedHumans.Contains(segment)) {
                segmentsThatNeedHumans.Remove(segment);
                Assert.IsFalse(segmentsThatNeedHumans.Contains(segment));
            }
        }

        if (!_hideEditorLogs) {
            Debug.Log($"{humansThatNeedNewSegment.Count} Humans need to find new segments");
        }

        foreach (var segment in res.AddedSegments) {
            foreach (var segmentToLink in segments) {
                // Mb there Graph.CollidesWith(other.Graph) is needed for optimization
                if (segmentToLink.HasSomeOfTheSameVertices(segment)) {
                    segment.Link(segmentToLink);
                    segmentToLink.Link(segment);
                }
            }

            segments.Add(segment);
        }

        _resourceTransportation.PathfindItemsInQueue();
        Tracing.Log("_itemTransportationSystem.PathfindItemsInQueue()");

        while (humansThatNeedNewSegment.Count > 0 && segmentsThatNeedHumans.Count > 0) {
            var segment = segmentsThatNeedHumans.Dequeue();

            var (oldSegment, human) = humansThatNeedNewSegment.Pop();
            human.segment = segment;
            segment.AssignedHuman = human;
            _humanController.OnHumanCurrentSegmentChanged(human, oldSegment);
        }

        foreach (var segment in res.AddedSegments) {
            if (humansThatNeedNewSegment.Count == 0) {
                segmentsThatNeedHumans.Enqueue(segment, 0);
                continue;
            }

            var (oldSegment, human) = humansThatNeedNewSegment.Pop();
            human.segment = segment;
            segment.AssignedHuman = human;
            _humanController.OnHumanCurrentSegmentChanged(human, oldSegment);
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
                        var g1x = x + g1.offset.x;
                        // ReSharper disable once InconsistentNaming
                        var g1y = y + g1.offset.y;
                        if (!g2.Contains(g1x, g1y)) {
                            continue;
                        }

                        // ReSharper disable once InconsistentNaming
                        var g2y = g1y - g2.offset.y;
                        // ReSharper disable once InconsistentNaming
                        var g2x = g1x - g2.offset.x;
                        var node = g2.nodes[g2y][g2x];

                        Assert.AreEqual(node & g1.nodes[y][x], 0);
                    }
                }
            }
        }
    }

    SimplePriorityQueue<GraphSegment> segmentsThatNeedHumans { get; } = new();

    public PathFindResult FindPath(
        Vector2Int source,
        Vector2Int destination,
        bool avoidHarvestableResources
    ) {
        if (source == destination) {
            return new() {
                Value = new() { Capacity = 0 },
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

    public void OnResourcePlacedInsideBuilding(MapResource res, Building building) {
        var totalCount = 0;
        foreach (var requiredResource in building.scriptable.requiredResourcesToBuild) {
            totalCount += requiredResource.Number;
        }

        if (building.PlacedResourcesForConstruction.Count == totalCount) {
            Human? human = null;
            foreach (var h in _humans) {
                if (
                    h.type != Human.Type.Constructor
                    || h.state != MainState.MovingInTheWorld
                    || h.stateMovingInTheWorld != MovingInTheWorld.State.MovingToTheCityHall
                ) {
                    continue;
                }

                human = h;
                break;
            }

            if (human == null) {
                CreateHuman_Constructor(cityHall, building);
            }
            else {
                human.building = building;
                building.constructor = human;
            }
        }

        DomainEvents<E_ResourcePlacedInsideBuilding>.Publish(new());
    }

    public void OnBuildingConstructed(Building building, Human constructor) {
        OnHumanConstructedBuilding.OnNext(new() {
            Building = building,
            Human = constructor,
        });

        if (building.scriptable.type == BuildingType.Harvest) {
            CreateHuman_Employee(cityHall, building);
        }
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

        if (elementTiles[y][x].type != ElementTileType.None) {
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

    public int height => _mapSizeY;
    public int width => _mapSizeX;

    public bool Contains(Vector2Int pos) {
        return Contains(pos.x, pos.y);
    }

    public bool Contains(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void UpdateBuildings(float dt) {
        foreach (var building in buildings) {
            if (building.constructionProgress < 1) {
                UpdateBuilding_NotConstructed(building, dt);
            }
            else {
                UpdateBuilding_Constructed(building, dt);
            }
        }
    }

    void UpdateBuilding_NotConstructed(Building building, float dt) {
        if (!building.isConstructed) {
            building.timeSinceItemWasPlaced += dt;
        }

        if (building.ResourcesToBook.Count > 0) {
            _resourceTransportation.Add_ResourcesToBook(building.ResourcesToBook);
            building.ResourcesToBook.Clear();
        }
    }

    void UpdateBuilding_Constructed(Building building, float dt) {
        var scriptableBuilding = building.scriptable;
        if (scriptableBuilding.type == BuildingType.SpecialCityHall) {
            building.timeSinceHumanWasCreated += dt;
            if (building.timeSinceHumanWasCreated > _humanSpawningDelay) {
                building.timeSinceHumanWasCreated = _humanSpawningDelay;
            }

            if (segmentsThatNeedHumans.Count != 0) {
                if (building.timeSinceHumanWasCreated >= _humanSpawningDelay) {
                    building.timeSinceHumanWasCreated -= _humanSpawningDelay;
                    CreateHuman_Transporter(cityHall, segmentsThatNeedHumans.Dequeue());
                }
            }
        }

        _buildingController.Update(building, dt);
    }

    Building cityHall => buildings.Find(i => i.scriptable.type == BuildingType.SpecialCityHall);

    #region HumanSystem

    float humanMovingOneTileDuration => _humanMovingOneTileDuration;
    public IBookedTiles bookedTiles { get; } = new BookedTilesSet();

    public void CreateHuman_Employee_ForTheNextProcessingCycle(
        Building building,
        EmployeeBehaviourSet behaviourSet
    ) {
        Assert.AreEqual(building.scriptable.type, BuildingType.Harvest);

        var human = Human.Employee(Guid.NewGuid(), building.pos, building);
        human.BehaviourSet = behaviourSet;

        Assert.AreEqual(building.employee, null);
        building.employee = human;

        _humansToAdd.Add(human);

        FinalizeNewHuman(building, human, MainState.Employee);
    }

    public void EmployeeReachedBuildingCallback(Human human) {
        _humansToRemove.Add(new(HumanRemovalReason.EmployeeReachedBuilding, human));
    }

    [FoldoutGroup("Humans", true)]
    [ShowInInspector]
    [ReadOnly]
    [Min(0.01f)]
    float _humanMovingOneTileDuration = 1f;

    [FoldoutGroup("Humans", true)]
    [SerializeField]
    [Min(0)]
    float _humanSpawningDelay = 1f;

    readonly List<Human> _humans = new();
    readonly List<Human> _humansToAdd = new();
    readonly List<(HumanRemovalReason, Human)> _humansToRemove = new();

    ResourceTransportation _resourceTransportation = null!;
    MainController _humanController = null!;

    enum HumanRemovalReason {
        TransporterReturnedCityHall,
        EmployeeReachedBuilding,
    }

    void CreateHuman_Transporter(Building building, GraphSegment segment) {
        var human = Human.Transporter(Guid.NewGuid(), building.pos, segment);
        segment.AssignedHuman = human;
        _humans.Add(human);

        FinalizeNewHuman(cityHall, human, MainState.MovingInTheWorld);
    }

    void CreateHuman_Constructor(Building cityHall, Building building) {
        var human = Human.Constructor(Guid.NewGuid(), cityHall.pos, building);
        building.constructor = human;
        _humansToAdd.Add(human);

        FinalizeNewHuman(cityHall, human, MainState.MovingInTheWorld);
    }

    void CreateHuman_Employee(Building cityHall, Building building) {
        Assert.AreEqual(building.scriptable.type, BuildingType.Harvest);

        var human = Human.Employee(Guid.NewGuid(), cityHall.pos, building);

        Assert.AreEqual(building.employee, null);
        building.employee = human;

        _humansToAdd.Add(human);

        FinalizeNewHuman(cityHall, human, MainState.MovingInTheWorld);
    }

    void FinalizeNewHuman(Building building, Human human, MainState state) {
        _humanController.SetState(human, state);

        onHumanCreated.OnNext(new() { Human = human });

        if (building.scriptable.type == BuildingType.SpecialCityHall) {
            onCityHallCreatedHuman.OnNext(new() { CityHall = building });
            DomainEvents<E_CityHallCreatedHuman>.Publish(new() { CityHall = building });
        }
    }

    void UpdateHumans(float dt) {
        foreach (var (reason, _) in _humansToRemove) {
            Assert.AreNotEqual(reason, HumanRemovalReason.TransporterReturnedCityHall);
        }

        RemoveHumans();

        foreach (var human in _humans) {
            UpdateHuman(dt, human, _humansToRemove);
        }

        foreach (var human in _humansToAdd) {
            _humans.Add(human);
            UpdateHuman(dt, human, _humansToRemove);
        }

        _humansToAdd.Clear();

        RemoveHumans();
    }

    void RemoveHumans() {
        foreach (var (reason, human) in _humansToRemove) {
            if (reason == HumanRemovalReason.TransporterReturnedCityHall) {
                onHumanReachedCityHall.OnNext(new() { Human = human });
            }
            else if (reason == HumanRemovalReason.EmployeeReachedBuilding) {
                onEmployeeReachedBuilding.OnNext(new() { Human = human });
            }

            _humans.RemoveAt(_humans.IndexOf(human));
        }

        _humansToRemove.Clear();
    }

    void UpdateHuman(float dt, Human human, List<(HumanRemovalReason, Human)> humansToRemove) {
        if (human.moving.to != null) {
            UpdateHumanMovingComponent(dt, human);
        }

        _humanController.Update(human, dt);
        var state = MovingInTheWorld.State.MovingToTheCityHall;
        if (
            human.stateMovingInTheWorld == state
            && human.moving.pos == cityHall.pos
            && human.moving.to == null
        ) {
            humansToRemove.Add((
                HumanRemovalReason.TransporterReturnedCityHall,
                human
            ));
        }
    }

    void UpdateHumanMovingComponent(float dt, Human human) {
        // ReSharper disable once InconsistentNaming
        const int GUARD_MAX_MOVING_TILES_PER_FRAME = 4;

        var moving = human.moving;
        moving.elapsed += dt;

        var iteration = 0;
        while (
            iteration < 10 * GUARD_MAX_MOVING_TILES_PER_FRAME
            && moving.to != null
            && moving.elapsed > humanMovingOneTileDuration
        ) {
            iteration++;

            using var _ = Tracing.Scope();
            Tracing.Log("Human reached the next tile");

            moving.elapsed -= humanMovingOneTileDuration;

            moving.pos = moving.to.Value;
            moving.from = moving.pos;
            moving.PopMovingTo();

            _humanController.OnHumanMovedToTheNextTile(human);
            onHumanMovedToTheNextTile.OnNext(new() {
                Human = human,
            });
        }

        Assert.IsTrue(iteration < 10 * GUARD_MAX_MOVING_TILES_PER_FRAME);
        if (iteration >= GUARD_MAX_MOVING_TILES_PER_FRAME) {
            Debug.LogWarning("WTF?");
        }

        moving.progress = Mathf.Min(1, moving.elapsed / _humanMovingOneTileDuration);
    }

    #endregion

    #region Events

    public Subject<E_HumanCreated> onHumanCreated { get; } = new();
    public Subject<E_CityHallCreatedHuman> onCityHallCreatedHuman { get; } = new();

    public Subject<E_HumanReachedCityHall> onHumanReachedCityHall { get; } = new();
    public Subject<E_EmployeeReachedBuilding> onEmployeeReachedBuilding { get; } = new();

    public Subject<E_HumanMovedToTheNextTile>
        onHumanMovedToTheNextTile { get; } = new();

    public Subject<E_HumanStartedPickingUpResource>
        onHumanStartedPickingUpResource { get; } = new();

    public Subject<E_HumanPickedUpResource> onHumanPickedUpResource { get; } =
        new();

    public Subject<E_HumanStartedPlacingResource>
        onHumanStartedPlacingResource { get; } =
        new();

    public Subject<E_HumanPlacedResource> onHumanPlacedResource { get; } =
        new();

    public Subject<E_HumanStartedConstructingBuilding> OnHumanStartedConstructingBuilding { get; } =
        new();

    public Subject<E_HumanConstructedBuilding> OnHumanConstructedBuilding { get; } = new();

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

        for (var y = 0; y < _mapSizeY; y++) {
            var row = new List<TerrainTile>();

            for (var x = 0; x < _mapSizeX; x++) {
                var forestK = MakeSomeNoise2D(_randomSeed, x, y, _forestNoiseScale);
                var hasForest = forestK > _forestThreshold;
                var tile = new TerrainTile {
                    Name = "grass",
                    Resource = hasForest ? _logResource : null,
                    ResourceAmount = hasForest ? _maxForestAmount : 0,
                };

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
}
}
