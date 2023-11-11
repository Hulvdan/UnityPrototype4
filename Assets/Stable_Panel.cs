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

    public void Init(List<Tuple<int, ScriptableResource>> requiredResources) {
        foreach (var res in requiredResources) {
            var go = Instantiate(_requiredResourcePrefab, _requiredResourcesContainer);
            go.GetComponent<Building_RequiredItem>().Init(res.Item2.sprite, res.Item1);
        }
    }
}
}
