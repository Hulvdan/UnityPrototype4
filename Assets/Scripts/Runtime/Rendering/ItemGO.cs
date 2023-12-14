using UnityEngine;

namespace BFG.Runtime.Rendering {
public class ItemGo : MonoBehaviour {
    [SerializeField]
    SpriteRenderer _spriteRenderer;

    public void SetAs(ScriptableResource resource) {
        _spriteRenderer.sprite = resource.smallerSprite;
    }
}
}
