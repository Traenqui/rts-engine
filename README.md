# RTS Engine

A small Age-of-Empires-style real-time strategy engine in C# / .NET 10, built to learn RTS engine techniques: a deterministic fixed-tick simulation, fixed-point math, an ECS-light data model and lockstep-ready commands. Milestone 1 is the economy loop (villagers gather wood, the town center trains villagers); see the [milestone 1 PRD](docs/prd-milestone-1-economy-loop.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows, Linux or macOS (x64 or ARM64)

## Build and test

```sh
dotnet build Rts.slnx
dotnet test Rts.slnx
```

CI runs the same commands on Windows, Linux and macOS, with warnings as errors.

## Play

```sh
dotnet run --project src/hosts/Rts.Game
dotnet run --project src/hosts/Rts.Game -- --scenario scenarios/move-around-rocks.scenario --debug
```

| Input | Action |
| --- | --- |
| W A S D / arrow keys | Pan the camera |
| Mouse wheel | Zoom |
| Left click | Select a villager or the town center; click on empty ground to deselect |
| Left drag | Select all own villagers inside the box |
| Right click | Move the selected villagers to the clicked tile |
| F3 | Toggle the debug overlay (grid, paths, tick, FPS/TPS, state hash) |

`--screenshot <file.png>` (a path relative to the working directory) saves a frame after about two seconds and exits. Rendering is checked manually with the [rendering checklist](docs/rendering-checklist.md).

## Headless scenarios

`Rts.Headless` plays scenario files from [`scenarios/`](scenarios) without rendering and prints the deterministic end-state hash.

```sh
dotnet run --project src/hosts/Rts.Headless -- run scenarios/move-straight.scenario
dotnet run --project src/hosts/Rts.Headless -- verify scenarios
```

`verify` compares every `*.scenario` with [`scenarios/golden-hashes.txt`](scenarios/golden-hashes.txt) and exits with `1` on any difference. A changed hash means the simulation behaves differently: either a bug, or an intended change. For an intended change, regenerate the file and explain why in the commit message:

```sh
dotnet run --project src/hosts/Rts.Headless -- print scenarios > scenarios/golden-hashes.txt
```

The scenario format is documented in `ScenarioParser` (`src/modules/simulation/Rts.Simulation/Scenarios`).

## Documentation

- [Architecture decisions](docs/decisions/README.md)
- [Determinism checklist](docs/determinism-checklist.md)
- [Rendering checklist](docs/rendering-checklist.md)

Last updated: 2026-10-19
