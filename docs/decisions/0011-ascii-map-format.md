# ADR-0011: ASCII-Kartenformat mit Validierung

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Meilenstein 1 braucht eine Karte mit Bäumen, Dorfzentrum, Start-Dorfbewohnern und Hindernissen. Pfad- und Simulationstests brauchen viele kleine, lesbare Testkarten.

## Decision

Karten werden als **ASCII-Text** definiert und von einem Parser in eine immutable `MapDefinition` übersetzt, aus der das `NavGrid` und die Start-Entities entstehen. Für Meilenstein 1 wird die Karte als eingebettete Ressource bzw. als String geladen, ohne Datei-Dialog.

| Zeichen | Bedeutung                                  |
| ------- | ------------------------------------------ |
| `.`     | begehbar                                   |
| `T`     | Baum (blockiert, Ressource)                |
| `C`     | Dorfzentrum (muss ein vollständiger 3×3-Block sein) |
| `V`     | Start-Dorfbewohner (auf begehbarem Grund)  |
| `#`     | unpassierbar (Fels/Wasser)                 |

Der Parser ist eine **Systemgrenze** und validiert mit klaren Fehlermeldungen (Zeile/Spalte): unbekannte Zeichen, nicht rechteckige Zeilen, ein unvollständiges 3×3-Dorfzentrum, fehlendes Dorfzentrum, leere Eingabe.

## Alternatives Considered

- **Karte im Code:** kein Parser nötig, aber Testkarten sind unlesbar, und Daten wären mit Code vermischt.
- **Prozedural mit Seed:** ein schönes späteres Lernthema, braucht aber einen eigenen deterministischen PRNG und macht Tests weniger gezielt. Ein späterer Generator erzeugt dieselbe `MapDefinition`.

## Consequences

- Testszenarien sind auf einen Blick lesbar (auch für BFS-Tests).
- Der Parser liefert gute Edge-Case-Tests für die Validierung an der Grenze.
- Trade-off: ein Zeichen pro Tile. Mehrere Spieler, Ressourcentypen oder Höhen erfordern später eine Formaterweiterung (Legende oder Wechsel auf JSON).
