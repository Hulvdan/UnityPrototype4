# Roads of Horses

This is a gamedev project in Unity, C# with a primary goal of honing my development skills.

Preview can be found at [hulvdan.github.io](https://hulvdan.github.io/).

Documentation can be found at [hulvdan.github.io/roads-of-horses](https://hulvdan.github.io/RoadsOfHorses/).

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

## Coding Styleguide

### Terms

- `Tile` - A tile on the grid. Using the word `Cell` is **prohibited** for naming anything related to the tiles on the grid
- `Progress` - A not curve affected coefficient. Usually represents `elapsed / duration`
- `t` - Coefficients used in Lerp operations. Usually affected by curves

## Development

### Setting Up The Machine

Due to licensing issues `3.1.14.3` version of `Odin Inspector and Serializer` must be installed manually at `Assets/VendorLicensed/Odin Inspector/`.

Unity Version: `2022.3.13f1`

1. Clone the repository
2. Open up the project. You will see entering SAFE MODE dialog, ignore it
3. Click `NuGet` -> `Restore Packages`
4. Reopen the project

### Rendering. Order in Layer

- -1 - Background
- 0 - Terrain
- 100 - Roads
- 200 - Buildings, Humans
- 300 - Horse, Wagons
- 400 - Items
- 500 - Preview
- 2000 - Debug_UnwalkableCells
- 2100 - Debug_MovementSystemTilemap

## External Assets

### Used in the project

- [Itch.io. PixelHole's Overworld Tileset](https://pixelhole.itch.io/pixelholes-overworld-tileset)
- [whtdragon's animals and running horses- now with more dragons!](https://forums.rpgmakerweb.com/index.php?threads/whtdragons-animals-and-running-horses-now-with-more-dragons.53552/)
- [Itch.io. caves-rails-tileset](https://heyitswidmo.itch.io/caves-rails-tileset)
- [Improved horses ( 4 colors ) for CP](https://www.nexusmods.com/stardewvalley/mods/1903?tab=description)
- [Roguelike/RPG Items](https://opengameart.org/content/roguelikerpg-items)
- [Pixel Items Inventory](https://www.deviantart.com/blackkarma3840/art/Pixel-Items-Inventory-882911608)
- [free flag with animation](https://ankousse26.itch.io/free-flag-with-animation)

### Considered to be used

- [Itch.io. Mo's Trolley Follies Levels (C64) Commodore 64](https://modernart.itch.io/mos-trolley-follies-levels-c64)
- [Itch.io. World Map Tiles [16x16]](https://malibudarby.itch.io/world-map-tiles)
- [Itch.io. tagged Tilemap and Top-Down](https://itch.io/game-assets/tag-tilemap/tag-top-down)
- [Itch.io. CC BY 4.0 Deed - fantasy rpg icon pack](https://franuka.itch.io/rpg-icon-pack-demo)
- [Itch.io. Cryo's Mini Characters](https://paperhatlizard.itch.io/cryos-mini-characters)
- [Itch.io. Cryo's Mini GUI](https://paperhatlizard.itch.io/cryos-mini-gui)
- [Itch.io. Pixel Art GUI Elements](https://mounirtohami.itch.io/pixel-art-gui-elements)
- [Itch.io. ultimate-ui-pixelart-icons](https://lucky-loops.itch.io/ultimate-ui-pixelart-icons)
- [Itch.io. Flat game user interface FREE](https://sungraphica.itch.io/flat-game-user-interface-free)
- [Itch.io. 16x16 RPG Item Pack](https://alexs-assets.itch.io/16x16-rpg-item-pack)
- [Itch.io. 16x16 RPG Assets](https://ssugmi.itch.io/16x16-rpg-assets)
- [Itch.io. Free 16x16 Mini World Sprites](https://merchant-shade.itch.io/16x16-mini-world-sprites)
