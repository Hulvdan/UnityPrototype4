using BFG.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPickupFeedback : MonoBehaviour {
    [SerializeField]
    TMP_Text _quantityText;

    [SerializeField]
    TMP_Text _resourceNameText;

    [SerializeField]
    Image _resourceImage;

    public void Init(ScriptableResource res) {
        _resourceNameText.SetText(res.name);
        _quantityText.SetText("+1");
        _resourceImage.sprite = res.sprite;
    }
}
