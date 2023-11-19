using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BFG.Runtime {
public class Human {
    public Human(Guid id, Building building, Vector2 position) {
        ID = id;
        this.building = building;
        this.position = position;
    }

    public readonly Guid ID;
    public Vector2Int MovingFrom;

    public float harvestingElapsed { get; set; }

    public Vector2 position { get; set; }

    public HumanState state { get; set; } = HumanState.Idle;

    public Vector2Int? harvestTilePosition { get; set; }

    [CanBeNull]
    public Building building { get; }

    [CanBeNull]
    public Building storeBuilding { get; set; }

    public Vector2Int movingFrom {
        get => MovingFrom;
        set => MovingFrom = value;
    }
}
}
