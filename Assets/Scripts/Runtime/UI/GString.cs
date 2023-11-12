using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BFG.Runtime {
public enum Language {
    EN = 0,
    RU = 1,
}

[Serializable]
public class GStringKey {
    public string Key;
}

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
        _onDestroy = LocalizationDatabase.Instance.onLanguageChanged.Subscribe(OnLanguageChanged);
    }

    void Start() {
        _text.SetText(LocalizationDatabase.Instance.GetText(_key));
    }

    void OnDestroy() {
        _onDestroy?.Dispose();
    }

    void OnLanguageChanged(Language _) {
        _text.SetText(LocalizationDatabase.Instance.GetText(_key));
    }
}
}
