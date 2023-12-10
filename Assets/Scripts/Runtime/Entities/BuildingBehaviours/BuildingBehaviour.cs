#nullable enable

namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour {
    public virtual bool CanBeRun(Building building, BuildingDatabase bdb) {
        return true;
    }

    public virtual void BookRequiredTiles(Building building, BuildingDatabase bdb) {
    }

    public virtual void OnEnter(Building building, BuildingDatabase bdb) {
    }

    public virtual void OnExit(Building building, BuildingDatabase bdb) {
    }

    public virtual void UpdateDt(Building building, BuildingDatabase bdb, float dt) {
    }
}
}
