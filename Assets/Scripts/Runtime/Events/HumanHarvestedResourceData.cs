using UnityEngine;

namespace BFG.Runtime {
public class HumanHarvestedResourceData {
    public int Amount;
    public Human Human;

    public ScriptableResource Resource;
    public Vector2Int ResourceTilePosition;

    public HumanHarvestedResourceData(
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
