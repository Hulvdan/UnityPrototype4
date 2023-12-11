#nullable enable
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class MoveToTheDestinationEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreEqual(human.destination, null);
    }
}
}
