using System;
using System.Collections.Generic;
using UnityEngine;

namespace BFG.Runtime {
public class UIManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    List<ResourceTMPTextMapping> _resourceTextsMapping;

    IMap _map;

    public void InitDependencies(IMap map) {
        _map = map;
        _map.onResourceChanged.Subscribe(OnResourceChanged);
    }

    void OnResourceChanged(E_TopBarResourceChanged data) {
        foreach (var mapping in _resourceTextsMapping) {
            if (mapping.Resource != data.Resource) {
                continue;
            }

            mapping.Text.text = data.NewAmount.ToString();
            break;
        }
    }

    void OnHumanHarvestedResource(E_HumanPickedUpResource data) {
    }

    public void OnButtonPressed() {
    }
}
}
