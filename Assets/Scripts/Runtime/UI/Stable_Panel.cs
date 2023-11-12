using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BFG.Runtime {
public class Stable_Panel : MonoBehaviour {
    [SerializeField]
    [Required]
    Transform _requiredResourcesContainer;

    [SerializeField]
    [Required]
    GameObject _requiredResourcePrefab;

    [SerializeField]
    [Required]
    Button _createButton;

    readonly List<Building_RequiredItem> _requiredItems = new();

    GameManager _gameManager;

    IDisposable _hook;

    IMap _map;

    [NonSerialized]
    public Action<Stable_Panel> OnClose = delegate { };

    public Guid id { get; private set; }
    public Building building { get; private set; }

    public void Init(
        Guid id_,
        IMap map,
        GameManager gameManager,
        Building building_,
        List<Tuple<int, ScriptableResource>> requiredResources
    ) {
        id = id_;
        building = building_;
        _map = map;
        _gameManager = gameManager;

        foreach (var res in requiredResources) {
            var go = Instantiate(_requiredResourcePrefab, _requiredResourcesContainer);
            var requiredItem = go.GetComponent<Building_RequiredItem>();
            requiredItem.Init(res.Item2, res.Item1);

            _requiredItems.Add(requiredItem);
        }

        _hook?.Dispose();
        _hook = _map.onResourceChanged.Subscribe(_ => RecalculateResources());

        RecalculateResources();
    }

    void RecalculateResources() {
        var allSufficient = true;
        foreach (var reqItem in _requiredItems) {
            var sufficient = false;
            foreach (var mapResource in _map.resources) {
                if (mapResource.Resource == reqItem.resource) {
                    sufficient = mapResource.Amount >= reqItem.quantity;
                }
            }

            reqItem.SetSufficient(sufficient);
            allSufficient &= sufficient;
        }

        _createButton.interactable = allSufficient;
    }

    public void OnButtonClosePressed() {
        Close();
    }

    public void Close() {
        _hook.Dispose();
        _hook = null;
        OnClose?.Invoke(this);
    }
}
}
