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
        _map.OnResourceChanged.Subscribe(OnResourceChanged);
    }

    void OnResourceChanged(TopBarResourceChangedData data) {
        foreach (var mapping in _resourceTextsMapping) {
            if (mapping.Resource != data.Resource) {
                continue;
            }

            mapping.Text.text = data.NewAmount.ToString();
            break;
        }
    }

    void OnHumanHarvestedResource(HumanPickedUpResourceData data) {
    }

    public void OnButtonPressed() {
    }
}
}
