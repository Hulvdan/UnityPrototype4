using UnityEngine;

namespace BFG.Runtime {
public class TrainNodePickedUpResourceData {
    public HorseTrain Train;
    public TrainNode TrainNode;

    public int PickedUpAmount;

    public ScriptableResource Resource;
    public Building Building;
    public int ResourceIndex;
}
}
