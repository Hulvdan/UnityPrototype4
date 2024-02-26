# Roads of Horses

> There is [a WIP C++ port of this repo](https://github.com/Hulvdan/handmade-cpp-game)

## Overview

This is a gamedev project in Unity, C# with a primary goal of honing my development skills.

Preview can be found at [hulvdan.github.io](https://hulvdan.github.io/).

Documentation can be found at [hulvdan.github.io/RoadsOfHorses](https://hulvdan.github.io/RoadsOfHorses/).

## Applied Skills / Fun Programming Things

### Tracing

I found a huge (HUGE) benefit of tracing logs that indent log statements ~ based on the call stack's depth. Debugging state machines became a breeze after I applied this approach.

A simplified example of what am I talking about:

```js
// THE PROGRAM
function outer() {
  var _ = Tracing.Scope()

  Tracing.Log("Trace from outer() before inner()")
  inner()
  Tracing.Log("Trace from outer() after inner()")

function inner() {
  var _ = Tracing.Scope()

  Tracing.Log("Trace from inner()")
}
```

```
// TRACING OUTPUT - Tracing.log
[outer]
Trace from outer() before inner()
  [inner]
  Trace from inner()
Trace from outer() after inner()
```

### Code Generation

Having read "The Pragmatic Programmer", I started exploring ways of reducing amount of time needed for code maintenance.

I used `python` + `jinja` for generating QoL code for `Vector2`, `Vector2Int`, `Vector3`, `Vector3Int` at `Assets/Scripts/Runtime/Extensions`.
