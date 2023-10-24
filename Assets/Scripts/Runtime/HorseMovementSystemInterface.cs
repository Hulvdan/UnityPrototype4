using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BFG.Runtime {
public class HorseMovementSystemInterface : MonoBehaviour {
    HorseMovementSystem _system;

    public List<GameObject> Nodes = new();

    [SerializeField]
    Sprite _arrow0;

    [SerializeField]
    Sprite _arrow1;

    [SerializeField]
    Sprite _arrow21;

    [SerializeField]
    Sprite _arrow22;

    [SerializeField]
    Sprite _arrow3;

    [SerializeField]
    Sprite _arrow4;

    [Button("Generate system with nodes")]
    void GenerateSystemWithNodes() {
    }
}
}
