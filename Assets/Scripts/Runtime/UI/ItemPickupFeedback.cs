using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BFG.Runtime {
public class ItemPickupFeedback : MonoBehaviour {
    [SerializeField]
    [Required]
    TMP_Text _quantityText;

    [SerializeField]
    [Required]
    TMP_Text _resourceNameText;

    [SerializeField]
    [Required]
    Image _resourceImage;

    [SerializeField]
    [Required]
    CanvasGroup _group;

    public CanvasGroup Group => _group;

    public void Init(ScriptableResource res) {
        _resourceNameText.SetText(res.name);
        _quantityText.SetText("+1");
        _resourceImage.sprite = res.sprite;
    }
}
}
