using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BFG.Runtime {
public class Building_RequiredItem : MonoBehaviour {
    [SerializeField]
    Image _resourceImage;

    [SerializeField]
    TMP_Text _resourceQuantityText;

    public void Init(Sprite sprite, int quantity) {
        _resourceImage.sprite = sprite;
        _resourceQuantityText.SetText($"{quantity}");
    }
}
}
