# ADR-0009: Typisierte Commands, Auflösung im Client, Validierung in der Simulation

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Im Lockstep ([ADR-0004](0004-deterministic-fixed-tick-simulation.md)) sind Commands die einzige Tür in die Simulation. Später kommen sie über das Netzwerk und sind dann **nicht vertrauenswürdig**. Ein Rechtsklick ist kontextabhängig: Baum = sammeln, Boden = laufen.

## Decision

- **Typisierte, immutable Commands** in `Rts.Simulation.Contracts` als Records mit gemeinsamer Basis, z. B.:
  - `Command(PlayerId Issuer, int TargetTick)`
  - `MoveCommand(ImmutableArray<EntityId> Units, FixedVector2 Target)`
  - `GatherCommand(ImmutableArray<EntityId> Units, EntityId Node)`
  - `TrainCommand(EntityId Building, UnitType Type)`
- **Kontextauflösung im Client:** Picking erkennt das Ziel und erzeugt den passenden Command-Typ.
- **Validierung in der Simulation** bei der Ausführung: Ownership (`Issuer` besitzt die Einheit), Existenz (die Entity-Generation passt) und Gültigkeit des Ziels (z. B. genug Holz für `Train`).
- **Ungültige Commands werden deterministisch verworfen**, ohne Exception. Ein Zähler pro Grund und ein Log-Event dokumentieren das. Das ist eine definierte Regel und kein Schlucken von Fehlern, denn ein fehlerhaftes Netzwerkpaket darf den Lockstep nie abbrechen.
- **`PlayerId` ab Tag 1**, auch mit nur einem Spieler.
- **Die Auswahl ist reiner Client-Zustand** und gelangt nie in die Simulation.

## Alternatives Considered

- **Generischer `SmartCommand(target)`, aufgelöst in der Simulation:** Die Simulation müsste UI-Semantik kennen, und Tests wären weniger eindeutig.
- **Exceptions bei ungültigen Commands:** Im Lockstep würde ein manipuliertes oder verspätetes Paket die Partie aller Spieler abbrechen.
- **Keine Validierung im Einzelspieler:** Die Sicherheitslücke würde mit Multiplayer erst später sichtbar und schwer nachzurüsten.

## Consequences

- Jeder Command-Typ hat genau eine Bedeutung und ist einzeln testbar.
- Ownership-Prüfungen sind von Anfang an vorhanden (Grundlage gegen Cheating im späteren Multiplayer).
- Trade-off: Neue Interaktionen erfordern jeweils einen neuen Command-Typ plus Validierung.
- Verworfene Commands müssen sichtbar bleiben (Zähler im Debug-Overlay), sonst bleiben Client-Bugs unbemerkt.
