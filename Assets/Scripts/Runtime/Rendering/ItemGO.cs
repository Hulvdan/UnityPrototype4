using UnityEngine;

namespace BFG.Runtime {
public class ItemGO : MonoBehaviour {
    [SerializeField]
    SpriteRenderer _spriteRenderer;

    public void SetAs(ScriptableResource resource) {
        _spriteRenderer.sprite = resource.smallerSprite;
    }
}
}
