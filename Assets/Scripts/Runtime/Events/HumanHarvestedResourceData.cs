using UnityEngine;

namespace BFG.Runtime {
public class HumanHarvestedResourceData {
    public Human Human;

    public string ResourceCodename;
    public int Amount;
    public Vector2Int ResourceTilePosition;

    public HumanHarvestedResourceData(
        Human human,
        string resourceCodename,
        int amount,
        Vector2Int resourceTilePosition
    ) {
        Human = human;
        ResourceCodename = resourceCodename;
        Amount = amount;
        ResourceTilePosition = resourceTilePosition;
    }
}
}
