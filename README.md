# Cardinal Conflict

A local hot-seat territory strategy game for 2–8 players, built with **Godot 4.5 (C#/.NET)**.

Players take turns on one shared 15×15 board. On your turn you play cards to capture tiles, attack your neighbours and fortify blockades. When everyone has played, all the cards resolve together. To win, own three pyramids that are each fully surrounded by your own tiles. A player who loses every pyramid is eliminated.

## Requirements

- [Godot 4.5 – .NET edition](https://godotengine.org/download) (the standard build cannot run C# projects)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Running

1. Open Godot 4.5 .NET and import this folder (pick `project.godot`).
2. Press **F5**. Godot builds the C# project and starts the game.

To build without opening the editor:

```bash
dotnet build
```

## Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move cursor / menu focus | Arrow keys, or point with the mouse | D-pad |
| Play card / press button | Enter, or left click | A |
| Previous / next card in hand | Mouse wheel up / down | RB / RT |
| Swap attack ↔ magic hand | Tab | Back or LB |
| Take back this turn's plays | Backspace | B |
| Open / close the "End Turn?" menu | Esc | Start |
| Scroll the board | Move the mouse to a screen edge | Right stick |

On the setup screen, type a name, choose a colour with **Color**, then press **New Player**. Repeat for each player (2–8), then press **Start Game**.

## Project layout

```
project.godot          Godot project settings, including the input map
CardinalConflict.csproj
scenes/Main.tscn       Entry scene (one Node running Main.cs)
scripts/Core/          Main, Stage base class, input routing, assets, colours
scripts/Stages/        MenuStage (player setup) and GameStage (a match)
scripts/Board/         Tile grid data, board rendering, target overlay
scripts/Rules/         Players, cards, card targets, the card dealer
scripts/UI/            HUD elements and text helpers
assets/textures/       The sprites the game uses
docs/DESIGN.md         Architecture and class reference
```

See [`docs/DESIGN.md`](docs/DESIGN.md) for how the pieces fit together.
