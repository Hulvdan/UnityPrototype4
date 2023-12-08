using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Controllers.Human;
using BFG.Runtime.Entities;
using BFG.Runtime.Extensions;
using DG.Tweening;
using Foundation.Architecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace BFG.Runtime.Rendering {
public enum PickupableItemHoveringState {
    StartedHovering,
    FinishedHovering,
}

public class MapRenderer : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    const string BuildingsTilemapNameTemplate = "Gen-Buildings";
    const string ResourcesTilemapNameTemplate = "Gen-Resources";

    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _movementSystemTilemap;

    [SerializeField]
    [Required]
    Tilemap _flagsTilemap;

    [SerializeField]
    [Required]
    Tilemap _previewTilemap;

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
    TileBase _tileUnfinishedBuilding;

    [SerializeField]
    [Required]
    TileBase _tileFlag;

    [SerializeField]
    [Required]
    GameObject _tilemapPrefab;

    [SerializeField]
    [Required]
    GameObject _tilemapBuildingsPrefab;

    [SerializeField]
    [Required]
    GameObject _humanPrefab;

    [SerializeField]
    [Required]
    ScriptableResource _logResource;

    [SerializeField]
    [Required]
    Transform _itemsLayer;

    [SerializeField]
    [Required]
    GameObject _itemPrefab;

    [Header("Setup")]
    [SerializeField]
    [Required]
    BuildingFeedback _scaleFeedback;

    [SerializeField]
    [Min(0.1f)]
    float _buildingMovingItemToTheWarehouseDuration = 1f;

    [SerializeField]
    [Min(0.1f)]
    AnimationCurve _buildingMovingItemToTheWarehouseDurationCurve =
        AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    [Min(.1f)]
    float _sinCosScale;

    [SerializeField]
    [Required]
    MovementPattern _movementPattern;

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    [FormerlySerializedAs("_debugTileUnwalkable")]
    [SerializeField]
    [Required]
    TileBase _debugTileUnbuildable;

    readonly List<IDisposable> _dependencyHooks = new();

    readonly Dictionary<Guid, (Human, HumanGO, HumanBinding)> _humans =
        new();

    readonly Dictionary<Guid, ItemGO> _storedItems = new();

    float _buildingScaleTimeline;

    readonly Dictionary<Guid, (BuildingData, List<BuildingFeedback>)> _buildingFeedbacks = new();
    Tilemap _buildingsTilemap;

    GameManager _gameManager;

    IMap _map;
    IMapSize _mapSize;

    public bool mouseBuildActionWasPressed { get; set; }
    public Vector2Int? hoveredTile { get; set; }

    Tilemap _resourceTilemap;

    public Subject<PickupableItemHoveringState> onPickupableItemHoveringChanged { get; } = new();

    void Update() {
        UpdateHumans();
        UpdateBuildings();

        DisplayPreviewTile();

        // TODO: Move inputs to GameManager
        if (hoveredTile != null && mouseBuildActionWasPressed) {
            if (
                _gameManager.ItemToBuild != null
                && _map.CanBePlaced(hoveredTile.Value, _gameManager.ItemToBuild.Type)
            ) {
                _map.TryBuild(hoveredTile.Value, _gameManager.ItemToBuild);
            }
        }
    }

    void OnDrawGizmos() {
        if (_map == null) {
            return;
        }

        Gizmos.color = Color.cyan;

        foreach (var building in _map.buildings) {
            if (
                building.scriptable == null
                || building.scriptable.type == BuildingType.Harvest
                || building.scriptable.tilesRadius == 0
            ) {
                continue;
            }

            var r = building.scriptable.tilesRadius + .45f;
            var points = new Vector3[] {
                new(r, r, 0),
                new(r, -r, 0),
                new(r, -r, 0),
                new(-r, -r, 0),
                new(-r, -r, 0),
                new(-r, r, 0),
                new(-r, r, 0),
                new(r, r, 0),
            };
            for (var i = 0; i < points.Length; i++) {
                var point = points[i];
                point.x += building.posX + .5f;
                point.y += building.posY + .5f;
                points[i] = _grid.transform.TransformPoint(point);
            }

            Gizmos.DrawLineList(points);
        }

        foreach (var (human, _, _) in _humans.Values) {
            Gizmos.color = Color.cyan;
            var offset = Vector2.one / 2;
            Gizmos.DrawSphere(_grid.transform.TransformPoint(human.moving.pos + offset), .2f);

            if (human.moving.to != null) {
                Gizmos.color = Color.red;
                var humanMovingFrom = human.moving.from + offset;
                var humanMovingTo = human.moving.to.Value + offset;
                Gizmos.DrawLine(
                    _grid.transform.TransformPoint(humanMovingFrom),
                    _grid.transform.TransformPoint(humanMovingTo)
                );
                Gizmos.DrawSphere(
                    _grid.transform.TransformPoint(
                        Vector2.Lerp(
                            humanMovingFrom,
                            humanMovingTo,
                            human.moving.progress
                        )
                    ),
                    .2f
                );
            }
        }

        var clri = 0;
        var colors = new[] {
            Color.white, Color.red, Color.cyan, Color.green, Color.magenta, Color.yellow,
        };
        foreach (var segment in _map.segments) {
            Gizmos.color = colors[clri];

            for (var y = 0; y < segment.Graph.height; y++) {
                for (var x = 0; x < segment.Graph.width; x++) {
                    var node = segment.Graph.nodes[y][x];
                    if (node == 0) {
                        continue;
                    }

                    var pos = new Vector3(x, y) + Vector3.one / 2 +
                              new Vector3(segment.Graph.offset.x, segment.Graph.offset.y);
                    if (GraphNode.IsRight(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.right / 2)
                        );
                    }

                    if (GraphNode.IsUp(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.up / 2)
                        );
                    }

                    if (GraphNode.IsLeft(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.left / 2)
                        );
                    }

                    if (GraphNode.IsDown(node)) {
                        Gizmos.DrawLine(
                            _grid.transform.TransformPoint(pos),
                            _grid.transform.TransformPoint(pos + Vector3.down / 2)
                        );
                    }
                }
            }

            clri++;
            if (clri >= colors.Length) {
                clri = 0;
            }
        }
    }

    public void InitDependencies(GameManager gameManager, IMap map, IMapSize mapSize) {
        _map = map;
        _mapSize = mapSize;
        _gameManager = gameManager;

        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();
        InitializeDependencyHooks();
    }

    public void Init() {
        foreach (var building in _map.buildings) {
            _buildingFeedbacks.Add(
                building.id, new(BuildingData.Create(), new() { _scaleFeedback })
            );
        }
    }

    void InitializeDependencyHooks() {
        var hooks = _dependencyHooks;

        hooks.Add(_map.onElementTileChanged.Subscribe(
            OnElementTileChanged));

        hooks.Add(_map.onHumanCreated.Subscribe(
            OnHumanCreated));
        hooks.Add(_map.onCityHallCreatedHuman.Subscribe(
            OnCityHallCreatedHuman));
        hooks.Add(_map.onHumanReachedCityHall.Subscribe(
            OnHumanReachedCityHall));

        hooks.Add(_map.onHumanMovedToTheNextTile.Subscribe(
            OnHumanMovedToTheNextTile));
        hooks.Add(_map.onHumanStartedPickingUpResource.Subscribe(
            OnHumanStartedPickingUpResource));
        hooks.Add(_map.onHumanPickedUpResource.Subscribe(
            OnHumanPickedUpResource));
        hooks.Add(_map.onHumanStartedPlacingResource.Subscribe(
            OnHumanStartedPlacingResource));
        hooks.Add(_map.onHumanPlacedResource.Subscribe(
            OnHumanPlacedResource));

        hooks.Add(_map.onBuildingPlaced.Subscribe(
            OnBuildingPlaced));

        hooks.Add(_map.OnHumanStartedConstructingBuilding.Subscribe(
            OnHumanStartedConstructingBuilding));
        hooks.Add(_map.OnHumanConstructedBuilding.Subscribe(
            OnHumanConstructedBuilding));
    }

    void OnHumanMovedToTheNextTile(E_HumanMovedToTheNextTile data) {
        var (human, go, binding) = _humans[data.Human.ID];

        var pos = new Vector2(human.moving.pos.x, human.moving.pos.y);
        go.transform.localPosition = pos + Vector2.one / 2;

        binding.CurvePerFeedback.Clear();
        foreach (var feedback in _movementPattern.Feedbacks) {
            binding.CurvePerFeedback.Add(feedback.GetRandomCurve());
        }

        DomainEvents<E_HumanFootstep>.Publish(new() { Human = data.Human });
    }

    void OnHumanPlacedResource(E_HumanPlacedResource data) {
        var (human, go, _) = _humans[data.Human.ID];
        go.OnStoppedPlacingResource();

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var pos = new Vector2(human.moving.pos.x, human.moving.pos.y);
        item.transform.localPosition = pos + Vector2.one / 2;

        var itemGo = item.GetComponent<ItemGO>();
        itemGo.SetAs(data.Resource.Scriptable);

        _storedItems.Add(data.Resource.ID, itemGo);

        if (data.Building != null) {
            data.Building.timeSinceItemWasPlaced = 0;
        }
    }

    void OnHumanPickedUpResource(E_HumanPickedUpResource data) {
        var (_, go, _) = _humans[data.Human.ID];
        go.OnStoppedPickingUpResource();

        if (_storedItems.TryGetValue(data.Resource.ID, out var itemGo)) {
            Destroy(itemGo.gameObject);
            _storedItems.Remove(data.Resource.ID);
        }
    }

    void OnHumanStartedPlacingResource(
        E_HumanStartedPlacingResource data
    ) {
        var (_, go, _) = _humans[data.Human.ID];
        go.OnStartedPlacingResource(data.Resource.Scriptable);
    }

    void OnHumanStartedPickingUpResource(
        E_HumanStartedPickingUpResource data
    ) {
        var (_, go, _) = _humans[data.Human.ID];
        go.OnStartedPickingUpResource(data.Resource.Scriptable);

        if (_storedItems.TryGetValue(data.Resource.ID, out var res)) {
            Destroy(res.gameObject);
            _storedItems.Remove(data.Resource.ID);
        }
    }

    void OnHumanStartedConstructingBuilding(E_HumanStartedConstructingBuilding data) {
        // Hulvdan: Intentionally left blank
    }

    void OnHumanConstructedBuilding(E_HumanConstructedBuilding data) {
        var building = data.Building;
        SetBuilding(building, 1, 1, Color.white);
    }

    void OnBuildingPlaced(E_BuildingPlaced data) {
        _buildingFeedbacks.Add(
            data.Building.id, new(BuildingData.Create(), new() { _scaleFeedback })
        );
        SetBuilding(data.Building, 1, 1, Color.white);
    }

    void UpdateBuildings() {
        _buildingScaleTimeline += Time.deltaTime * _gameManager.currentGameSpeed;
        if (_buildingScaleTimeline >= 2 * Mathf.PI * _sinCosScale) {
            _buildingScaleTimeline -= 2 * Mathf.PI * _sinCosScale;
        }

        var prevData = BuildingData.Create();

        foreach (var building in _map.buildings) {
            var color = Color.white;
            var (buildingData, feedbacks) = _buildingFeedbacks[building.id];

            prevData.Scale = buildingData.Scale;
            foreach (var feedback in feedbacks) {
                feedback.UpdateData(building, ref buildingData);
            }

            if (prevData.Equals(buildingData)) {
                continue;
            }

            prevData.Scale = Vector2.one;
            _buildingFeedbacks[building.id] = new(buildingData, feedbacks);
            SetBuilding(building, buildingData.Scale.x, buildingData.Scale.y, color);
        }
    }

    void OnElementTileChanged(Vector2Int pos) {
        var elementTile = _map.elementTiles[pos.y][pos.x];

        TileBase tile;
        switch (elementTile.type) {
            case ElementTileType.Road:
                tile = _tileRoad;
                break;
            case ElementTileType.Flag:
                _flagsTilemap.SetTile(new(pos.x, pos.y), _tileFlag);
                return;
            case ElementTileType.None:
                return;
            default:
                return;
        }

        _movementSystemTilemap.SetTile(new(pos.x, pos.y), tile);

        var debugTile = _map.IsBuildable(pos.x, pos.y) ? null : _debugTileUnbuildable;
        _debugTilemap.SetTile(new(pos.x, pos.y, 0), debugTile);
        foreach (var offset in DirectionOffsets.Offsets) {
            var newPos = pos + offset;
            if (!_mapSize.Contains(newPos)) {
                continue;
            }

            debugTile = _map.IsBuildable(newPos.x, newPos.y) ? null : _debugTileUnbuildable;
            _debugTilemap.SetTile(new(newPos.x, newPos.y, 0), debugTile);
        }
    }

    void UpdateTileBasedOnRemainingResourcePercent(
        Vector2Int pos,
        float dataRemainingAmountPercent
    ) {
        if (dataRemainingAmountPercent > 0) {
            return;
        }

        _resourceTilemap.SetTile(new(pos.x, pos.y, 0), null);
        _resourceTilemap.SetTile(new(pos.x, pos.y, -1), null);
    }

    public void ResetRenderer() {
        DeleteOldTilemaps();
        RegenerateTilemapGameObjects();
        RegenerateDebugTilemapGameObject();
        UpdateGridPosition();
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

    void RegenerateTilemapGameObjects() {
        var maxHeight = 0;
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                maxHeight = Math.Max(maxHeight, _map.terrainTiles[y][x].Height);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain = GenerateTerrainTilemap(i, i, TerrainTilemapNameTemplate, _tilemapPrefab);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _mapSize.height; y++) {
                if (h > 0 && y == _mapSize.height) {
                    continue;
                }

                for (var x = 0; x < _mapSize.width; x++) {
                    if (
                        h == 0
                        || _map.terrainTiles[y][x].Height >= h
                        || (y > 0 && _map.terrainTiles[y - 1][x].Height == h)
                    ) {
                        terrainMaps[h].SetTile(new(x, y, 0), _tileGrass);
                    }
                }
            }
        }

        _resourceTilemap = GenerateTerrainTilemap(
                0, maxHeight + 1, ResourcesTilemapNameTemplate, _tilemapPrefab
            )
            .GetComponent<Tilemap>();
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                if (_map.terrainTiles[y][x].Resource != null
                    && _map.terrainTiles[y][x].Resource.name == _logResource.name) {
                    _resourceTilemap.SetTile(new(x, y, 0), _tileForest);
                    _resourceTilemap.SetTile(new(x, y, -1), _tileForestTop);
                }
            }
        }

        _buildingsTilemap = GenerateTerrainTilemap(
                0, maxHeight + 2, BuildingsTilemapNameTemplate, _tilemapBuildingsPrefab
            )
            .GetComponent<Tilemap>();
        foreach (var building in _map.buildings) {
            SetBuilding(building, 1, 1, Color.white);
        }
    }

    void SetBuilding(Building building, float scaleX, float scaleY, Color color) {
        var widthOffset = (building.scriptable.size.x - 1) / 2f;
        var heightOffset = (building.scriptable.size.y - 1) / 2f;

        var tile = building.scriptable.tile;
        if (!building.isConstructed) {
            tile = _tileUnfinishedBuilding;
        }

        _buildingsTilemap.SetTile(
            new(
                new(building.posX, building.posY, 0),
                tile,
                color,
                Matrix4x4.TRS(
                    new(widthOffset, heightOffset),
                    Quaternion.identity,
                    new(scaleX, scaleY, 1)
                )
            ),
            false
        );

        _movementSystemTilemap.SetTile(new(building.posX, building.posY, 0), _tileRoad);
    }

    void RegenerateDebugTilemapGameObject() {
        _debugTilemap.ClearAllTiles();

        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                if (!_map.IsBuildable(x, y)) {
                    _debugTilemap.SetTile(new(x, y, 0), _debugTileUnbuildable);
                }
            }
        }
    }

    GameObject GenerateTerrainTilemap(
        int i,
        float order,
        string nameTemplate,
        GameObject prefabTemplate
    ) {
        var terrainTilemap = Instantiate(prefabTemplate, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.localPosition = new(0, -order / 100000f, 0);
        return terrainTilemap;
    }

    void UpdateGridPosition() {
        _grid.transform.localPosition = new(-_mapSize.width / 2f, -_mapSize.height / 2f);
    }

    void DisplayPreviewTile() {
        _previewTilemap.ClearAllTiles();

        var hover = hoveredTile;
        if (hover == null) {
            return;
        }

        var pos = hover.Value;
        if (!_mapSize.Contains(pos)) {
            return;
        }

        TileBase tilemapTile;
        var item = _gameManager.ItemToBuild;
        if (item == null) {
            return;
        }

        switch (item.Type) {
            case ItemToBuildType.Road:
                tilemapTile = _tileRoad;
                break;
            case ItemToBuildType.Flag:
                tilemapTile = _tileFlag;
                break;
            case ItemToBuildType.Building:
                Assert.AreNotEqual(item.Building, null);
                Assert.AreNotEqual(item.Building!.tile, null);
                tilemapTile = item.Building.tile;
                break;
            default:
                return;
        }

        var buildable = _map.CanBePlaced(pos, item.Type);
        if (!buildable) {
            return;
        }

        var matrix = Matrix4x4.identity;
        if (item.Type == ItemToBuildType.Building) {
            matrix = Matrix4x4.TRS(new(0, -0.5f, 0), Quaternion.identity, Vector3.one);
        }

        _previewTilemap.SetTile(
            new(
                new(pos.x, pos.y, 0),
                tilemapTile,
                Color.white,
                matrix
            ),
            false
        );
    }

    #region HumanSystem

    void OnHumanCreated(E_HumanCreated data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        var humanGo = go.GetComponent<HumanGO>();

        var movementBinding = new HumanBinding {
            CurvePerFeedback = new() { Capacity = _movementPattern.Feedbacks.Count },
        };
        foreach (var feedback in _movementPattern.Feedbacks) {
            movementBinding.CurvePerFeedback.Add(feedback.GetRandomCurve());
        }

        _humans.Add(data.Human.ID, new(data.Human, humanGo, movementBinding));
        UpdateHuman(data.Human, humanGo, movementBinding);
    }

    void OnCityHallCreatedHuman(E_CityHallCreatedHuman data) {
        data.CityHall.timeSinceHumanWasCreated = 0;
    }

    void OnHumanReachedCityHall(E_HumanReachedCityHall data) {
        var human = _humans[data.Human.ID];
        Destroy(human.Item2.gameObject);
        _humans.Remove(data.Human.ID);
    }

    void UpdateHumans() {
        foreach (var (human, go, binding) in _humans.Values) {
            UpdateHuman(human, go, binding);
        }
    }

    void UpdateHuman(Human human, HumanGO go, HumanBinding binding) {
        if (human.moving.to == null) {
            go.transform.localPosition = human.moving.from;
        }
        else {
            for (var i = 0; i < _movementPattern.Feedbacks.Count; i++) {
                var feedback = _movementPattern.Feedbacks[i];
                var curve = binding.CurvePerFeedback[i];
                var t = curve.Evaluate(human.moving.progress);

                feedback.UpdateData(
                    Time.deltaTime,
                    human.moving.progress,
                    t,
                    human.moving.from,
                    human.moving.to.Value,
                    go.gameObject
                );
            }
        }

        go.transform.localPosition += Vector3.one.With(z: 0) / 2;
        if (
            human.movingResources
            == MovingResources.State.PickingUpResource
        ) {
            go.SetPickingUpResourceProgress(human.movingResources_pickingUpResourceProgress);
        }

        if (
            human.movingResources
            == MovingResources.State.PlacingResource
        ) {
            go.SetPlacingResourceProgress(human.movingResources_placingResourceProgress);
        }
    }

    #endregion
}

internal struct BuildingData : IEquatable<BuildingData> {
    public Vector2 Scale;

    public static BuildingData Create() {
        return new(Vector2.one);
    }

    BuildingData(Vector2 scale) {
        Scale = scale;
    }

    public bool Equals(BuildingData other) {
        return Scale.Equals(other.Scale);
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        if (obj.GetType() != GetType()) {
            return false;
        }

        return Equals((BuildingData)obj);
    }

    public override int GetHashCode() {
        return Scale.GetHashCode();
    }
}
}
