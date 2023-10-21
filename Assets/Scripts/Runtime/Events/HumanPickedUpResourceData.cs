using UnityEngine;

namespace BFG.Runtime {
public class HumanPickedUpResourceData {
    public Human Human;

    public ScriptableResource Resource;
    public Vector2Int ResourceTilePosition;

    public int PickedUpAmount;
    public float RemainingAmountPercent;

    public HumanPickedUpResourceData(Human human,
        ScriptableResource resource,
        Vector2Int resourceTilePosition,
        int pickedUpAmount,
        float remainingAmountPercent) {
        Human = human;
        Resource = resource;
        PickedUpAmount = pickedUpAmount;
        ResourceTilePosition = resourceTilePosition;
        RemainingAmountPercent = remainingAmountPercent;
    }
}
}
