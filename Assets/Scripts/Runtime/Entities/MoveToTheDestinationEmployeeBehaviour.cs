#nullable enable
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class MoveToTheDestinationEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        Assert.AreEqual(human.destination, null);
    }
}
}
