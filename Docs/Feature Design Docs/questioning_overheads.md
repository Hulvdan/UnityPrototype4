# Questioning Overheads

## The problem or a point of improvement

Category: @Speed

Hulvdan believes that currently performance overheads happen in the way the system reacts to the changes in the `Human`, `Building` behaviours. Several different approaches need to be investigated in order to find the best ways of scructuring a CPU-friendly reaction to the Humans' state changes.

## Draft 1. Firing events through observers (the current approach)

```csharp
public static class HumanProcessingBehaviour {
    public static void OnExit(Human& human, Dependencies& deps) {
        ...  // Cleaning state's data
        deps.map.OnHumanFinishedProcessing(human);
    }
}
```

```csharp
public class Map {
    public Subject<E_HumanFinishedProcessing> onHumanFinishedProcessing;

    public void OnHumanFinishedProcessing(Human& human) {
        ...  // Doing Map's simulation

        GlobalData.eHumanFinishedProcessing.Human = human;

        // Note: Assuming that this observer internally just
        // iterates over an array of function pointers and calls them
        onHumanFinishedProcessing.Invoke(
            &GlobalData.eHumanFinishedProcessing
        );
    }
}
```

```csharp
public class MapRenderer {
    void Init() {
        _map.onHumanFinishedProcessing.Connect(OnHumanFinishedProcessing);
    }

    void OnHumanFinishedProcessing(E_HumanFinishedProcessing& data) {
        ...  // Reacting to it
    }

    Map _map;
}
```

Questions:
1) Is it a problem that there's an immediate end-to-end call happening from the Human's behaviour down to the MapRenderer?

    I mean, is it bad that `HumanProcessingBehaviour.OnExit()` calls `Map.OnHumanFinishedProcessing()` that goes through a list of function-pointers in the `onHumanFinishedProcessing` observer that calls `MapRenderer.OnHumanFinishedProcessing()`?

    What are the overheads?

    Hulvdan assumes that there could be huge overheads.

## Draft 2. TBD

## Conclusion. TBD
