# ADR-0003: 3D mit fester Schrägkamera, Simulation bleibt 2D

**Status:** Accepted | **Date:** 2026-10-19 | **Deciders:** Jonas Gerber

## Context

Das Spiel soll „2,5D“ aussehen. Das kann entweder isometrische 2D-Sprites (AoE 1/2) oder eine 3D-Welt mit fester RTS-Kamera (AoE 3/4) bedeuten. Für ein Solo-Lernprojekt ist die Asset-Erstellung ein realer Engpass.

## Decision

**3D-Welt mit fester Schrägkamera** (~55° Neigung, Verschieben und Zoom in festen Grenzen, vorerst kein Drehen). Meilenstein 1 nutzt **Primitive**: Würfel = Dorfzentrum, Zylinder = Dorfbewohner, Kegel = Baum. Die **Simulation bleibt 2D** in Grid- bzw. Weltkoordinaten (x, y). Erst das Rendering hebt sie in 3D (x, 0, y). Picking erfolgt per Raycast auf die Bodenebene y = 0.

## Alternatives Considered

- **Isometrische 2D-Sprites:** klassische AoE-Optik, aber jede Einheit braucht Sprites in 8 Richtungen mal mehrere Animationen, was ab Tag 1 blockiert. Dazu kommt fehleranfälliges Iso-Picking bei überlappenden Sprites.

## Consequences

- Kein Asset-Engpass. Gratis-Low-Poly-Modelle (z. B. Kenney, Quaternius) sind später ein reiner Austausch im Rendering.
- Die Trennung zwischen Simulation und Rendering wird erzwungen, weil die Simulation die dritte Dimension nie kennt.
- Kamera drehen und zoomen ist später fast kostenlos.
- Trade-off: Die Optik weicht vom klassischen AoE-2-Look ab, und Beleuchtung bzw. das Laden von Modellen sind zusätzliche Themen, sobald Primitive ersetzt werden.
