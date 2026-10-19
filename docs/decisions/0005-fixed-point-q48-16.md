# ADR-0005: Eigener Fixed-Point-Typ Q48.16 auf `long`

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

Die deterministische Simulation ([ADR-0004](0004-deterministic-fixed-tick-simulation.md)) darf kein `float`/`double` verwenden, weil Ergebnisse über x64 und ARM64 sowie verschiedene Math-Implementierungen nicht garantiert identisch sind. Positionen, Geschwindigkeiten und Distanzen brauchen einen Fixed-Point-Typ. Weltmassstab: **1 Tile = 1,0 Welteinheit.** Auf einer Karte mit 256×256 Tiles liegt die maximale Distanz bei ~362 und ihr Quadrat bei ~131 000.

## Decision Drivers

- Determinismus vollständig kontrollierbar (Rundungsregeln, keine Black Box)
- Wertebereich reicht für Distanzquadrate auf grossen Karten
- Multiplikation ohne 128-Bit-Zwischenwerte
- Lernwert ([ADR-0001](0001-learning-goal-engine-on-demand.md))
- Implementierungsaufwand

## Considered Options

1. Eigener Q48.16 auf `long`
2. FixedMath.Net (`Fix64`, Q31.32)
3. Eigener Q16.16 auf `int`
4. Eigener Q32.32 auf `long`

## Decision Matrix

| Criterion                         | Weight | Q48.16 eigen | FixedMath.Net | Q16.16 `int` | Q32.32 `long` |
| --------------------------------- | ------ | ------------ | ------------- | ------------ | ------------- |
| Determinismus kontrollierbar      | 5      | 5            | 3             | 5            | 5             |
| Wertebereich (Distanzquadrate)    | 5      | 5            | 5             | 1            | 5             |
| Multiplikation ohne 128 Bit       | 3      | 4            | 3             | 5            | 2             |
| Lernwert                          | 3      | 5            | 2             | 5            | 5             |
| Aufwand                           | 1      | 3            | 5             | 3            | 3             |
| **Weighted Total**                |        | **80**       | **60**        | **63**       | **74**        |

## Decision Outcome

**Chosen option:** „Eigener Q48.16 auf `long`“, weil er den grössten Wertebereich pro Aufwand bietet (±1,4 · 10¹⁴, Auflösung 1/65 536), Multiplikationen mit Spielwerten ohne `Int128` auskommen und der Determinismus vollständig in eigener, getesteter Hand liegt.

Umfang in `Rts.Primitives`:

- `readonly struct Fixed` mit `+ − × ÷`, Vergleichen, `Abs`, `Min`/`Max` und `Sqrt` (Ganzzahl-Newton)
- `readonly struct FixedVector2` mit Addition, Skalierung, `LengthSquared`, `Length` und `Normalize`
- **Eine definierte Rundungsregel** (Abschneiden Richtung null), dokumentiert und getestet
- Konvertierung `float → Fixed` nur an Systemgrenzen (Laden von `GameRules`, Tests); `Fixed → float` nur für das Rendering
- `Sin`/`Atan2` erst bei Bedarf, dann über Lookup-Tabellen
- `GridPosition` bleibt `int`. Die Umrechnung Tile ↔ Welt ist eine eigene, getestete Funktion.

### Positive Consequences

- Plattformunabhängig bitgenaue Ergebnisse
- Ideal für TDD: Grenzwerte, Rundung, Überlauf und `Sqrt`-Genauigkeit sind präzise testbar
- Keine externe Abhängigkeit

### Negative Consequences

- Eigener Code muss gepflegt werden, und Fehler in `Sqrt` oder der Division wirken sich überall aus
- Auflösung 1/65 536 statt 1/2³². Bei sehr kleinen Geschwindigkeiten entstehen Rundungsfehler, die bewusst akzeptiert werden.
- Überlaufprüfung kostet Performance. Sie läuft nur in Debug bzw. Tests (`checked`), nicht im Release.

## Pros and Cons of the Options

### Q48.16 eigen

- Good, because der Wertebereich praktisch unbegrenzt ist und `a * b >> 16` für Spielwerte in `long` passt
- Bad, because geringere Nachkommaauflösung als Q32.32

### FixedMath.Net

- Good, because fertig und erprobt
- Bad, because lange ungepflegt und die `Sqrt`/Trig-Genauigkeit gemischt ist
- Bad, because der Lerninhalt ausgelagert wird

### Q16.16 `int`

- Good, because kompakt und schnell
- Bad, because der Wertebereich ±32 768 schon bei Distanz ~181 im Quadrat überläuft

### Q32.32 `long`

- Good, because höchste Präzision
- Bad, because jede Multiplikation 128 Bit braucht (`Math.BigMul`/`Int128`) und langsamer ist

## Action Items

| #   | Action                                                                     | Owner        | Due       |
| --- | -------------------------------------------------------------------------- | ------------ | --------- |
| 1   | `Fixed` und `FixedVector2` per TDD in `Rts.Primitives`                     | Jonas Gerber | Schritt 1 |
| 2   | Tests: Rundung, Grenzwerte, Überlauf (checked), `Sqrt`-Fehlergrenze        | Jonas Gerber | Schritt 1 |
| 3   | Konvertierung Tile ↔ Welt plus Tests                                       | Jonas Gerber | Schritt 3 |

## Links

- [ADR-0004](0004-deterministic-fixed-tick-simulation.md) Deterministische Simulation
- [ADR-0013](0013-test-strategy-os-matrix-golden-hash.md) Nachweis über die OS-Matrix
