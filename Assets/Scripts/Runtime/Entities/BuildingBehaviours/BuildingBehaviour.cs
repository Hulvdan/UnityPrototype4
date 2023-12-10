#nullable enable

namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void OnEnter(Building building, BuildingDatabase db) {
    }

    public virtual void OnExit(Building building, BuildingDatabase db) {
    }

    public virtual void UpdateDt(Building building, BuildingDatabase db, float dt) {
    }
}
}
