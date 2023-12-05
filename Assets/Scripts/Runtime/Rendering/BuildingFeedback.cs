using BFG.Runtime.Entities;
using UnityEngine;

namespace BFG.Runtime {
internal abstract class BuildingFeedback : MonoBehaviour {
    public abstract void UpdateData(Building building, ref BuildingData data);
}
}
