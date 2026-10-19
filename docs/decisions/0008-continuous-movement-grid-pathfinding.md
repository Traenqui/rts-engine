# ADR-0008: Kontinuierliche Bewegung, Grid nur für Pfadsuche

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

`NavGrid` (4 Nachbarn) und eine BFS-Pfadsuche existieren bereits. Offen ist, ob Einheiten auf Tiles springen oder sich frei bewegen. Bäume und das Dorfzentrum belegen Tiles und sind nicht begehbar. Gefällte Bäume machen Tiles zur Laufzeit frei.

## Decision

- **Position** ist ein `FixedVector2` in Weltkoordinaten ([ADR-0005](0005-fixed-point-q48-16.md)). Das Grid dient **nur der Pfadsuche**. Einheiten laufen mit fester Geschwindigkeit pro Tick die Wegpunkte (Tile-Mittelpunkte) ab.
- **Pfadsuche:** BFS mit 4 Nachbarn bleibt für Meilenstein 1. Sie wird erweitert um „Ziel = beliebiges begehbares Nachbar-Tile eines blockierten Tiles“.
- **Keine Kollision zwischen Einheiten** in Meilenstein 1, Überlappung ist erlaubt.
- **Keine Pfad-Invalidierung:** In Meilenstein 1 werden Tiles nur frei (gefällte Bäume), nie neu blockiert. Laufende Pfade bleiben gültig. Ein Test hält diese Annahme fest.
- **A\* mit 8 Nachbarn** (ohne Abkürzung über Ecken) ist der erste explizite Kandidat für „nur auf Bedarf“, sobald L-förmige Wege sichtbar stören.

## Alternatives Considered

- **Tile-basierte Bewegung:** einfach und deterministisch, aber Brettspiel-Optik, und mehrere Einheiten pro Tile sind ein Sonderfall.
- **A\* sofort:** Für ein ungewichtetes 4er-Grid auf 64×64 bringt es keinen Nutzen, und es fehlt der Vergleichsanlass.
- **Kollision bzw. Steering sofort:** ein grosses eigenes Thema, das erst mit Armeen relevant wird.

## Consequences

- Die Bewegung sieht AoE-artig aus, und die bestehende BFS-Arbeit bleibt nutzbar.
- Trade-off: L-förmige Wege und überlappende Einheiten sind sichtbar und akzeptiert.
- Gebäudebau (Tiles werden neu blockiert) erfordert später Pfad-Invalidierung. Die Annahme ist per Test dokumentiert und fällt dort bewusst.
