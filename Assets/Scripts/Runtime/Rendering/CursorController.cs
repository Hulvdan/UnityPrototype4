using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BFG.Runtime.Rendering {
public enum CursorType {
    Pointer,
    Hand,
}

[Serializable]
public class CursorPair {
    public Texture2D texture;
    public CursorType type;
    public Vector2 hotspot = Vector2.zero;
}

public class CursorController : MonoBehaviour {
    [SerializeField]
    [Required]
    MapRenderer _mapRenderer;

    [FormerlySerializedAs("_pairs")]
    [SerializeField]
    List<CursorPair> _cursors = new();

    readonly List<IDisposable> _dependencyHooks = new();

    public void Start() {
        SetCursor(CursorType.Pointer);
    }

    public void InitDependencies() {
        foreach (var hook in _dependencyHooks) {
            hook.Dispose();
        }

        _dependencyHooks.Clear();

        _dependencyHooks.Add(_mapRenderer.onPickupableItemHoveringChanged.Subscribe(
            e => {
                var cursorType = e == PickupableItemHoveringState.StartedHovering
                    ? CursorType.Hand
                    : CursorType.Pointer;

                SetCursor(cursorType);
            }
        ));
    }

    void SetCursor(CursorType type) {
        foreach (var c in _cursors) {
            if (c.type == type) {
                Cursor.SetCursor(c.texture, c.hotspot, CursorMode.Auto);
                break;
            }
        }
    }
}
}
