# ADR-0006: ECS-light mit Ausnahme für mutable Simulationsdaten

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

Einheiten, Gebäude und Ressourcen brauchen ein Datenmodell. RTS-Entities kombinieren Fähigkeiten flexibel (ein Dorfbewohner sammelt, baut, kämpft), was Vererbungshierarchien schnell sprengt. Die Simulation muss deterministisch iterieren ([ADR-0004](0004-deterministic-fixed-tick-simulation.md)).

Zusätzlich gilt in den Projektregeln „Immutability (CRITICAL)“. Pro Tick den gesamten Weltzustand neu zu erzeugen, ist bei tausenden Einheiten und 20 Ticks/s nicht tragbar.

## Decision Drivers

- Volle Kontrolle über die Iterationsreihenfolge (Determinismus)
- Lernwert datenorientiertes Design
- Flexible Komposition von Fähigkeiten
- Aufwand für Meilenstein 1
- Performance bei vielen Einheiten (aktuell nachrangig)

## Considered Options

1. OOP-Hierarchie (`Villager : Unit : Entity` mit virtuellem `Update`)
2. Externe ECS-Bibliothek (Arch, Friflo.Engine.ECS, DefaultEcs)
3. Eigenes minimales ECS-light

## Decision Matrix

| Criterion                     | Weight | OOP    | Externe ECS | ECS-light |
| ----------------------------- | ------ | ------ | ----------- | --------- |
| Determinismus-Kontrolle       | 5      | 4      | 3           | 5         |
| Lernwert DOD                  | 3      | 1      | 2           | 5         |
| Flexible Komposition          | 3      | 1      | 5           | 4         |
| Aufwand Meilenstein 1         | 3      | 5      | 3           | 4         |
| Performance (viele Einheiten) | 1      | 2      | 5           | 3         |
| **Weighted Total**            |        | **43** | **50**      | **67**    |

## Decision Outcome

**Chosen option:** „Eigenes ECS-light“, bewusst minimal:

- `EntityId` = `int` Index + `int` Generation (erkennt veraltete Referenzen)
- Komponenten sind `struct`s in **Sparse Sets** (dichtes Array + Index-Array)
- Systeme sind Funktionen, die in **fester Reihenfolge** und nach **aufsteigender `EntityId`** laufen
- Keine Archetypen, kein Change-Tracking, keine Parallelisierung. Das kommt erst bei Bedarf, belegt durch einen Profiler.
- Meilenstein 1 benötigt etwa `Position`, `Movement`/`Path`, `Gatherer`, `ResourceNode`, `Dropsite` und `ProductionQueue`

**Ausnahme von der Immutability-Regel:** Innerhalb von `Rts.Simulation` sind **mutable Komponenten-Arrays erlaubt**. Die Ausnahme ist eng begrenzt:

- Sie gilt nur für die internen Komponentenspeicher und Systeme
- Alles, was die Simulation verlässt, ist immutable: `WorldSnapshot` ([ADR-0007](0007-snapshot-boundary-with-interpolation.md)), Commands ([ADR-0009](0009-typed-commands-with-sim-validation.md)) und `GameRules` ([ADR-0010](0010-game-rules-injected-record.md))
- Komponentenspeicher sind `internal`, und kein öffentlicher Typ gibt mutable Referenzen heraus

### Positive Consequences

- Determinismus ist durch die ID-Reihenfolge strukturell garantiert
- Fähigkeiten lassen sich frei kombinieren (Komponente hinzufügen statt Unterklasse)
- Ein späterer Wechsel auf Arch oder Friflo bleibt möglich, weil Systeme über eine schmale Schnittstelle zugreifen

### Negative Consequences

- Eigener Infrastruktur-Code (Sparse Set, ID-Recycling) muss geschrieben und getestet werden
- Ohne Archetypen ist die Cache-Effizienz bei Multi-Komponenten-Abfragen schlechter als bei Arch
- Die Immutability-Ausnahme verlangt Disziplin. Reviews müssen darauf achten, dass keine mutable Daten nach aussen gelangen.

## Pros and Cons of the Options

### OOP-Hierarchie

- Good, because sofort verständlich und wenig Infrastruktur
- Bad, because Mehrfachfähigkeiten zu tiefen Hierarchien oder Gottklassen führen
- Bad, because der Zustand über Objekte verstreut ist, was Snapshots und Hashes erschwert

### Externe ECS-Bibliothek

- Good, because schnell, ausgereift und mit Queries
- Bad, because die Iterationsreihenfolge an Archetyp-Umbauten hängt und der Determinismus erst verstanden und abgesichert werden muss
- Bad, because der Lerninhalt ausgelagert wird

### ECS-light

- Good, because Determinismus und Lerninhalt in eigener Hand liegen
- Bad, because weniger Komfort und Performance als ausgereifte Bibliotheken

## Action Items

| #   | Action                                                                           | Owner        | Due       |
| --- | -------------------------------------------------------------------------------- | ------------ | --------- |
| 1   | `EntityId` mit Generation und Sparse-Set-Speicher per TDD                        | Jonas Gerber | Schritt 4 |
| 2   | Test: Zugriff mit veralteter `EntityId` wird erkannt                             | Jonas Gerber | Schritt 4 |
| 3   | Review-Checkpunkt: keine öffentlichen mutable Typen aus `Rts.Simulation`         | Jonas Gerber | Schritt 4 |

## Links

- [ADR-0004](0004-deterministic-fixed-tick-simulation.md) Deterministische Simulation
- [ADR-0007](0007-snapshot-boundary-with-interpolation.md) Snapshot-Grenze
