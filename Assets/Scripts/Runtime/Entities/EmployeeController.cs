#nullable enable
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class EmployeeController {
    public EmployeeController(BuildingDatabase bdb, HumanDatabase db) {
        _bdb = bdb;
        _db = db;
        db.Controller = this;
    }

    public void SwitchToTheNextBehaviour(Human human) {
        Assert.AreEqual(human.type, Human.Type.Employee);
        Assert.AreNotEqual(human.building, null);

        var building = human.building!;

        if (human.currentBehaviourId >= 0) {
            var beh = human.BehaviourSet.Behaviours[human.currentBehaviourId];
            beh.OnExit(human.currentBehaviourId, building, _bdb, human, _db);
        }

        human.currentBehaviourId++;
        if (human.currentBehaviourId >= human.BehaviourSet.Behaviours.Count) {
            human.currentBehaviourId = -1;

            // TODO: Event human finished processing cycle
            _bdb.Controller.SwitchToTheNextBehaviour(building);
            return;
        }

        var newBeh = human.BehaviourSet.Behaviours[human.currentBehaviourId];
        newBeh.OnEnter(human.currentBehaviourId, building, _bdb, human, _db);
    }

    readonly BuildingDatabase _bdb;
    readonly HumanDatabase _db;
}
}
