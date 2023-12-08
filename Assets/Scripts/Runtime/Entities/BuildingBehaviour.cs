#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BFG.Runtime.Entities {
public abstract class BuildingBehaviour : MonoBehaviour {
    public virtual bool CanBeRun(BuildingDatabase db) {
        return true;
    }

    public virtual void OnEnter(Building building, BuildingDatabase db) {
    }

    public virtual void OnExit(Building building, BuildingDatabase db) {
    }

    public virtual void Update(Building building, BuildingDatabase db, float dt) {
    }
}

public class IdleBuildingBehaviour : BuildingBehaviour {
    public override bool CanBeRun(BuildingDatabase db) {
        return true;
    }

    public override void OnEnter(Building building, BuildingDatabase db) {
        Assert.AreEqual(building.idleElapsed, 0);
    }

    public override void OnExit(Building building, BuildingDatabase db) {
        building.idleElapsed = 0;
    }
}

public sealed class TakeResourceBuildingBehaviour : BuildingBehaviour {
}

public sealed class ProcessingBuildingBehaviour : BuildingBehaviour {
}

public sealed class PlaceResourceBuildingBehaviour : BuildingBehaviour {
}

public sealed class OutsourceHumanBuildingBehaviour : BuildingBehaviour {
    [SerializeField]
    List<EmployeeBehaviour> _behaviours;

    public override bool CanBeRun(BuildingDatabase db) {
        Assert.IsTrue(_behaviours.Count > 0);

        foreach (var behaviour in _behaviours) {
            if (!behaviour.CanBeRun()) {
                return false;
            }
        }

        return true;
    }
}

public abstract class EmployeeBehaviour : MonoBehaviour {
    public virtual bool CanBeRun() {
        return false;
    }

    public virtual void OnEnter(Human human, HumanDatabase db) {
    }

    public virtual void OnExit(Human human, HumanDatabase db) {
    }

    public virtual void Update(Human human, HumanDatabase db, float dt) {
    }
}

public sealed class ChooseDestinationEmployeeBehaviour : EmployeeBehaviour {
    public override void OnEnter(Human human, HumanDatabase db) {
        Controller
    }
}

public sealed class MoveToTheDestinationEmployeeBehaviour : EmployeeBehaviour {
    public override void OnExit(Human human, HumanDatabase db) {
        Assert.AreEqual(human.destination, null);
        base.OnExit(human, db);
    }
}

public sealed class ProcessingEmployeeBehaviour : EmployeeBehaviour {
}

public sealed class 

public class HumanDatabase {
}
}
