# ADR-0002: Desktop-only mit Raylib-cs

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

Für Meilenstein 1 braucht das Spiel ein Fenster, Maus- und Tastatureingaben und eine 3D-Darstellung (siehe [ADR-0003](0003-3d-fixed-camera-2d-simulation.md)). `Rts.Game` referenziert noch keine Bibliothek, `Rts.Rendering` ist ein Platzhalter. Das Spiel soll plattformübergreifend laufen. Gemeint sind **Windows, Linux und macOS (inkl. Apple Silicon / ARM64)**. Web, Mobile und Konsolen sind ausdrücklich kein Ziel, auch weil sich die RTS-Bedienung (Rahmenauswahl, Rechtsklick-Befehle) nicht direkt auf Touch übertragen lässt.

## Decision Drivers

- Läuft auf Win/Linux/macOS (x64 und ARM64) ohne Sonderbau
- Schreibt keinen Game Loop vor, damit fester Tick und Snapshot-Grenze im eigenen Code bleiben ([ADR-0004](0004-deterministic-fixed-tick-simulation.md), [ADR-0007](0007-snapshot-boundary-with-interpolation.md))
- Kurze Zeit bis zum ersten Bild
- Lernfokus bleibt beim RTS, nicht bei Grafikprogrammierung
- Spätere Erweiterbarkeit auf Web/Mobile (nur geringes Gewicht)

## Considered Options

1. Raylib-cs
2. MonoGame
3. Silk.NET mit eigenem OpenGL/Vulkan-Renderer

## Decision Matrix

| Criterion                      | Weight | Raylib-cs | MonoGame | Silk.NET |
| ------------------------------ | ------ | --------- | -------- | -------- |
| Desktop Win/Linux/macOS        | 5      | 5         | 5        | 5        |
| Eigener Loop / Architektur     | 5      | 5         | 2        | 5        |
| Zeit bis zum ersten Bild       | 3      | 5         | 3        | 1        |
| Lernfokus RTS statt Grafik     | 3      | 5         | 4        | 1        |
| Spätere Web/Mobile-Option      | 1      | 1         | 5        | 3        |
| **Weighted Total**             |        | **81**    | **61**   | **59**   |

## Decision Outcome

**Chosen option:** „Raylib-cs“, weil es die nativen Bibliotheken für alle drei Desktop-Plattformen mitliefert, als dünne Immediate-Mode-API keinen Loop vorschreibt und 3D (Camera3D, `GetMouseRay`, Modelle) ohne Shader-Arbeit unterstützt.

**Kapselungsregel:** Raylib wird ausschliesslich von `Rts.Rendering`, `Rts.Client` (Input-Adapter) und `Rts.Game` referenziert. `Rts.Simulation`, alle `*.Contracts`, `Rts.Primitives`, `Rts.Navigation` und `Rts.Headless` referenzieren Raylib **nie**.

### Positive Consequences

- Schneller Weg zum ersten sichtbaren Ergebnis
- Der Loop gehört uns, Simulation und Rendering bleiben getrennt
- Die Bibliothek ist austauschbar, weil sie nur an der Rendering- und Input-Grenze lebt

### Negative Consequences

- Kein Web/Mobile ohne Bibliothekswechsel
- Raylib-cs ist ein Community-Binding. Updates des nativen Raylib können verzögert ankommen.
- Keine Content Pipeline: Asset-Laden und -Verwaltung müssen wir selbst lösen, sobald Modelle statt Primitiven kommen.

## Pros and Cons of the Options

### Raylib-cs

- Good, because sehr kleine API und native Libs im NuGet-Paket
- Good, because kein Framework-Loop, sondern Aufruf aus dem eigenen Loop
- Bad, because Web/Mobile praktisch nicht möglich
- Bad, because weniger ausgereiftes Tooling (keine Content Pipeline)

### MonoGame

- Good, because ausgereift, mit Content Pipeline, Mobile und Konsolen
- Bad, because `Game`-Klasse und eigener Loop kollidieren mit unserer Host- und Simulationsarchitektur
- Bad, because 3D ist umständlicher als 2D (SpriteBatch-Fokus)

### Silk.NET mit eigenem Renderer

- Good, because maximaler Lerneffekt beim Rendering und volle Kontrolle
- Bad, because wochenlange Grafikarbeit (Shader, Buffer, Kamera) vor dem ersten RTS-Feature
- Bad, because Rendering ist laut [ADR-0001](0001-learning-goal-engine-on-demand.md) nicht der Lernfokus

## Action Items

| #   | Action                                                                                                | Owner        | Due       |
| --- | ----------------------------------------------------------------------------------------------------- | ------------ | --------- |
| 1   | Raylib-cs-Paket in `Rts.Rendering` / `Rts.Game` aufnehmen (vorher Vulnerability-Check)                | Jonas Gerber | Schritt 7 |
| 2   | Architekturtest: Simulation, Contracts, Primitives, Navigation und Headless referenzieren kein Raylib | Jonas Gerber | Schritt 7 |
| 3   | Fenster plus Kamera auf allen drei OS manuell starten                                                 | Jonas Gerber | Schritt 7 |

## Links

- [ADR-0003](0003-3d-fixed-camera-2d-simulation.md) 3D mit fester Schrägkamera
- [ADR-0012](0012-client-module.md) Modul `Rts.Client`
