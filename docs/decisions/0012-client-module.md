# ADR-0012: Modul `Rts.Client` für Kamera, Auswahl und Input

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Meilenstein 1 braucht Kamerasteuerung, Picking, Klick- und Rahmenauswahl, Rechtsklick-Befehle, Hotkeys, HUD und ein Debug-Overlay. Diese Logik ist weder Simulation (die Auswahl ist Client-Zustand, siehe [ADR-0009](0009-typed-commands-with-sim-validation.md)) noch reines Zeichnen.

## Decision

Ein neues Modul **`src/modules/client/`** (`Rts.Client` + `Rts.Client.Tests`) enthält:

- Kamerazustand und -steuerung, Picking (Mausstrahl → Bodenpunkt → `GridPosition`/`EntityId`)
- Auswahlzustand, Input-Mapping → Commands
- HUD- und Debug-Overlay-Zustand

Regeln:

- `Rts.Client` referenziert `Rts.Simulation.Contracts` und `Rts.Primitives`, **nicht** `Rts.Simulation` ([ADR-0007](0007-snapshot-boundary-with-interpolation.md)).
- Die Mathematik (Picking, Rahmenauswahl, Kamera-Transformationen) besteht aus **reinen Funktionen** und wird per Unit-Test geprüft. Der Raylib-Input-Adapter ist eine dünne Schicht ([ADR-0002](0002-desktop-only-with-raylib-cs.md)).
- **`Rts.Game` ist die Composition Root:** Es setzt Simulation, Client und Rendering zusammen und enthält keine Logik.

## Alternatives Considered

- **In `Rts.Rendering`:** Zeichnen und Eingabeverarbeitung wären vermischt, und die Picking-Logik wäre an Raylib-Draw-Code gekoppelt.
- **In `Rts.Game`:** Der Host wüchse zu einer Gottklasse, und die Logik wäre schwer testbar.

## Consequences

- Die Client-Logik ist ohne Fenster testbar und fällt unter die Coverage-Grenze ([ADR-0013](0013-test-strategy-os-matrix-golden-hash.md)).
- Ein späterer Multiplayer-Client nutzt dasselbe Modul.
- Trade-off: Ein weiteres Projektpaar in der Solution, und die Grenze zwischen „Client-Zustand“ und „Rendering“ muss im Review gehalten werden.
