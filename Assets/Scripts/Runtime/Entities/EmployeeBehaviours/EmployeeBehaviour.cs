#nullable enable

namespace BFG.Runtime.Entities {
public abstract class EmployeeBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void BookRequiredTiles(Building building, BuildingDatabase bdb) {
    }

    public virtual void OnEnter(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void OnExit(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db
    ) {
    }

    public virtual void UpdateDt(
        Building building,
        BuildingDatabase bdb,
        Human human,
        HumanDatabase db,
        float dt
    ) {
    }
}
}
