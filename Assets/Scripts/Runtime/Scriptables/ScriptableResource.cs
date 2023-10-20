﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Resource", order = 1)]
public class ScriptableResource : ScriptableObject {
    [SerializeField]
    [PreviewField]
    Sprite _sprite;

    [SerializeField]
    bool _displayInTheRopBar;

    public bool displayInTheRopBar => _displayInTheRopBar;
    public Sprite sprite => _sprite;
}
}