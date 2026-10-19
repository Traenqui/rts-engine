# ADR-0007: Snapshot-Grenze zwischen Simulation und Rendering mit Interpolation

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

Die Simulation läuft mit 20 Ticks/s ([ADR-0004](0004-deterministic-fixed-tick-simulation.md)), das Rendering mit 60–144 FPS. Ohne klare Grenze ruckeln Einheiten (50-ms-Sprünge), oder Rendering-Code koppelt sich an ECS-Interna ([ADR-0006](0006-ecs-light-with-mutable-sim-state.md)). Zudem darf `float` aus dem Rendering und Picking nie in die Simulation zurückfliessen.

## Decision Drivers

- Harte Entkopplung (auch über Projekt-Referenzen erzwingbar)
- Flüssige Darstellung trotz grobem Tick
- Testbarkeit: Assertions auf einem stabilen Zustandsobjekt
- Geringe Komplexität für Meilenstein 1
- Späterer Umzug der Simulation in einen eigenen Thread

## Considered Options

1. Direkter Lesezugriff des Renderings auf die Komponentenspeicher (read-only Interface)
2. Immutable Snapshot pro Tick, Interpolation, ein Thread
3. Eigener Simulations-Thread mit Triple Buffering

## Decision Matrix

| Criterion                | Weight | Direkter Zugriff | Snapshot | Thread + Triple Buffer |
| ------------------------ | ------ | ---------------- | -------- | ---------------------- |
| Entkopplung              | 5      | 1                | 5        | 5                      |
| Testbarkeit              | 3      | 2                | 5        | 4                      |
| Flüssige Darstellung     | 5      | 2                | 5        | 5                      |
| Geringe Komplexität      | 3      | 5                | 4        | 1                      |
| Erweiterbar auf Thread   | 1      | 1                | 4        | 5                      |
| **Weighted Total**       |        | **37**           | **81**   | **70**                 |

## Decision Outcome

**Chosen option:** „Immutable Snapshot pro Tick, Interpolation, ein Thread“.

```
while running:
    input → Commands (gestempelt mit Ziel-Tick)
    accumulator += frameDelta
    while accumulator >= TICK_DURATION:
        sim.Step(commandsForTick)
        previous = current
        current  = sim.CreateSnapshot()
        accumulator -= TICK_DURATION
    render(previous, current, alpha = accumulator / TICK_DURATION)
```

- `WorldSnapshot` liegt in `Rts.Simulation.Contracts` und ist **immutable** (Records, `ImmutableArray`). Er enthält pro Entity ID, Typ, Spieler, Position, Ausrichtung und Zustand, dazu globale Werte (Ressourcen, Tick, State-Hash).
- **`Rts.Rendering` und `Rts.Client` referenzieren nur `Rts.Simulation.Contracts`, nie `Rts.Simulation`.**
- Die Umrechnung Fixed → `float` passiert im Snapshot bzw. im Rendering.
- Picking erfolgt in `float`, wird aber **vor** dem Erzeugen eines Commands auf `GridPosition` oder `EntityId` quantisiert.
- Die Interpolation läuft linear zwischen `previous` und `current`. Neu gespawnte bzw. entfernte Entities erscheinen bzw. verschwinden ohne Interpolation.

### Positive Consequences

- Die Grenze ist physisch durch die Projekt-Referenzen erzwungen
- Der Headless-Host nutzt denselben `sim.Step()` ohne Rendering, und Tests prüfen auf Snapshots
- Die Immutability-Regel ist genau an der Modulgrenze erfüllt
- Der Weg zu Option 3 steht offen, ohne dass sich das Rendering ändert

### Negative Consequences

- Allokation eines Snapshots pro Tick (bei Meilenstein 1 mit 10–50 Entities irrelevant, später ggf. Pooling)
- Die Darstellung hängt einen Tick (50 ms) hinter der Simulation, ein bewusst akzeptierter Interpolationspreis
- Datenfelder müssen doppelt gepflegt werden (Komponente + Snapshot-Feld)

## Pros and Cons of the Options

### Direkter Zugriff

- Good, because es am wenigsten Code braucht
- Bad, because Rendering-Code ECS-Interna kennt und die Interpolation eigene Vorher-Zustände bräuchte
- Bad, because ein Umzug der Simulation in einen eigenen Thread blockiert ist

### Snapshot pro Tick

- Good, because Entkopplung, Testbarkeit und flüssige Darstellung gemeinsam erreicht werden
- Bad, because pro Tick Allokationen und ein Frame Latenz anfallen

### Thread + Triple Buffering

- Good, because die Simulation nicht durch das Rendering gebremst wird
- Bad, because Synchronisation und Debugging für Meilenstein 1 unnötig komplex sind

## Action Items

| #   | Action                                                                          | Owner        | Due       |
| --- | ------------------------------------------------------------------------------- | ------------ | --------- |
| 1   | `WorldSnapshot` in `Rts.Simulation.Contracts` definieren                        | Jonas Gerber | Schritt 4 |
| 2   | Loop mit Accumulator in `Rts.Game`, Interpolation in `Rts.Rendering`            | Jonas Gerber | Schritt 7 |
| 3   | Architekturtest: Rendering und Client referenzieren `Rts.Simulation` nicht      | Jonas Gerber | Schritt 7 |

## Links

- [ADR-0004](0004-deterministic-fixed-tick-simulation.md) Fester Tick
- [ADR-0006](0006-ecs-light-with-mutable-sim-state.md) ECS-light
- [ADR-0012](0012-client-module.md) Modul `Rts.Client`
