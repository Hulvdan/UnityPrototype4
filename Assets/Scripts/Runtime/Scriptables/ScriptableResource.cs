using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Resource", order = 1)]
public class ScriptableResource : ScriptableObject {
    [SerializeField]
    [PreviewField]
    Sprite _sprite;

    [SerializeField]
    [PreviewField]
    Sprite _smallerSprite;

    [SerializeField]
    bool _canBePlacedOnTheMap;

    [SerializeField]
    [ShowIf("_canBePlacedOnTheMap")]
    [Min(0.01f)]
    float _harvestingDuration;

    [SerializeField]
    [ShowIf("_canBePlacedOnTheMap")]
    [Min(0.01f)]
    float _plantingDuration;

    public Sprite sprite => _sprite;
    public Sprite smallerSprite => _smallerSprite;
    public float harvestingDuration => _harvestingDuration;
    public bool canBePlacedOnTheMap => _canBePlacedOnTheMap;
    public float plantingDuration => _plantingDuration;
}
}
