# Questioning Overheads

## The problem or a point of improvement

Category: Speed

Hulvdan believes that at the moment performance overheads happen in the way the MapRenderer gets notified about the
changes in the `Human` and `Building` behaviours.

Several different approaches need to be investigated in order to find the best ways of structuring a CPU-friendly
reaction to the Humans' state changes.

## Draft 1. Firing events through observers immediately (the current approach)

```cpp
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
    Observer<E_HumanFinishedProcessing> onHumanFinishedProcessing;

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

Is it a problem that there's an immediate end-to-end call happening from the Human's behaviour down to the MapRenderer?

I mean, is it bad that `HumanProcessingBehaviour.OnExit()` calls `Map.OnHumanFinishedProcessing()` that goes through a
list of function-pointers in the `onHumanFinishedProcessing` observer that
calls `MapRenderer.OnHumanFinishedProcessing()`?

Hulvdan assumes that there could be huge overheads:

If there is a lot of things that get cached during the calls to the MapRenderer, could the processor just drop the Map's
cache of the `for` loop and make the program 100x slower?

Do people just use the queues for that reason? 🤔 I imagine it requiring some memory for queues pre-allocation, but not
for the operation, which is a good thing

## Draft 2. Firing events through observers after the simulation

```cpp
// HumanProcessingBehaviour.h
public class HumanProcessingBehaviour {
public:
    static void OnExit(Human& human, Dependencies& deps) {
        ...  // Cleaning state's data
        deps.map.AddEvent(new E_HumanFinishedProcessing(human));
    }
}

// Map.h
public class Map {
public:
    Observer<E_HumanFinishedProcessing> onHumanFinishedProcessing;

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

        for (auto& evType : _queue) {
            switch (evType) {
                case EventType::E_HumanFinishedProcessing:
                    const auto& ev = _eventSpecificQueue.next();
                    onHumanFinishedProcessing.Invoke(ev);
                    break;
                ...  // other event types
            }
        }

        _queue.clear();
        _eventSpecificQueue.clear();
    }

    void AddEvent(E_HumanFinishedProcessing ev) {
        _queue.Add(EventType::E_HumanFinishedProcessing);
        _eventSpecificQueue.Add(ev);
    }

    enum EventType {
        E_HumanFinishedProcessing,
    }

    // NOTE: It can be refactored into an array of bytes where reading
    // the leading byte determines which event was fired, thus, making
    // us able to store differrenly sized event structs in the queue.
    // However, it's not the point of this document
    superduper::Queue<EventType> _queue;

    superduper::Queue<E_HumanFinishedProcessing> _eventSpecificQueue;
    // superduper::Queue<E_HumanStartedProcessing> _eventSpecificQueue2;
    // superduper::Queue<E_HumanStartedWalking> _eventSpecificQueue3;
    // superduper::Queue<E_HumanStoppedWalking> _eventSpecificQueue4;
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

Hulvdan thinks that using queues of events and going through observers AFTER the simulation won't spontaneously decrease
the speed of simulation because of the MapRenderer's demanding CPU requirements that could override the CPU cache.

Is it correct?

## 3. More Questions

Let's imagine that we still use the same approach as in Ex. 1. Event gets handled by the renderer that takes requires
heavy computation and erases the CPU cache of the simnulation's logic.

Is there any way to do this but prevent renderer from messing with CPU cache of the simulation?

## Conclusion. TBD

## References:

1. [EnTT. Crash Course: entity-component system](https://skypjack.github.io/entt/md_docs_md_entity.html)
2. [EnTT. Crash Course: events, signals and everything in between](https://github.com/skypjack/entt/wiki/Crash-Course:-events,-signals-and-everything-in-between#named-queues)
