using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
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
    TileBase _tileStationHorizontal;

    [SerializeField]
    [Required]
    TileBase _tileStationVertical;

    [SerializeField]
    [Required]
    TileBase _tileForest;

    [SerializeField]
    [Required]
    TileBase _tileForestTop;

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

    [Header("Inputs")]
    [SerializeField]
    [Required]
    InputActionAsset _inputActionAsset;

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    // [SerializeField]
    // [Required]
    // TileBase _debugTileWalkable;

    [SerializeField]
    [Required]
    TileBase _debugTileUnwalkable;

    readonly List<IDisposable> _dependencyHooks = new();

    readonly Dictionary<Guid, Tuple<Human, HumanGO>> _humans = new();
    Camera _camera;

    GameManager _gameManager;
    InputActionMap _gameplayInputMap;
    Map _map;
    InputAction _mouseBuildAction;
    InputAction _mouseMoveAction;
    Matrix4x4 _previewMatrix;

    Tilemap _resourceTilemap;

    void Awake() {
        _camera = Camera.main;

        _gameplayInputMap = _inputActionAsset.FindActionMap("Gameplay");
        _mouseMoveAction = _gameplayInputMap.FindAction("PreviewMouseMove");
        _mouseBuildAction = _gameplayInputMap.FindAction("Build");
    }

    void Update() {
        UpdateHumans();
        DisplayPreviewTile();

        // TODO: Move inputs to GameManager
        if (_mouseBuildAction.WasPressedThisFrame()) {
            var hoveredCell = GetHoveredCell();
            if (!_map.Contains(hoveredCell)) {
                return;
            }

            _map.TryBuild(hoveredCell, _gameManager.selectedItem);
        }
        // else if (_mouseBuildAction.WasReleasedThisFrame()) {
        // }
    }

    void OnEnable() {
        _gameplayInputMap.Enable();
    }

    void OnDisable() {
        _gameplayInputMap.Disable();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;

        foreach (var building in _map.buildings) {
            if (building.scriptableBuilding == null
                || building.scriptableBuilding.cellsRadius == 0) {
                continue;
            }

            var r = building.scriptableBuilding.cellsRadius + .55f;
            var gridOffset = _grid.transform.localPosition + transform.localPosition;
            var points = new Vector3[] {
                new(r, r, 0),
                new(r, -r, 0),
                new(r, -r, 0),
                new(-r, -r, 0),
                new(-r, -r, 0),
                new(-r, r, 0),
                new(-r, r, 0),
                new(r, r, 0)
            };
            for (var i = 0; i < points.Length; i++) {
                var point = points[i];
                point.x += building.posX + gridOffset.x + .5f;
                point.y += building.posY + gridOffset.y + .5f;
                points[i] = point;
            }

            Gizmos.DrawLineList(points);
        }
    }

    public void InitDependencies(GameManager gameManager, Map map) {
        _map = map;
        _gameManager = gameManager;

        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();
        _dependencyHooks.Add(_gameManager.OnSelectedItemChanged.Subscribe(OnSelectedItemChanged));
        _dependencyHooks.Add(_map.OnElementTileChanged.Subscribe(OnElementTileChanged));
        _dependencyHooks.Add(_map.OnHumanCreated.Subscribe(OnHumanCreated));
        _dependencyHooks.Add(_map.OnHumanPickedUpResource.Subscribe(OnHumanPickedUpResource));
        _dependencyHooks.Add(_map.OnHumanPlacedResource.Subscribe(OnHumanPlacedResource));
    }

    void OnSelectedItemChanged(SelectedItem item) {
    }

    void OnElementTileChanged(Vector2Int pos) {
        var elementTile = _map.elementTiles[pos.y][pos.x];

        TileBase tile;
        switch (elementTile.Type) {
            case ElementTileType.Road:
                tile = _tileRoad;
                break;
            case ElementTileType.Station:
                tile = elementTile.Rotation == 0 ? _tileStationHorizontal : _tileStationVertical;
                break;
            case ElementTileType.None:
                return;
            default:
                return;
        }

        _movementSystemTilemap.SetTile(new Vector3Int(pos.x, pos.y), tile);
    }

    void UpdateTileBasedOnRemainingResourcePercent(
        Vector2Int pos,
        float dataRemainingAmountPercent
    ) {
        if (dataRemainingAmountPercent > 0) {
            return;
        }

        _resourceTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), null);
        _resourceTilemap.SetTile(new Vector3Int(pos.x, pos.y, -1), null);
    }

    public void ResetRenderer() {
        DeleteOldTilemaps();
        RegenerateTilemapGameObject();
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

    void RegenerateTilemapGameObject() {
        var maxHeight = 0;
        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                maxHeight = Math.Max(maxHeight, _map.terrainTiles[y][x].Height);
            }
        }

        // Terrain 0 (y=0)
        // Terrain 1 (y=-0.001)
        // Terrain 2 (y=-0.002)
        // Buildings 2 (y=-0.0021)
        var terrainMaps = new List<Tilemap>();
        for (var i = 0; i <= maxHeight; i++) {
            var terrain = GenerateTilemap(i, i, TerrainTilemapNameTemplate, _tilemapPrefab);
            terrainMaps.Add(terrain.GetComponent<Tilemap>());
        }

        for (var h = 0; h <= maxHeight; h++) {
            for (var y = 0; y < _map.sizeY; y++) {
                if (h > 0 && y == _map.sizeY) {
                    continue;
                }

                for (var x = 0; x < _map.sizeX; x++) {
                    if (
                        h == 0
                        || _map.terrainTiles[y][x].Height >= h
                        || (y > 0 && _map.terrainTiles[y - 1][x].Height == h)
                    ) {
                        terrainMaps[h].SetTile(new Vector3Int(x, y, 0), _tileGrass);
                    }
                }
            }
        }

        _resourceTilemap = GenerateTilemap(
            0, maxHeight + 1, ResourcesTilemapNameTemplate, _tilemapPrefab
        ).GetComponent<Tilemap>();
        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                if (_map.terrainTiles[y][x].Resource != null
                    && _map.terrainTiles[y][x].Resource.name == _logResource.name) {
                    _resourceTilemap.SetTile(new Vector3Int(x, y, 0), _tileForest);
                    _resourceTilemap.SetTile(new Vector3Int(x, y, -1), _tileForestTop);
                }
            }
        }

        var buildingsTilemap = GenerateTilemap(
            0, maxHeight + 2, BuildingsTilemapNameTemplate, _tilemapBuildingsPrefab
        ).GetComponent<Tilemap>();
        foreach (var building in _map.buildings) {
            buildingsTilemap.SetTile(
                new Vector3Int(building.posX, building.posY, 0),
                building.scriptableBuilding.tile
            );
        }
    }

    void RegenerateDebugTilemapGameObject() {
        _debugTilemap.ClearAllTiles();

        for (var y = 0; y < _map.sizeY; y++) {
            for (var x = 0; x < _map.sizeX; x++) {
                bool walkable;
                if (y >= _map.sizeY) {
                    walkable = false;
                }
                else {
                    walkable = !TileIsACliff(x, y);
                }

                if (!walkable) {
                    _debugTilemap.SetTile(new Vector3Int(x, y, 0), _debugTileUnwalkable);
                }
            }
        }
    }

    bool TileIsACliff(int x, int y) {
        return _map.terrainTiles[y][x].Name == "cliff";
    }

    GameObject GenerateTilemap(int i, float order, string nameTemplate, GameObject prefabTemplate) {
        var terrainTilemap = Instantiate(prefabTemplate, _grid.transform);
        terrainTilemap.name = nameTemplate + i;
        terrainTilemap.transform.localPosition = new Vector3(0, -order / 100000f, 0);
        return terrainTilemap;
    }

    void UpdateGridPosition() {
        _grid.transform.localPosition = new Vector3(-_map.sizeX / 2f, -_map.sizeY / 2f);
    }

    void DisplayPreviewTile() {
        _previewTilemap.ClearAllTiles();

        var cell = GetHoveredCell();
        if (!_map.Contains(cell)) {
            return;
        }

        TileBase tile;
        if (_gameManager.selectedItem == SelectedItem.Road) {
            tile = _tileRoad;
        }
        else if (_gameManager.selectedItem == SelectedItem.Station) {
            tile = _gameManager.selectedItemRotation % 2 == 0
                ? _tileStationVertical
                : _tileStationHorizontal;
        }
        else {
            return;
        }

        _previewTilemap.SetTile(
            new TileChangeData(
                new Vector3Int(cell.x, cell.y, 0),
                tile,
                Color.white,
                _previewMatrix
            ),
            false
        );
    }

    Vector2Int GetHoveredCell() {
        var mousePos = _mouseMoveAction.ReadValue<Vector2>();
        var wPos = _camera.ScreenToWorldPoint(mousePos);
        return (Vector2Int)_previewTilemap.WorldToCell(wPos);
    }

    #region HumanSystem

    void OnHumanCreated(HumanCreatedData data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        _humans.Add(data.Human.ID, Tuple.Create(data.Human, go.GetComponent<HumanGO>()));
    }

    void OnHumanPickedUpResource(HumanPickedUpResourceData data) {
        UpdateTileBasedOnRemainingResourcePercent(
            data.ResourceTilePosition,
            data.RemainingAmountPercent
        );

        _humans[data.Human.ID].Item2.OnPickedUpResource(data.Resource);
    }

    void OnHumanPlacedResource(HumanPlacedResourceData data) {
        _humans[data.Human.ID].Item2.OnPlacedResource();

        var item = Instantiate(_itemPrefab, _itemsLayer);

        var building = data.StoreBuilding;
        var scriptable = building.scriptableBuilding;

        var i = building.storedResources.Count - 1;
        if (i >= scriptable.storedItemPositions.Count) {
            Debug.LogError("WTF i >= scriptable.storedItemPositions.Count");
            i = scriptable.storedItemPositions.Count - 1;
        }

        var itemOffset = scriptable.storedItemPositions[i];
        item.transform.localPosition = building.position + itemOffset + Vector2.right / 2;
        item.GetComponent<ItemGO>().SetAs(data.Resource);
    }

    Vector3 GameLogicToRenderPos(Vector2 pos) {
        return _grid.LocalToWorld(pos) + new Vector3(.5f, .5f, 0);
    }

    void UpdateHumans() {
        foreach (var (human, go) in _humans.Values) {
            go.transform.position = GameLogicToRenderPos(human.position);
        }
    }

    #endregion
}
}
