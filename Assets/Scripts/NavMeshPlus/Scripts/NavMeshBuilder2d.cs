using NavMeshPlus.Components;
using System;
using System.Collections.Generic;
using BFG.Graphs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace NavMeshPlus.Extensions
{
    class NavMeshBuilder2dState: IDisposable {
        public Dictionary<Guid, Mesh> graphMeshMap = new();
        public Action<UnityEngine.Object, NavMeshBuildSource> lookupCallback;
        public int defaultArea;
        public int agentID;
        public bool overrideByGrid;
        public GameObject useMeshPrefab;
        public bool compressBounds;
        public Vector3 overrideVector;
        public GameObject parent;
        public bool hideEditorLogs;

        protected IEnumerable<GameObject> _root;
        private bool _disposed;

        public Mesh GetMesh(Graph graph)
        {
            if (graphMeshMap.TryGetValue(graph.ID, out var value)) {
                return value;
            }

            var mesh = new Mesh();
            NavMeshBuilder2d.graph2mesh(graph, mesh);
            graphMeshMap.Add(graph.ID, mesh);

            return mesh;
        }

        public void SetRoot(IEnumerable<GameObject> root)
        {
            _root = root;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (var item in graphMeshMap)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(item.Value);
#else
                    Object.Destroy(item.Value);
#endif
                }
                graphMeshMap.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
    }

    class NavMeshBuilder2d
    {
        private static void AddDefaultWalkableTilemap(List<NavMeshBuildSource> sources, NavMeshBuilder2dState builder, NavMeshModifier modifier)
        {
            var tilemap = modifier.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                if (builder.compressBounds)
                    tilemap.CompressBounds();

                if (!builder.hideEditorLogs)
                    Debug.Log($"Walkable Bounds [{tilemap.name}]: {tilemap.localBounds}");

                var box = BoxBoundSource(NavMeshSurface.GetWorldBounds(tilemap.transform.localToWorldMatrix, tilemap.localBounds));
                box.area = builder.defaultArea;
                sources.Add(box);
            }
        }

        public static void CollectSources(
            List<NavMeshBuildSource> sources, Graph graph, int area, NavMeshBuilder2dState builder
        ) {
            if (graph == null) {
                Debug.LogError("WTF?");
                return;
            }

            var mesh = builder.GetMesh(graph);
            if (mesh == null) {
                if (!builder.hideEditorLogs) {
                    Debug.Log($"{graph} mesh is null");
                }

                return;
            }

            var src = new NavMeshBuildSource();
            src.shape = NavMeshBuildSourceShape.Mesh;
            // src.component = spriteRenderer;
            src.area = area;
            src.transform = Matrix4x4.TRS(
                Vector3.Scale(
                    new (graph.Offset.x, graph.Offset.y, 0),
                    builder.overrideVector
                ),
                Quaternion.identity,
                Vector3.one
            );

            src.sourceObject = mesh;
            sources.Add(src);

            // builder.lookupCallback?.Invoke(graph, src);
        }

        public static void CollectTileSources(List<NavMeshBuildSource> sources, Tilemap tilemap, int area, NavMeshBuilder2dState builder)
        {
            var bound = tilemap.cellBounds;

            var modifierTilemap = tilemap.GetComponent<NavMeshModifierTilemap>();

            var vec3int = new Vector3Int(0, 0, 0);

            var size = new Vector3(tilemap.layoutGrid.cellSize.x, tilemap.layoutGrid.cellSize.y, 0);
            Mesh sharedMesh = null;
            Quaternion rot = default;

            if (builder.useMeshPrefab != null)
            {
                sharedMesh = builder.useMeshPrefab.GetComponent<MeshFilter>().sharedMesh;
                size = builder.useMeshPrefab.transform.localScale;
                rot = builder.useMeshPrefab.transform.rotation;
            }
            for (int i = bound.xMin; i < bound.xMax; i++)
            {
                for (int j = bound.yMin; j < bound.yMax; j++)
                {
                    var src = new NavMeshBuildSource();
                    src.area = area;

                    vec3int.x = i;
                    vec3int.y = j;
                    if (!tilemap.HasTile(vec3int))
                    {
                        continue;
                    }

                    CollectTile(tilemap, builder, vec3int, size, sharedMesh, rot, ref src);
                    if (modifierTilemap && modifierTilemap.TryGetTileModifier(vec3int, tilemap, out NavMeshModifierTilemap.TileModifier tileModifier))
                    {
                        src.area = tileModifier.overrideArea ? tileModifier.area : area;
                    }
                    sources.Add(src);

                    builder.lookupCallback?.Invoke(tilemap.GetInstantiatedObject(vec3int), src);
                }
            }
        }

        private static void CollectTile(Tilemap tilemap, NavMeshBuilder2dState builder, Vector3Int vec3int, Vector3 size, Mesh sharedMesh, Quaternion rot, ref NavMeshBuildSource src)
        {
            if (!builder.overrideByGrid && tilemap.GetColliderType(vec3int) == Tile.ColliderType.Sprite)
            {
                Debug.LogError("WTF?");
                // var sprite = tilemap.GetSprite(vec3int);
                // if (sprite != null)
                // {
                //     Mesh mesh = builder.GetMesh(sprite);
                //     src.component = tilemap;
                //     src.transform = GetCellTransformMatrix(tilemap, builder.overrideVector, vec3int);
                //     src.shape = NavMeshBuildSourceShape.Mesh;
                //     src.sourceObject = mesh;
                // }
            }
            else if (builder.useMeshPrefab != null || (builder.overrideByGrid && builder.useMeshPrefab != null))
            {
                src.transform = Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), builder.overrideVector), rot, size);
                src.shape = NavMeshBuildSourceShape.Mesh;
                src.sourceObject = sharedMesh;
            }
            else //default to box
            {
                src.transform = GetCellTransformMatrix(tilemap, builder.overrideVector, vec3int);
                src.shape = NavMeshBuildSourceShape.Box;
                src.size = size;
            }
        }

        public static Matrix4x4 GetCellTransformMatrix(Tilemap tilemap, Vector3 scale, Vector3Int vec3int)
        {
            return Matrix4x4.TRS(Vector3.Scale(tilemap.GetCellCenterWorld(vec3int), scale) - tilemap.layoutGrid.cellGap, tilemap.transform.rotation, tilemap.transform.lossyScale) * tilemap.orientationMatrix * tilemap.GetTransformMatrix(vec3int);
        }

        internal static void graph2mesh(Graph graph, Mesh mesh)
        {
            var verticesCount = 0;
            var trianglesCount = 0;
            for (var y = 0; y < graph.height; y++) {
                for (var x = 0; x < graph.width; x++) {
                    if (!graph.Contains(x, y)) {
                        verticesCount += 4;
                        trianglesCount += 6;
                    }
                }
            }

            var vert = new Vector3[verticesCount];
            var tri = new int[trianglesCount];
            var uv = new Vector2[verticesCount];
            var vi = 0;
            var ti = 0;
            for (var y = 0; y < graph.height; y++) {
                for (var x = 0; x < graph.width; x++) {
                    if (graph.Contains(x, y)) {
                        continue;
                    }

                    vert[vi+0] = new (x, y, 0);
                    vert[vi+1] = new (x, y+1, 0);
                    vert[vi+2] = new (x+1, y, 0);
                    vert[vi+3] = new (x+1, y+1, 0);

                    tri[ti+0] = vi+0;
                    tri[ti+1] = vi+1;
                    tri[ti+2] = vi+2;
                    tri[ti+3] = vi+2;
                    tri[ti+4] = vi+1;
                    tri[ti+5] = vi+3;

                    uv[vi+0] = vert[vi+0];
                    uv[vi+1] = vert[vi+1];
                    uv[vi+2] = vert[vi+2];
                    uv[vi+3] = vert[vi+3];

                    vi += 4;
                    ti += 6;
                }
            }

            mesh.vertices = vert;
            mesh.uv = uv;
            mesh.triangles = tri;
        }

        // internal static void sprite2mesh(Sprite sprite, Mesh mesh)
        // {
        //     Vector3[] vert = new Vector3[sprite.vertices.Length];
        //     for (int i = 0; i < sprite.vertices.Length; i++)
        //     {
        //         vert[i] = new Vector3(sprite.vertices[i].x, sprite.vertices[i].y, 0);
        //     }
        //     mesh.vertices = vert;
        //     mesh.uv = sprite.uv;
        //     int[] tri = new int[sprite.triangles.Length];
        //     for (int i = 0; i < sprite.triangles.Length; i++)
        //     {
        //         tri[i] = sprite.triangles[i];
        //     }
        //     mesh.triangles = tri;
        // }

        static private NavMeshBuildSource BoxBoundSource(Bounds localBounds)
        {
            var src = new NavMeshBuildSource();
            src.transform = Matrix4x4.Translate(localBounds.center);
            src.shape = NavMeshBuildSourceShape.Box;
            src.size = localBounds.size;
            src.area = 0;
            return src;
        }
    }
}
