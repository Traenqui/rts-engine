# ADR-0004: Deterministische Simulation mit festem Tick und Command-Queue

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

AoE realisiert Multiplayer per **Lockstep**: Übertragen werden nur Befehle, jeder Rechner simuliert selbst, und alle müssen bitgenau dasselbe Ergebnis liefern. Multiplayer ist ein späteres Ziel, Replays und reproduzierbare Headless-Tests sind schon früher nützlich. Determinismus lässt sich nicht nachträglich einbauen, ohne die Simulation weitgehend neu zu schreiben. Die Zielplattformen umfassen x64 und ARM64 ([ADR-0002](0002-desktop-only-with-raylib-cs.md)).

## Decision Drivers

- Spätere Lockstep-Multiplayer- und Replay-Fähigkeit
- Reproduzierbare Tests: gleiche Karte + gleiche Befehle = gleicher Zustand
- Lernwert („wie funktioniert AoE unter der Haube“)
- Aufwand für Meilenstein 1
- Kosten des Nachrüstens, falls jetzt nicht entschieden

## Considered Options

1. Deterministisch: fester Tick, Command-Queue, Fixed-Point
2. Nicht deterministisch: fester Tick mit `float`
3. Variabler Timestep mit `float` (Update pro Frame)

## Decision Matrix

| Criterion                      | Weight | Deterministisch | Fester Tick + float | Variabler Timestep |
| ------------------------------ | ------ | --------------- | ------------------- | ------------------ |
| Multiplayer/Replay-Fähigkeit   | 5      | 5               | 1                   | 1                  |
| Reproduzierbare Tests          | 5      | 5               | 3                   | 1                  |
| Lernwert                       | 3      | 5               | 3                   | 2                  |
| Aufwand Meilenstein 1          | 3      | 2               | 4                   | 5                  |
| Kosten späteren Nachrüstens    | 3      | 5               | 2                   | 1                  |
| **Weighted Total**             |        | **86**          | **47**              | **34**             |

## Decision Outcome

**Chosen option:** „Deterministisch“. Konkret:

- **Fester Tick: 20 Ticks/s** (`TICKS_PER_SECOND` als Konstante), entkoppelt von der Framerate
- **Alle Eingaben als Commands**, gestempelt mit Ziel-Tick ([ADR-0009](0009-typed-commands-with-sim-validation.md)). Die UI ruft nie direkt Simulationslogik auf.
- **Fixed-Point statt `float`** in der Simulation ([ADR-0005](0005-fixed-point-q48-16.md))
- **Deterministische Regeln:** keine Iteration über `Dictionary`/`HashSet` ohne definierte Reihenfolge, kein `System.Random` (eigener PRNG mit Seed, sobald Zufall nötig ist), keine Uhrzeit oder Systemzeit in der Simulation, feste System-Reihenfolge, Iteration nach aufsteigender `EntityId`
- **State-Hash** (z. B. FNV-1a über alle Komponenten) als Desync-Detektor und Testorakel ([ADR-0013](0013-test-strategy-os-matrix-golden-hash.md))
- **`Rts.Jobs` (Multithreading) ist vertagt:** Parallele Systeme müssten reihenfolgeunabhängig sein, und Meilenstein 1 erzeugt keine Last, die das rechtfertigt.
- **Netzwerk ist vertagt** bis zu einem eigenen Meilenstein.

### Positive Consequences

- Replays = Seed + Befehlsliste
- `Rts.Headless` kann ganze Partien als Test abspielen
- Multiplayer ist später „nur“ Netzwerktransport plus Input-Delay, kein Umbau der Simulation

### Negative Consequences

- Höherer Aufwand: eigener Fixed-Point-Typ, Disziplin bei Collections und Reihenfolgen
- Determinismus-Bugs sind subtil und zeigen sich erst über Plattformen hinweg. Die CI-Matrix ist deshalb Pflicht.
- Die Tick-Granularität (50 ms) erzeugt Eingabelatenz, die das Rendering per Interpolation kaschieren muss ([ADR-0007](0007-snapshot-boundary-with-interpolation.md)).

## Pros and Cons of the Options

### Deterministisch

- Good, because Lockstep, Replays und reproduzierbare Tests sind möglich
- Good, because es den zentralen Lerninhalt von RTS-Engines abdeckt
- Bad, because Fixed-Point und strikte Regeln mehr Aufwand bedeuten

### Fester Tick + float

- Good, because einfache Arithmetik, gute Tool-Unterstützung
- Bad, because `float`-Ergebnisse über x64/ARM64 und Math-Implementierungen nicht garantiert identisch sind, was zu Desyncs führt
- Bad, because Tests nur auf derselben Plattform reproduzierbar sind

### Variabler Timestep

- Good, because am einfachsten umzusetzen
- Bad, because Ergebnisse von der Framerate abhängen und weder Lockstep noch Replays möglich sind

## Action Items

| #   | Action                                                                         | Owner        | Due       |
| --- | ------------------------------------------------------------------------------ | ------------ | --------- |
| 1   | Simulationskern mit festem Tick, Command-Queue und State-Hash                  | Jonas Gerber | Schritt 4 |
| 2   | Test: zwei Läufe mit gleicher Eingabe ergeben gleichen Hash                    | Jonas Gerber | Schritt 4 |
| 3   | Determinismus-Regeln als Checkliste im README bzw. in der Simulations-Doku     | Jonas Gerber | Schritt 4 |

## Links

- [ADR-0005](0005-fixed-point-q48-16.md) Fixed-Point
- [ADR-0006](0006-ecs-light-with-mutable-sim-state.md) ECS-light
- [ADR-0013](0013-test-strategy-os-matrix-golden-hash.md) Golden-Hash in der CI
