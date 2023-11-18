using System;
using JetBrains.Annotations;
using UnityEngine;

namespace BFG.Runtime {
public enum HumanRole {
    Transporter,
    Harvester,
}

public class Human {
    public Human(Guid id, Building cityHall, GraphSegment segment) {
        ID = id;
        role = HumanRole.Transporter;
        building = cityHall;
        this.segment = segment;
        position = position;
    }

    public Human(Guid id, Building building, Vector2 position) {
        ID = id;
        role = HumanRole.Harvester;
        this.building = building;
        this.position = position;
    }

    public readonly Guid ID;
    public HumanRole role;
    public Vector2Int MovingFrom;

    [CanBeNull]
    public GraphSegment segment { get; }

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
