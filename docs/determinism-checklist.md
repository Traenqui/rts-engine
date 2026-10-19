# Determinismus-Checkliste

Die Simulation muss auf Windows, Linux und macOS (x64 und ARM64) bitgenau dasselbe Ergebnis liefern, sonst laufen Lockstep-Multiplayer und Replays auseinander ([ADR-0004](decisions/0004-deterministic-fixed-tick-simulation.md)). Diese Checkliste gilt für jeden Code in `Rts.Simulation`, `Rts.Navigation` und `Rts.Primitives` und ist Teil jedes Reviews.

## Zahlen

- Kein `float`, `double` oder `decimal` in der Simulation, nur `Fixed` und `FixedVector2` ([ADR-0005](decisions/0005-fixed-point-q48-16.md))
- Fraktionale Konstanten über `Fixed.FromRatio(...)`, nie über Gleitkomma-Literale
- `Fixed.ToSingle()` nur im Rendering; das Ergebnis fliesst nie zurück
- Kein `Math.Sin`, `Math.Sqrt` oder andere `System.Math`-Gleitkommafunktionen in der Simulation

## Reihenfolge

- Systeme laufen in fester Reihenfolge, Entities nach aufsteigender `EntityId` (`ComponentStore.EntitiesInIdOrder()`)
- Keine Iteration über `Dictionary`, `HashSet` oder `ConcurrentDictionary`, deren Reihenfolge das Ergebnis beeinflusst; als reine Lookups sind sie erlaubt
- `SortedDictionary` und Listen mit definierter Einfügereihenfolge sind erlaubt
- Kein Multithreading in der Simulation, solange `Rts.Jobs` vertagt ist

## Zustand und Eingaben

- Alle Eingaben kommen als `Command` mit `TargetTick`; nichts ruft Simulationslogik direkt auf ([ADR-0009](decisions/0009-typed-commands-with-sim-validation.md))
- Kein `DateTime`, `Stopwatch`, `Environment.TickCount` oder andere Uhrzeit in der Simulation
- Kein `System.Random`; Zufall nur über einen eigenen PRNG mit Seed, sobald er gebraucht wird
- Kein `GetHashCode()` für Spielzustand oder State-Hash; er ist pro Prozess randomisiert. Der State-Hash nutzt `StateHasher` (FNV-1a)
- Jedes neue Feld im Simulationszustand wird in `GameSimulation.ComputeStateHash` aufgenommen

## Nachweis

- Ein Test vergleicht zwei Läufe mit identischen Eingaben über den State-Hash
- Die CI-Matrix führt alle Tests auf drei Betriebssystemen aus; ab Schritt 6 vergleicht sie zusätzlich einen Golden-Hash ([ADR-0013](decisions/0013-test-strategy-os-matrix-golden-hash.md))

Last updated: 2026-10-19
