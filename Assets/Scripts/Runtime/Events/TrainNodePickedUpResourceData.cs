using UnityEngine;

namespace BFG.Runtime {
public class TrainNodePickedUpResourceData {
    public HorseTrain Train;

    public int PickedUpAmount;

    public ScriptableResource Resource;
    public Vector2Int BuildingResourceTilePosition;
}
}
