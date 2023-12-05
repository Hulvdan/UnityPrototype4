using System;
using System.Collections.Generic;
using System.Linq;
using BFG.Runtime;
using BFG.Runtime.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor {
public class GStringKeySelectorWindow : ScriptableWizard {
    readonly Rect _gridRect = new(0, 0, 300, 60);

    int _selected;

    Dictionary<string, LocalizationRecord> _strings;
    List<string> _stringsHuman = new();

    [NonSerialized]
    public string InitialKey;

    [NonSerialized]
    public Action<string> OnKeyChanged = delegate { };

    void Awake() {
        Load();
    }

    void OnGUI() {
        GUILayout.BeginScrollView(Vector2.zero);

        var newSelected = GUI.SelectionGrid(_gridRect, _selected, _stringsHuman.ToArray(), 1);
        if (_selected != newSelected) {
            _selected = newSelected;
            OnKeyChanged?.Invoke(_strings.Keys.ToArray()[_selected]);
            Close();
        }

        GUILayout.EndScrollView();
    }

    void Load() {
        _strings = new LocalizationDatabaseLoader().Load();
        _stringsHuman = _strings.Select(i => $"{i.Key} (ex. {i.Value.En})").ToList();

        _selected = -1;
        if (!string.IsNullOrEmpty(InitialKey)) {
            _selected = _strings.Keys.ToList().IndexOf(InitialKey);
        }
    }
}
}
