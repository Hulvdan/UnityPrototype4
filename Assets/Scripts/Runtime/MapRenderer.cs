using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
public class MapRenderer : MonoBehaviour {
    const string TerrainTilemapNameTemplate = "Gen-Terrain";
    const string BuildingsTilemapNameTemplate = "Gen-Buildings";
    const string ResourcesTilemapNameTemplate = "Gen-Resources";

    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

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

    [Header("Debug Dependencies")]
    [SerializeField]
    [Required]
    Tilemap _debugTilemap;

    [SerializeField]
    [Required]
    TileBase _debugTileWalkable;

    [SerializeField]
    [Required]
    TileBase _debugTileUnwalkable;

    Tilemap _resourceTilemap;
    readonly Dictionary<Guid, Tuple<Human, HumanGO>> _humans = new();

    void Awake() {
        _map.OnHumanCreated += OnHumanCreated;
        _map.OnHumanPickedUpResource += OnHumanPickedUpResource;
        _map.OnHumanPlacedResource += OnHumanPlacedResource;
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

    void Update() {
        UpdateHumans();
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
                maxHeight = Math.Max(maxHeight, _map.tileHeights[y][x]);
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
                        || _map.tileHeights[y][x] >= h
                        || (y > 0 && _map.tileHeights[y - 1][x] == h)
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
                if (_map.tiles[y][x].Resource != null
                    && _map.tiles[y][x].Resource.name == _logResource.name) {
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

                _debugTilemap.SetTile(new Vector3Int(x, y, 0),
                    walkable ? _debugTileWalkable : _debugTileUnwalkable);
            }
        }
    }

    bool TileIsACliff(int x, int y) {
        return _map.tiles[y][x].Name == "cliff";
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

    #region HumanSystem

    void OnHumanCreated(HumanCreatedData data) {
        var go = Instantiate(_humanPrefab, _grid.transform);
        _humans.Add(data.Human.ID, Tuple.Create(data.Human, go.GetComponent<HumanGO>()));
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
