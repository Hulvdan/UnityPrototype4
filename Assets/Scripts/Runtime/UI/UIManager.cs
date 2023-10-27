using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class UIManager : MonoBehaviour {
    [Header("Dependencies")]
    [SerializeField]
    [Required]
    Map _map;

    [SerializeField]
    List<ResourceTMPTextMapping> _resourceTextsMapping;

    void Start() {
        _map.OnResourceChanged += OnResourceChanged;
        // _map.OnHumanHarvestedResource += OnHumanHarvestedResource;
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
