using UnityEngine;

namespace BFG.Runtime {
public class HumanPickedUpResourceData {
    public int Amount;
    public Human Human;

    public ScriptableResource Resource;
    public Vector2Int ResourceTilePosition;

    public HumanPickedUpResourceData(
        Human human,
        ScriptableResource resource,
        int amount,
        Vector2Int resourceTilePosition
    ) {
        Human = human;
        Resource = resource;
        Amount = amount;
        ResourceTilePosition = resourceTilePosition;
    }
}
}
