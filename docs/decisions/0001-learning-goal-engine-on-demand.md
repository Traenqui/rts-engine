# ADR-0001: Lernziel Engine-Technik, Features nur auf Bedarf

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Das Projekt ist ein einfacher Age-of-Empires-Clone, um RTS-Entwicklung zu lernen und zu erforschen. „RTS lernen“ kann zwei Projekte bedeuten: Engine-Technik (Pfadsuche, Simulation, Determinismus, Rendering-Grenzen) oder Gameplay-Systeme (Wirtschaft, Kampf, KI, Balancing). Das Repo enthält bereits eine eigene modulare .NET-10-Engine mit Headless-Host und Job-Modul.

## Decision

Der Schwerpunkt liegt auf **Engine-Technik**. Leitplanke: **Ein Engine-Feature wird erst gebaut, wenn ein konkretes Gameplay-Feature es braucht.** Erster Meilenstein ist der Wirtschaftskreislauf: Dorfbewohner sammelt Holz, Abgabe am vorplatzierten Dorfzentrum, Produktion eines neuen Dorfbewohners. Nur Holz, nur ein Gebäude, kein Gegner.

## Alternatives Considered

- **Gameplay-Fokus mit fertiger Engine (Godot, MonoGame):** schneller spielbar, aber der gewünschte Lerninhalt (Engine-Interna) würde übersprungen. Die bestehende Engine-Struktur wäre hinfällig.
- **Engine-Technik ohne Spielziel:** maximaler Freiraum, aber hohes Risiko für Infrastruktur auf Vorrat (z. B. ein Job-System ohne Last).
- **Kampfkreislauf als Meilenstein 1:** erfordert sofort einen Gegner bzw. eine KI und lässt den Kern von AoE (Wirtschaft) aus.

## Consequences

- Jedes Feature hat einen messbaren Anlass. YAGNI ist strukturell verankert.
- Vertagte Themen sind explizit: Multithreading (`Rts.Jobs`), Netzwerk, Kollision zwischen Einheiten, Gebäudebau, Kampf, prozedurale Karten.
- Trade-off: Manche Refactorings kommen später zwangsläufig (z. B. BFS → A\*). Sie werden bewusst in Kauf genommen statt vorab gebaut.
- Gameplay-Details (Grenzfälle, Balancing-Werte, Client-Umfang, Schrittfolge) gehören in PRD bzw. Roadmap, nicht in ADRs.
