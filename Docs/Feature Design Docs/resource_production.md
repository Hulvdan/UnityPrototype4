# Resource Production

## The problem or a point of improvement

Category: @Logic

Having completed resource transportation system, we need to create a system of resources harvesting / processing.

It is done through making buildings be able to employ humans to do these things.

## Thesaurus

- `Building`
  - `City Hall` is a building that creates Humans
  - `Processing Cycle` is a cycle during which building does it's primary task. For example, `Lumberjack's Hut` will
    check the surrounding for an `unbooked` terrain tile that contains forest. If succeed, building `books` this tile
    and starts a cycle during which it sends an `employee` who moves to the tile, harvests forest, returns to the
    building, places the resource, finalizing the `cycle`.
- `Human`
  - `Constructors` are humans that `construct` buildings
  - `Employees` are humans that are `employed` by buildings

## Draft 1. Building Behaviour

### Description

Constructed, buildings send a signal so that `City Hall` creates an employee that starts moving inside the building.

After employee moved inside the building, we say that "Building has employee inside".

Building can start a `processing cycle` if there is no one in progress and preconditions are met.

Examples of building processing cycle `specific preconditions`:

- `Lumberjack's Hut`. There is an unbooked terrain tile in the specified area around the building that contains forest.
- `Forester's Hut`. There is an unbooked terrain tile in the specified area around the building that forest can be
  planted onto.
- `Fisherman's Hut`. There is an unbooked solid terrain tile in the specified are around the building that is adjacent
  to a lake that's also not booked
- `Mine`. There's a specific food on the building's tile.

Examples of building processing cycle `common preconditions`:

- If building produces resources:
  - There is less than `4` resources that building produces on the tile where building creates them

That leads us to the concepts of `Building Behaviour` and `Human Behaviour`.

`Building Behaviour` is a set of specific tasks that building can perform. Examples of these include:

- Idle
- Processing
  - Consume a resource
  - Processing
  - Produce a resource
- Create an `employee` with a specific set of behaviours & wait for his cycle's completion
- Book a tile

`Employee` contains a set of behaviours:

- Choose a destination
  - Choose a building
  - Choose a terrain tile with a resource that can be harvested
  - Choose a tile where a sapling can be planted
  - Choose a tile where a resource can be picked up
  - Choose a tile where a resource can be placed
- Go to the destination
- Processing
- Pick up a resource
- Place the resource

### Code samples

TBD

## Conclusion

TBD
