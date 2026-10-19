# ADR-0013: Teststrategie mit OS-Matrix, Golden-Hash und Coverage-Ausnahme

| Field        | Value        |
| ------------ | ------------ |
| **Status**   | Accepted     |
| **Date**     | 2026-10-19   |
| **Deciders** | Jonas Gerber |

## Context and Problem Statement

Die Projektregeln verlangen Unit-, Integrations- und E2E-Tests sowie 80 % Coverage. Bei einem Spiel ist offen, was davon was bedeutet. Das grösste technische Risiko ist der plattformübergreifende Determinismus (x64 vs. ARM64, [ADR-0004](0004-deterministic-fixed-tick-simulation.md), [ADR-0005](0005-fixed-point-q48-16.md)), und der lässt sich nur durch Ausführung auf allen Plattformen nachweisen. Vorhanden sind xUnit 2.9.3 und coverlet, aber noch kein CI.

## Decision Drivers

- Plattformübergreifender Determinismus-Nachweis
- Schnelles Feedback bei geringem Pflegeaufwand
- Aussagekraft der Coverage-Kennzahl
- Erfüllung der Projektregeln

## Considered Options

1. Nur lokale Tests, kein CI
2. CI-Matrix Win/Linux/macOS + Golden-Hash, 80 % Coverage ohne Raylib-Schichten
3. CI-Matrix + Golden-Hash, 80 % Coverage auf alle Projekte

## Decision Matrix

| Criterion                        | Weight | Nur lokal | Matrix + Ausnahme | Matrix + 80 % überall |
| -------------------------------- | ------ | --------- | ----------------- | --------------------- |
| Determinismus-Nachweis           | 5      | 1         | 5                 | 5                     |
| Feedback / Aufwand               | 3      | 5         | 3                 | 2                     |
| Aussagekraft Coverage            | 3      | 3         | 5                 | 2                     |
| Erfüllung der Projektregeln      | 3      | 2         | 4                 | 5                     |
| **Weighted Total**               |        | **35**    | **61**            | **52**                |

## Decision Outcome

**Chosen option:** „CI-Matrix + Golden-Hash, Coverage-Ausnahme für die Raylib-Schichten“.

| Ebene           | Inhalt                                                                                                                           | Ort                    |
| --------------- | -------------------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| Unit            | `Fixed`, BFS, ASCII-Parser inkl. Fehlerfälle, einzelne Systeme mit injizierten `GameRules`, Command-Validierung, Client-Mathematik | `*.Tests` pro Modul    |
| Integration     | ASCII-Karte + Command-Skript über N Ticks → Assertions auf `WorldSnapshot`                                                        | `Rts.Simulation.Tests` |
| E2E             | `Rts.Headless` spielt ein Szenario ab und gibt den End-State-Hash aus. Ein **Golden-Hash** ist im Repo eingecheckt.              | Headless-CLI + Test    |
| Manuell         | Sichtbares Rendering nach Checkliste                                                                                              | —                      |

- **GitHub Actions** mit Matrix `windows-latest`, `ubuntu-latest` und `macos-latest` (ARM64): Build, Tests, Headless-Szenario. **Alle drei müssen denselben Hash liefern.**
- **Coverage ≥ 80 %** für `Rts.Primitives`, `Rts.Navigation`, `Rts.Simulation` und die reinen Funktionen in `Rts.Client`. **Ausgenommen:** `Rts.Rendering`, der Raylib-Input-Adapter in `Rts.Client` und `Rts.Game`.
- TDD verpflichtend für `Fixed`, den Parser und die Simulationssysteme.
- Testprojekte heissen einheitlich `*.Tests`.

### Positive Consequences

- Desyncs werden im kleinsten möglichen Codestand entdeckt (CI ab Schritt 2, Golden-Hash ab Schritt 6)
- Die Coverage-Zahl misst Logik statt Draw-Calls
- Der Golden-Hash dient als Regressionsschutz gegen unbeabsichtigte Verhaltensänderungen

### Negative Consequences

- Jede bewusste Verhaltensänderung erfordert ein Update des Golden-Hash. Das muss im Commit begründet werden.
- macOS-Runner sind langsamer und bei privaten Repos teurer (Minutenkontingent)
- Rendering-Regressionen werden nur manuell entdeckt

## Pros and Cons of the Options

### Nur lokal

- Good, because es keinen Pipeline-Aufwand gibt
- Bad, because ARM64-Determinismus nie geprüft wird und ein Desync erst im Multiplayer auffällt

### Matrix + Ausnahme

- Good, because der Determinismus bewiesen ist und die Coverage aussagekräftig bleibt
- Bad, because die Ausnahme dokumentiert und im Review verteidigt werden muss

### Matrix + 80 % überall

- Good, because die Regel formal vollständig erfüllt ist
- Bad, because Tests gegen Raylib-Draw-Calls keinen Erkenntnisgewinn bringen und Mocks der nativen Bibliothek nötig wären

## Action Items

| #   | Action                                                                          | Owner        | Due       |
| --- | ------------------------------------------------------------------------------- | ------------ | --------- |
| 1   | `Rts.Jobs.Test` und `Rts.Primitives.Test` auf `*.Tests` umbenennen              | Jonas Gerber | Schritt 1 |
| 2   | GitHub-Actions-Workflow mit OS-Matrix (Build + Test + Coverage-Report)          | Jonas Gerber | Schritt 2 |
| 3   | Headless-Szenario-Runner, Golden-Hash-Datei und Hash-Vergleich in der CI        | Jonas Gerber | Schritt 6 |
| 4   | Coverage-Ausschluss für die Raylib-Schichten konfigurieren                      | Jonas Gerber | Schritt 7 |
| 5   | Manuelle Rendering-Checkliste anlegen                                           | Jonas Gerber | Schritt 7 |

## Links

- [ADR-0004](0004-deterministic-fixed-tick-simulation.md) Deterministische Simulation
- [ADR-0005](0005-fixed-point-q48-16.md) Fixed-Point
- [ADR-0012](0012-client-module.md) Modul `Rts.Client`
