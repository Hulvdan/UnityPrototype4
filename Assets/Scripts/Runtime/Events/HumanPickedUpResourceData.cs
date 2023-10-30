using UnityEngine;

namespace BFG.Runtime {
public class HumanPickedUpResourceData {
    public Human Human;

    public int PickedUpAmount;
    public float RemainingAmountPercent;

    public ResourceObj Resource;
    public Vector2Int ResourceTilePosition;
}
}
