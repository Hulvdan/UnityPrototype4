#nullable enable

using System.Collections.Generic;
using BFG.Runtime.Controllers.Human;
using UnityEngine;

namespace BFG.Runtime.Entities {
public abstract class EmployeeBehaviour {
    public virtual bool CanBeRun(
        int behaviourId,
        Building building,
        BuildingDatabase bdb,
        List<Vector2Int> tempBookedTiles
    ) {
        return true;
    }

    public virtual void BookRequiredTiles(
        int behaviourId,
        Building building,
        BuildingDatabase bdb
    ) {
    }

    public virtual void OnEnter(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
    }

    public virtual void OnExit(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db
    ) {
    }

    public virtual void UpdateDt(
        Human human,
        BuildingDatabase bdb,
        HumanDatabase db,
        float dt
    ) {
    }

    public virtual void OnHumanMovedToTheNextTile(Human human, HumanData data, HumanDatabase db) {
    }
}
}
