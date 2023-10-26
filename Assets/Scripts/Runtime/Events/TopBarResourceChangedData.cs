namespace BFG.Runtime {
public record TopBarResourceChangedData {
    public int NewAmount;
    public int OldAmount;
    public ScriptableResource Resource;
}
}
