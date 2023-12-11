using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
    public ProcessingEmployeeBehaviour(int unbooks) {
        Assert.IsTrue(unbooks >= 0);
        _unbooks = unbooks;
    }

    public override void OnEnter(Human human, BuildingDatabase bdb, HumanDatabase db) {
        Assert.AreEqual(human.harvestingElapsed, 0);
        Assert.AreNotEqual(human.building, null);
    }

    public override void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
        Assert.AreNotEqual(human.building, null);
        var building = human.building!;

        var foundIndex = -1;
        for (var i = 0; i < building.BookedTiles.Count; i++) {
            var (tileBehaviourId, _) = building.BookedTiles[i];
            if (tileBehaviourId == _unbooks) {
                foundIndex = i;
                break;
            }
        }

        Assert.IsTrue(foundIndex >= 0);
        db.Map.bookedTiles.Remove(building.BookedTiles[foundIndex].Item2);
        building.BookedTiles.RemoveAt(foundIndex);

        human.harvestingElapsed = 0;
    }

    public override void UpdateDt(Human human, BuildingDatabase bdb, HumanDatabase db, float dt) {
        Assert.AreNotEqual(human.building, null);
        human.harvestingElapsed += dt;

        var harvestingDuration = human.building!.scriptable.harvestableResource.harvestingDuration;

        if (human.harvestingElapsed >= harvestingDuration) {
            human.harvestingElapsed = harvestingDuration;
            db.Controller.SwitchToTheNextBehaviour(human);
        }
    }

    readonly int _unbooks;
}
}
