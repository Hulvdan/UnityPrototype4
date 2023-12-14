using System;
using BFG.Runtime.Localization;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BFG.Runtime.Rendering.UI {
[RequireComponent(typeof(TMP_Text))]
public class GString : MonoBehaviour {
    [SerializeField]
    GStringKey _key;

    [CanBeNull]
    IDisposable _onDestroy;

    TMP_Text _text;

    void Awake() {
        _text = GetComponent<TMP_Text>();

        _onDestroy?.Dispose();
        _onDestroy = LocalizationDatabase.instance.onLanguageChanged.Subscribe(OnLanguageChanged);
    }

    void Start() {
        _text.SetText(LocalizationDatabase.instance.GetText(_key));
    }

    void OnDestroy() {
        _onDestroy?.Dispose();
    }

    void OnLanguageChanged(Language _) {
        _text.SetText(LocalizationDatabase.instance.GetText(_key));
    }
}
}
