namespace BFG.Runtime {
public class E_HumanStateChanged {
    public readonly Human Human;
    public readonly HumanState NewState;
    public readonly HumanState OldState;

    public E_HumanStateChanged(Human human, HumanState newState, HumanState oldState) {
        Human = human;
        NewState = newState;
        OldState = oldState;
    }
}
}
