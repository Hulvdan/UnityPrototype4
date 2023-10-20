using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BFG.Runtime {
[CreateAssetMenu(fileName = "Data", menuName = "Gameplay/Resource", order = 1)]
public class ScriptableResource : ScriptableObject {
    [SerializeField]
    string _codename;

    [SerializeField]
    [PreviewField]
    Texture2D _texture;

    [SerializeField]
    bool _displayInTheRopBar;

    public string codename => _codename;
    public bool displayInTheRopBar => _displayInTheRopBar;

    public Texture2D texture => _texture;
}
}
