#nullable enable

namespace BFG.Runtime.Entities {
public abstract class EmployeeBehaviour {
    public virtual bool CanBeRun(int behaviourId, Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void BookRequiredTiles(
        int behaviourId,
        Building building,
        BuildingDatabase bdb
    ) {
    }

    public virtual void OnEnter(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void OnExit(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void UpdateDt(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db,
        float dt
    ) {
        if (human.moving.to == null) {
            db.Controller.SwitchToTheNextBehaviour(human);
        }
    }
}
}
