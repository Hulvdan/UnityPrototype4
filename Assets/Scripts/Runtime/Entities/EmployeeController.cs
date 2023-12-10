#nullable enable
namespace BFG.Runtime.Entities {
public class EmployeeController {
    public EmployeeController(BuildingDatabase bdb, HumanDatabase db) {
        _bdb = bdb;
        _db = db;
        db.Controller = this;
    }

    public void SwitchToTheNextBehaviour(int behaviourId, Building building, Human human) {
        human.BehaviourSet.Behaviours[behaviourId].OnExit(behaviourId, building, _bdb, human, _db);

        human.currentBehaviourId++;
        if (behaviourId >= human.BehaviourSet.Behaviours.Count) {
            human.currentBehaviourId = -1;
            // TODO: Event human finished processing cycle
            _bdb.Controller.SwitchToTheNextBehaviour(building);
            return;
        }

        var newBeh = human.BehaviourSet.Behaviours[behaviourId];
        newBeh.OnEnter(behaviourId, building, _bdb, human, _db);
    }

    readonly BuildingDatabase _bdb;
    readonly HumanDatabase _db;
}
}
