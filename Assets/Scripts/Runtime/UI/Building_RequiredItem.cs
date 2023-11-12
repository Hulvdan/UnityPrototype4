using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BFG.Runtime {
public class Building_RequiredItem : MonoBehaviour {
    [SerializeField]
    [Required]
    Image _resourceImage;

    [SerializeField]
    [Required]
    TMP_Text _resourceQuantityText;

    [SerializeField]
    [Required]
    Image _backgroundImage;

    public ScriptableResource resource { get; private set; }
    public int quantity { get; private set; }

    public void Init(ScriptableResource resource2, int quantity2) {
        quantity = quantity2;
        resource = resource2;

        _resourceImage.sprite = resource.sprite;
        _resourceQuantityText.SetText($"{quantity}");
    }

    public void SetSufficient(bool sufficient) {
        _backgroundImage.color = sufficient ? Color.white : Color.red;
    }
}
}
