#nullable enable
using BFG.Runtime.Controllers.Human;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public class EmployeeController {
    public EmployeeController(BuildingDatabase bdb, HumanDatabase db) {
        _bdb = bdb;
        _db = db;
        db.controller = this;
    }

    public void SwitchToTheNextBehaviour(Human human) {
        Assert.AreEqual(human.type, Human.Type.Employee);
        Assert.AreNotEqual(human.building, null);
        Assert.AreNotEqual(human.behaviourSet, null);

        var building = human.building!;

        if (human.currentBehaviourId >= 0) {
            var beh = human.behaviourSet!.behaviours[human.currentBehaviourId];
            beh.OnExit(human, _bdb, _db);
        }

        human.currentBehaviourId++;
        if (human.currentBehaviourId >= human.behaviourSet!.behaviours.Count) {
            human.currentBehaviourId = -1;

            _db.map.EmployeeReachedBuildingCallback(human);
            building.employeeIsInside = true;

            _bdb.controller.SwitchToTheNextBehaviour(building);

            return;
        }

        var newBeh = human.behaviourSet.behaviours[human.currentBehaviourId];
        newBeh.OnEnter(human, _bdb, _db);
    }

    public void OnHumanMovedToTheNextTile(Human human, HumanData data) {
        Assert.AreNotEqual(human.behaviourSet, null);

        var beh = human.behaviourSet!.behaviours[human.currentBehaviourId];
        beh.OnHumanMovedToTheNextTile(human, data, _db);
    }

    public void Update(Human human, HumanData data, float dt) {
        Assert.AreNotEqual(human.behaviourSet, null);

        var beh = human.behaviourSet!.behaviours[human.currentBehaviourId];
        beh.UpdateDt(human, _bdb, _db, data, dt);
    }

    readonly BuildingDatabase _bdb;
    readonly HumanDatabase _db;
}
}
