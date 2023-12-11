#nullable enable
using BFG.Runtime.Controllers.Human;
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
            var beh = human.BehaviourSet.behaviours[human.currentBehaviourId];
            beh.OnExit(human, _bdb, _db);
        }

        human.currentBehaviourId++;
        if (human.currentBehaviourId >= human.BehaviourSet.behaviours.Count) {
            human.currentBehaviourId = -1;

            _db.Map.EmployeeReachedBuildingCallback(human);
            building.EmployeeIsInside = true;

            _bdb.Controller.SwitchToTheNextBehaviour(building);

            return;
        }

        var newBeh = human.BehaviourSet.behaviours[human.currentBehaviourId];
        newBeh.OnEnter(human, _bdb, _db);
    }

    public void OnHumanMovedToTheNextTile(Human human, HumanData data) {
        var beh = human.BehaviourSet.behaviours[human.currentBehaviourId];
        beh.OnHumanMovedToTheNextTile(human, data, _db);
    }

    public void Update(Human human, HumanData data, float dt) {
        var beh = human.BehaviourSet.behaviours[human.currentBehaviourId];
        beh.UpdateDt(human, _bdb, _db, data, dt);
    }

    readonly BuildingDatabase _bdb;
    readonly HumanDatabase _db;
}
}
