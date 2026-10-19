# Rendering-Checkliste

Raylib-Zeichencode ist bewusst von der Coverage ausgenommen ([ADR-0013](decisions/0013-test-strategy-os-matrix-golden-hash.md)). Diese Checkliste ersetzt die fehlenden automatischen Tests: vor jedem Merge, der `Rts.Rendering`, `Rts.Client` oder `Rts.Game` ändert, einmal durchgehen.

## Vorbereitung

```sh
dotnet run --project src/hosts/Rts.Game
dotnet run --project src/hosts/Rts.Game -- --scenario scenarios/move-around-rocks.scenario --debug
```

Für eine schnelle Sichtprüfung ohne Bedienung speichert `--screenshot <datei.png>` nach etwa zwei Sekunden ein Bild und beendet das Spiel.

## Darstellung (US-01)

- [ ] Das Fenster öffnet sich ohne Fehler im Terminal
- [ ] Kartenzeile 0 liegt oben; Bäume, Felsen, Dorfzentrum und Dorfbewohner stehen an den Positionen der Kartendatei
- [ ] Dorfzentrum ist ein 3×3-Würfel, Dorfbewohner sind Zylinder, Bäume Kegel, Felsen graue Blöcke

## Kamera (US-02)

- [ ] WASD und Pfeiltasten verschieben die Kamera in die erwartete Bildschirmrichtung
- [ ] Mausrad zoomt; Zoom stoppt an den Grenzen
- [ ] Die Kamera lässt sich nicht über den Kartenrand hinaus verschieben
- [ ] Kleine und grosse Karten starten formatfüllend

## Auswahl und Bewegung (US-03, US-04)

- [ ] Linksklick auf einen Dorfbewohner wählt nur ihn aus, ein weisser Ring liegt flach am Boden
- [ ] Ein Klick auf Kopf oder Füsse des Dorfbewohners trifft beide
- [ ] Linksklick auf das Dorfzentrum wählt es aus, Klick auf leeren Boden, Baum oder Felsen hebt die Auswahl auf
- [ ] Ein Rahmen zeigt beim Ziehen ein halbtransparentes Rechteck und wählt nur die eigenen Dorfbewohner darin aus
- [ ] Rechtsklick auf ein freies Tile schickt die ausgewählten Dorfbewohner per Pfad dorthin
- [ ] Rechtsklick ohne Auswahl, mit nur dem Dorfzentrum oder neben die Karte erzeugt keinen Command (Overlay: verworfene Commands bleiben gleich)

## Bewegung und Overlay (US-09)

- [ ] Einheiten im Szenario gleiten flüssig, ohne 20-Hz-Ruckeln
- [ ] F3 schaltet das Overlay ein und aus
- [ ] Overlay zeigt Grid, gelbe Pfade, Tick, FPS ≈ 60, TPS = 20, State-Hash, verworfene Commands und Anzahl Entities
- [ ] Der Hash bei gleichem Tick ist zwischen zwei Läufen desselben Szenarios identisch

## Robustheit

- [ ] Fenster verschieben oder kurz anhalten: Danach läuft das Spiel ohne Hänger weiter (max. 5 Ticks pro Frame)
- [ ] `--scenario` mit fehlender oder ungültiger Datei beendet mit verständlicher Meldung und Exit-Code 2

Last updated: 2026-10-19
