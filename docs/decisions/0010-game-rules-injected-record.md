# ADR-0010: Spielwerte als injizierter `GameRules`-Record

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Meilenstein 1 braucht Balancing-Werte (Laufgeschwindigkeit, Tragkapazität, Sammelrate, Holz pro Baum, Kosten, Produktionszeit, Startressourcen). Magic Numbers sind laut Projektregeln verboten. Für Lockstep müssen später alle Spieler identische Werte haben.

## Decision

- Ein **immutable `GameRules`-Record** (inkl. `UnitDefinition` usw.) wird der Simulation **per Konstruktor injiziert**. Die Simulation liest nie globale Konstanten.
- **`DefaultRules`** in C# liefert die Werte für Meilenstein 1. Die konkreten Werte stehen in der PRD für Meilenstein 1.
- Werte werden in **`Fixed`** gespeichert ([ADR-0005](0005-fixed-point-q48-16.md)). Angaben pro Sekunde werden **einmal beim Erzeugen** in Werte pro Tick umgerechnet.
- **JSON-Loader auf Bedarf** (vermutlich mit Kampf und Balancing in Meilenstein 2): Er erzeugt denselben Record, mit Validierung an der Systemgrenze.

## Alternatives Considered

- **JSON-Dateien sofort:** Balancing ohne Neukompilieren und Modding wären möglich, aber Parser, Schema und Validierung sind für sieben Werte nicht gerechtfertigt (YAGNI).
- **Globale Konstanten:** am einfachsten, aber Tests können keine eigenen Regeln injizieren.

## Consequences

- Tests injizieren eigene Regeln (z. B. Kapazität 1, Sammelrate 1 Tick) und prüfen Abläufe in wenigen Ticks.
- Der Umstieg auf JSON ändert nichts an der Simulation.
- Trade-off: Balancing erfordert bis dahin Neukompilieren.
- Für Multiplayer ist später ein Hash über `GameRules` beim Spielstart nötig.
