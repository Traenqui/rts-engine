# ADR-0014: Navigation als Core-Bibliothek statt Feature-Modul

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Die Simulation braucht `NavGrid` und die Pfadsuche: Der Kartenparser baut das Grid ([ADR-0011](0011-ascii-map-format.md)), und Einheiten suchen Pfade ([ADR-0008](0008-continuous-movement-grid-pathfinding.md)). Die Projektregel für modulare .NET-Lösungen sagt, dass Module sich nur über `*.Contracts` referenzieren. `NavGrid` und `BreadthFirstSearch` lagen aber in der Implementierung `src/modules/navigation/Rts.Navigation`, `Rts.Navigation.Contracts` enthielt nur `GridPosition`.

## Decision

Navigation ist eine **deterministische Algorithmenbibliothek ohne eigenen Spielzustand oder eigene Lebensdauer**, vergleichbar mit `Rts.Primitives`. Sie wird deshalb nach **`src/core/Navigation/`** verschoben:

- `Rts.Navigation` (ein Projekt) enthält `GridPosition`, `NavGrid` und `BreadthFirstSearch` (jetzt `public`). `Rts.Navigation.Contracts` entfällt.
- Feature-Module (`Rts.Simulation`, `Rts.Client`, `Rts.Rendering`) dürfen `Rts.Navigation` direkt referenzieren.
- `Rts.Navigation` referenziert höchstens andere Core-Bibliotheken (`Rts.Primitives`), **nie** ein Feature-Modul oder Raylib.
- Die Modulregel „nur über Contracts“ gilt unverändert für Feature-Module untereinander.

## Alternatives Considered

- **Interfaces in `Navigation.Contracts` (`INavGrid`, `IPathfinder`), Verdrahtung im Host:** Das hält die Modulregel formal ein, ist aber eine Abstraktion ohne zweiten Implementierer (YAGNI) und kostet pro Pfadsuche indirekte Aufrufe.
- **Dokumentierte Ausnahme (Simulation referenziert `Rts.Navigation` direkt):** Die Struktur würde etwas anderes behaupten, als tatsächlich gilt („Modul“, das keines ist).

## Consequences

- Einfache Abhängigkeiten: Simulation → Navigation → Primitives.
- Die Pfadsuche ist ohne Mocking direkt testbar und nutzbar.
- Trade-off: Bekommt Navigation später eigenen Zustand (z. B. Flow-Field-Caches, Hierarchical Pathfinding mit Lebensdauer), muss neu bewertet werden, ob sie wieder ein Modul mit Contracts wird.
