using BFG.Runtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class GameplayUIManager : MonoBehaviour {
    [Header("Dependencies")]
    [Required]
    [SerializeField]
    GameplayManager _gameplayManager;

    [SerializeField]
    [Required]
    TMP_Text _resourceWood;

    [SerializeField]
    [Required]
    TMP_Text _resourceStone;

    [SerializeField]
    [Required]
    TMP_Text _resourceFood;

    void Awake() {
        _gameplayManager.OnResourceChanged.AddListener(OnResourceChanged);
    }

    void OnResourceChanged(ResourceChanged data) {
        TMP_Text resource = null;

        switch (data.Codename) {
            case "wood":
                resource = _resourceWood;
                break;
            case "stone":
                resource = _resourceStone;
                break;
            case "food":
                resource = _resourceFood;
                break;
            default:
                Debug.LogError("Tweak this switch statement!");
                break;
        }

        if (resource != null) {
            resource.text = data.NewAmount.ToString();
        }
    }
}
