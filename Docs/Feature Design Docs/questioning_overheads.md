# Questioning Overheads

## The problem or a point of improvement

Category: @Speed

Hulvdan believes that currently performance overheads happen in the way the system reacts to the changes in the `Human`, `Building` behaviours. Several different approaches need to be investigated in order to find the best ways of scructuring a CPU-friendly reaction to the Humans' state changes.

## Draft 1. Firing events through observers (the current approach)

```c++
// HumanProcessingBehaviour.h
public class HumanProcessingBehaviour {
public:
    static void OnExit(Human& human, Dependencies& deps) {
        ...  // Cleaning state's data
        deps.map.OnHumanFinishedProcessing(human);
    }
}

// Map.h
public class Map {
public:
    Subject<E_HumanFinishedProcessing> onHumanFinishedProcessing;

    void OnHumanFinishedProcessing(Human& human) {
        ...  // Doing Map's simulation

        GlobalData.eHumanFinishedProcessing.Human = human;

        // Note: Assuming that this observer internally just
        // iterates over an array of function pointers and calls them
        onHumanFinishedProcessing.Invoke(
            &GlobalData.eHumanFinishedProcessing
        );
    }

    void Update(float dt) {
        for (auto& human : _humans) {
            switch (human.behaviourType) {
                case BehaviourType::Processing:
                    HumanProcessingBehaviour::Update(&human, dt);
                    break;
                ...  // Other behaviours
            }
        }
    }
}

// MapRenderer.h
public class MapRenderer {
public:
    MapRenderer(Map* map) {
        _map = map;
        _map.onHumanFinishedProcessing.Connect(OnHumanFinishedProcessing);
    }

private:
    void OnHumanFinishedProcessing(E_HumanFinishedProcessing& data) {
        ...  // Reacting to it
    }

    Map* _map;
}
```

Questions:
1) Is it a problem that there's an immediate end-to-end call happening from the Human's behaviour down to the MapRenderer?

    I mean, is it bad that `HumanProcessingBehaviour.OnExit()` calls `Map.OnHumanFinishedProcessing()` that goes through a list of function-pointers in the `onHumanFinishedProcessing` observer that calls `MapRenderer.OnHumanFinishedProcessing()`?

    What are the overheads?

    Hulvdan assumes that there could be huge overheads.

2) If there is a lot of thing that get cached during the calls to the MapRenderer, could the processor just drop the Map's cache of the `for` loop?

    Do people just use the queues for that reason? 🤔 It would require some memory for queues pre-allocation, but not for the operation

## Draft 2. TBD

## Conclusion. TBD
