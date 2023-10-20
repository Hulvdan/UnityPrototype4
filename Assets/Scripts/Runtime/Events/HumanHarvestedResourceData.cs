using UnityEngine;

namespace BFG.Runtime {
public class HumanHarvestedResourceData {
    public Human Human;

    public ScriptableResource Resource;
    public int Amount;
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
