
# Development

## Coding Styleguide

### Terms

- `Tile` - A tile on the grid. Using the word `Cell` is **prohibited** for naming anything related to the tiles on the grid
- `Progress` - A not curve affected coefficient. Usually represents `elapsed / duration`
- `t` - Coefficients used in Lerp operations. Usually affected by curves

## Setting Up The Machine

Due to licensing issues `3.1.14.3` version of `Odin Inspector and Serializer` must be installed manually at `Assets/VendorLicensed/Odin Inspector/`.

Unity Version: `2022.3.13f1`

1. Clone the repository
2. Open up the project. You will see entering SAFE MODE dialog, ignore it
3. Click `NuGet` -> `Restore Packages`
4. Reopen the project

## Rendering. Order in Layer

- -1 - Background
- 0 - Terrain
- 100 - Roads
- 200 - Buildings, Humans
- 400 - Items
- 500 - Preview
- 2000 - Debug_UnwalkableCells
- 2100 - Debug_MovementSystemTilemap
