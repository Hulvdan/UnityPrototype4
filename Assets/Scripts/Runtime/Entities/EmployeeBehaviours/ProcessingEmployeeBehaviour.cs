using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.building, null);
        Assert.IsTrue(human.currentBehaviourId >= 0);
        var building = human.building!;

        var foundIndex = -1;
        for (var i = 0; i < building.BookedTiles.Count; i++) {
            var (tileBehaviourId, _) = building.BookedTiles[i];
            if (tileBehaviourId == human.currentBehaviourId) {
                foundIndex = i;
                break;
            }
        }

        Assert.IsTrue(foundIndex >= 0);
        db.Map.bookedTiles.Remove(building.BookedTiles[foundIndex].Item2);
        building.BookedTiles.RemoveAt(foundIndex);
    }
}
}
