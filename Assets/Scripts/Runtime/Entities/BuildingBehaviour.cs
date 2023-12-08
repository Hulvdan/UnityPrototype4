#nullable enable
namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour {
    public virtual void OnEnter(Building building, BuildingDatabase db) {
    }

    public virtual void OnExit(Building building, BuildingDatabase db) {
    }

    public virtual void Update(Building building, BuildingDatabase db, float dt) {
    }
}

public class IdleBehaviour : BuildingBehaviour {
    public override void OnEnter(Building building, BuildingDatabase db) {
        base.OnEnter(building, db);
    }
}

public class TakeResourceBehaviour : BuildingBehaviour {
}

public class ProcessingBehaviour : BuildingBehaviour {
}

public class PlaceResourceBehaviour : BuildingBehaviour {
}

public class HumanBehaviour : BuildingBehaviour {
}
}
