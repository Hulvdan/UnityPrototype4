using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class Stable_Panel : MonoBehaviour {
    [SerializeField]
    [Required]
    Transform _requiredResourcesContainer;

    [SerializeField]
    [Required]
    GameObject _requiredResourcePrefab;

    public Action<Stable_Panel> OnClose = delegate { };

    public Guid id { get; private set; }
    public Building building => _building;

    Building _building;

    public void Init(
        Guid id, Building building, List<Tuple<int, ScriptableResource>> requiredResources
    ) {
        this.id = id;
        _building = building;

        foreach (var res in requiredResources) {
            var go = Instantiate(_requiredResourcePrefab, _requiredResourcesContainer);
            go.GetComponent<Building_RequiredItem>().Init(res.Item2.sprite, res.Item1);
        }
    }

    public void OnButtonClosePressed() {
        Close();
    }

    public void Close() {
        OnClose?.Invoke(this);
    }
}
}
