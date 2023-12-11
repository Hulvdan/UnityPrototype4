using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
        var foundIndex = -1;
        for (var i = 0; i < building.BookedTiles.Count; i++) {
            var tile = building.BookedTiles[i];
            if (tile.Item1 == behaviourId) {
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
