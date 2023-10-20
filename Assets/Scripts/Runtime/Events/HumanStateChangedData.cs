namespace BFG.Runtime {
public class HumanStateChangedData {
    public readonly Human Human;
    public readonly HumanState NewState;
    public readonly HumanState OldState;

    public HumanStateChangedData(Human human, HumanState newState, HumanState oldState) {
        Human = human;
        NewState = newState;
        OldState = oldState;
    }
}
}
