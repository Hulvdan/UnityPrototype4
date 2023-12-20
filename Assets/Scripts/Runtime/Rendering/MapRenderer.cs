using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using BFG.Core;
using BFG.Graphs;
using BFG.Runtime.Controllers.Human;
using BFG.Runtime.Entities;
using BFG.Runtime.Extensions;
using Foundation.Architecture;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Tilemaps;

namespace BFG.Runtime.Rendering {
public enum PickupableItemHoveringState {
    StartedHovering,
    FinishedHovering,
}

public class MapRenderer : MonoBehaviour {
    const string _TERRAIN_TILEMAP_NAME_TEMPLATE = "Gen-Terrain";
    const string _BUILDINGS_TILEMAP_NAME_TEMPLATE = "Gen-Buildings";
    const string _RESOURCES_TILEMAP_NAME_TEMPLATE = "Gen-Resources";

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
    [Min(.1f)]
    float _sinCosScale;

    [SerializeField]
    [Required]
    MovementPattern _movementPattern;

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    [SerializeField]
    [Required]
    TileBase _debugTileUnbuildable;

    readonly List<IDisposable> _dependencyHooks = new();

    readonly Dictionary<Guid, (Human, HumanGo, HumanBinding)> _humans = new();

    readonly Dictionary<Guid, ItemGo> _storedItems = new();

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
                && _map.CanBePlaced(hoveredTile.Value, _gameManager.ItemToBuild.type)
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
            Assert.AreNotEqual(building.scriptable, null);

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

            for (var y = 0; y < segment.graph.height; y++) {
                for (var x = 0; x < segment.graph.width; x++) {
                    var node = segment.graph.nodes[y][x];
                    if (node == 0) {
                        continue;
                    }

                    var pos = new Vector3(x, y) + Vector3.one / 2 +
                              new Vector3(segment.graph.offset.x, segment.graph.offset.y);
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

        hooks.Add(_map.onTerrainTileChanged.Subscribe(
            OnTerrainTileChanged));
        hooks.Add(_map.onElementTileChanged.Subscribe(
            OnElementTileChanged));

        hooks.Add(_map.onHumanCreated.Subscribe(
            OnHumanCreated));
        hooks.Add(_map.onCityHallCreatedHuman.Subscribe(
            OnCityHallCreatedHuman));
        hooks.Add(_map.onHumanReachedCityHall.Subscribe(
            data => RemoveHuman(data.human)));
        hooks.Add(_map.onEmployeeReachedBuilding.Subscribe(
            data => RemoveHuman(data.human)));

        hooks.Add(_map.onHumanMovedToTheNextTile.Subscribe(
            OnHumanMovedToTheNextTile));
        hooks.Add(_map.onHumanStartedPickingUpResource.Subscribe(
            OnHumanStartedPickingUpResource));
        hooks.Add(_map.onHumanFinishedPickingUpResource.Subscribe(
            OnHumanPickedUpResource));
        hooks.Add(_map.onHumanStartedPlacingResource.Subscribe(
            OnHumanStartedPlacingResource));
        hooks.Add(_map.onHumanFinishedPlacingResource.Subscribe(
            OnHumanPlacedResource));

        hooks.Add(_map.onBuildingPlaced.Subscribe(
            OnBuildingPlaced));

        hooks.Add(_map.onHumanStartedConstructingBuilding.Subscribe(
            OnHumanStartedConstructingBuilding));
        hooks.Add(_map.onHumanConstructedBuilding.Subscribe(
            OnHumanConstructedBuilding));
    }

    void OnHumanMovedToTheNextTile(E_HumanMovedToTheNextTile data) {
        var (human, go, binding) = _humans[data.human.id];

        var pos = new Vector2(human.moving.pos.x, human.moving.pos.y);
        go.transform.localPosition = pos + Vector2.one / 2;

        binding.curvePerFeedback.Clear();
        foreach (var feedback in _movementPattern.feedbacks) {
            binding.curvePerFeedback.Add(feedback.GetRandomCurve());
        }

        DomainEvents<E_HumanFootstep>.Publish(new() { human = data.human });
    }

    void OnHumanPlacedResource(E_HumanPlacedResource data) {
        var (human, go, _) = _humans[data.human.id];
        go.OnStoppedPlacingResource();

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var pos = new Vector2(human.moving.pos.x, human.moving.pos.y);
        item.transform.localPosition = pos + Vector2.one / 2;

        var itemGo = item.GetComponent<ItemGo>();
        itemGo.SetAs(data.resource.scriptable);

        _storedItems.Add(data.resource.id, itemGo);

        if (data.building != null) {
            data.building.timeSinceItemWasPlaced = 0;
        }
    }

    void OnHumanPickedUpResource(E_HumanPickedUpResource data) {
        var (_, go, _) = _humans[data.human.id];
        go.OnStoppedPickingUpResource();

        if (_storedItems.TryGetValue(data.resource.id, out var itemGo)) {
            Destroy(itemGo.gameObject);
            _storedItems.Remove(data.resource.id);
        }
    }

    void OnHumanStartedPlacingResource(E_HumanStartedPlacingResource data) {
        var (_, go, _) = _humans[data.human.id];
        go.OnStartedPlacingResource(data.resource.scriptable);
    }

    void OnHumanStartedPickingUpResource(E_HumanStartedPickingUpResource data) {
        var (_, go, _) = _humans[data.human.id];
        go.OnStartedPickingUpResource(data.resource.scriptable);

        if (_storedItems.TryGetValue(data.resource.id, out var res)) {
            Destroy(res.gameObject);
            _storedItems.Remove(data.resource.id);
        }
    }

    void OnHumanStartedConstructingBuilding(E_HumanStartedConstructingBuilding data) {
        // Hulvdan: Intentionally left blank
    }

    void OnHumanConstructedBuilding(E_HumanConstructedBuilding data) {
        var building = data.building;
        SetBuilding(building, 1, 1, Color.white);
    }

    void OnBuildingPlaced(E_BuildingPlaced data) {
        _buildingFeedbacks.Add(
            data.building.id, new(BuildingData.Create(), new() { _scaleFeedback })
        );
        SetBuilding(data.building, 1, 1, Color.white);
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

            prevData.scale = buildingData.scale;
            foreach (var feedback in feedbacks) {
                feedback.UpdateData(building, ref buildingData);
            }

            if (prevData.Equals(buildingData)) {
                continue;
            }

            prevData.scale = Vector2.one;
            _buildingFeedbacks[building.id] = new(buildingData, feedbacks);
            SetBuilding(building, buildingData.scale.x, buildingData.scale.y, color);
        }
    }

    void OnTerrainTileChanged(Vector2Int pos) {
        var terrainTile = _map.terrainTiles[pos.y][pos.x];

        if (terrainTile.resource == null) {
            Assert.AreEqual(terrainTile.resourceAmount, 0);

            _resourceTilemap.SetTile(new(pos.x, pos.y), null);
            _resourceTilemap.SetTile(new(pos.x, pos.y, -1), null);
        }
        else {
            Assert.AreNotEqual(terrainTile.resourceAmount, 0);
            Assert.IsTrue(terrainTile.resource!.canBePlacedOnTheMap);

            // TODO(Hulvdan): Generalize it, so that it's possible to plant other resources
            _resourceTilemap.SetTile(new(pos.x, pos.y, 0), _tileForest);
            _resourceTilemap.SetTile(new(pos.x, pos.y, -1), _tileForestTop);
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
        foreach (var offset in DirectionOffsets.OFFSETS) {
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
            if (child.gameObject.name.StartsWith(_TERRAIN_TILEMAP_NAME_TEMPLATE)
                || child.gameObject.name.StartsWith(_BUILDINGS_TILEMAP_NAME_TEMPLATE)
                || child.gameObject.name.StartsWith(_RESOURCES_TILEMAP_NAME_TEMPLATE)) {
                child.gameObject.SetActive(false);
            }
        }
    }

    void RegenerateTilemapGameObjects() {
        var maxHeight = 0;
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                maxHeight = Math.Max(maxHeight, _map.terrainTiles[y][x].height);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain =
                GenerateTerrainTilemap(i, i, _TERRAIN_TILEMAP_NAME_TEMPLATE, _tilemapPrefab);
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
                        || _map.terrainTiles[y][x].height >= h
                        || (y > 0 && _map.terrainTiles[y - 1][x].height == h)
                    ) {
                        terrainMaps[h].SetTile(new(x, y, 0), _tileGrass);
                    }
                }
            }
        }

        _resourceTilemap = GenerateTerrainTilemap(
                0, maxHeight + 1, _RESOURCES_TILEMAP_NAME_TEMPLATE, _tilemapPrefab
            )
            .GetComponent<Tilemap>();
        for (var y = 0; y < _mapSize.height; y++) {
            for (var x = 0; x < _mapSize.width; x++) {
                if (_map.terrainTiles[y][x].resource != null
                    && _map.terrainTiles[y][x].resource.name == _logResource.name) {
                    _resourceTilemap.SetTile(new(x, y, 0), _tileForest);
                    _resourceTilemap.SetTile(new(x, y, -1), _tileForestTop);
                }
            }
        }

        _buildingsTilemap = GenerateTerrainTilemap(
                0, maxHeight + 2, _BUILDINGS_TILEMAP_NAME_TEMPLATE, _tilemapBuildingsPrefab
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

        switch (item.type) {
            case ItemToBuildType.Road:
                tilemapTile = _tileRoad;
                break;
            case ItemToBuildType.Flag:
                tilemapTile = _tileFlag;
                break;
            case ItemToBuildType.Building:
                Assert.AreNotEqual(item.building, null);
                Assert.AreNotEqual(item.building!.tile, null);
                tilemapTile = item.building.tile;
                break;
            default:
                return;
        }

        var buildable = _map.CanBePlaced(pos, item.type);
        if (!buildable) {
            return;
        }

        var matrix = Matrix4x4.identity;
        if (item.type == ItemToBuildType.Building) {
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
        var humanGo = go.GetComponent<HumanGo>();

        var movementBinding = new HumanBinding {
            curvePerFeedback = new() { Capacity = _movementPattern.feedbacks.Count },
        };
        foreach (var feedback in _movementPattern.feedbacks) {
            movementBinding.curvePerFeedback.Add(feedback.GetRandomCurve());
        }

        _humans.Add(data.human.id, new(data.human, humanGo, movementBinding));
        UpdateHuman(data.human, humanGo, movementBinding);
    }

    void OnCityHallCreatedHuman(E_CityHallCreatedHuman data) {
        data.cityHall.timeSinceHumanWasCreated = 0;
    }

    void RemoveHuman(Human human) {
        var (_, humanGo, _) = _humans[human.id];
        Destroy(humanGo.gameObject);
        _humans.Remove(human.id);
    }

    void UpdateHumans() {
        foreach (var (human, go, binding) in _humans.Values) {
            UpdateHuman(human, go, binding);
        }
    }

    void UpdateHuman(Human human, HumanGo go, HumanBinding binding) {
        if (human.moving.to == null) {
            go.transform.localPosition = human.moving.from;
        }
        else {
            for (var i = 0; i < _movementPattern.feedbacks.Count; i++) {
                var feedback = _movementPattern.feedbacks[i];
                var curve = binding.curvePerFeedback[i];
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
    public Vector2 scale;

    public static BuildingData Create() {
        return new(Vector2.one);
    }

    BuildingData(Vector2 scale_) {
        scale = scale_;
    }

    public bool Equals(BuildingData other) {
        return scale.Equals(other.scale);
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
        return scale.GetHashCode();
    }
}
}
