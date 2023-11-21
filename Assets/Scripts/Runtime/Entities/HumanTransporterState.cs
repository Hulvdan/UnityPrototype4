namespace BFG.Runtime {
public enum HumanTransporterState {
    // ReSharper disable once InconsistentNaming
    Idle_NothingToDo,
    Idle_NoSegmentFound,
    MovingToCenter,
    Transporting,
    MovingToCityHall,
    MovingToSegment,
}
}
