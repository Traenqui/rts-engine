using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Raylib_cs;
using Rts.Navigation.Grid;
using Rts.Rendering.Contracts;
using Rts.Rendering.Scene;
using Rts.Simulation.Contracts;
using static Raylib_cs.Raylib;

namespace Rts.Rendering.Raylib;

/// <summary>
/// Draws the world from primitives (ADR-0003): ground plane, rocks, town center cube, villager cylinders, tree cones.
/// Simulation (x, y) becomes render (x, 0, y).
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Raylib draw calls, verified manually (ADR-0013).")]
public static class WorldRenderer
{
    private const float RockHeight = 0.6f;
    private const float TownCenterHeight = 1.5f;
    private const float VillagerRadius = 0.3f;
    private const float VillagerHeight = 0.8f;
    private const float TreeRadius = 0.45f;
    private const float TreeHeight = 1.6f;
    private const int Slices = 12;
    private const float Half = 0.5f;

    private static readonly Color Background = new(30, 34, 40, 255);
    private static readonly Color Ground = new(86, 125, 70, 255);
    private static readonly Color RockColor = new(120, 120, 128, 255);
    private static readonly Color PlayerColor = new(52, 120, 220, 255);
    private static readonly Color TreeColor = new(24, 92, 40, 255);
    private static readonly Color Outline = new(20, 20, 20, 255);

    /// <summary>Clears the frame and draws terrain, entities and selection rings; call between BeginDrawing and EndDrawing.</summary>
    /// <param name="drawIn3D">Extra drawing inside 3D mode, e.g. the debug overlay.</param>
    public static void Draw(SceneFrame frame, CameraView view, Action? drawIn3D = null)
    {
        ArgumentNullException.ThrowIfNull(frame);

        var terrain = frame.Terrain;

        ClearBackground(Background);
        BeginMode3D(RaylibCamera.From(view));

        DrawPlane(new Vector3(terrain.Width * Half, 0, terrain.Height * Half), new Vector2(terrain.Width, terrain.Height), Ground);

        foreach (var rock in terrain.Rocks)
        {
            DrawBox(TileCenter(rock, RockHeight * Half), new Vector3(1, RockHeight, 1), RockColor);
        }

        foreach (var entity in frame.Entities)
        {
            DrawEntity(entity);
        }

        SelectionRenderer.DrawRings(frame);
        drawIn3D?.Invoke();
        EndMode3D();
    }

    private static void DrawEntity(RenderEntity entity)
    {
        var ground = new Vector3(entity.Position.X, 0, entity.Position.Y);

        switch (entity.Kind)
        {
            case EntityKind.TownCenter:
                var size = new Vector3(MapSizes.TownCenterTiles, TownCenterHeight, MapSizes.TownCenterTiles);
                DrawBox(ground with { Y = TownCenterHeight * Half }, size, PlayerColor);
                break;
            case EntityKind.Villager:
                DrawCylinder(ground, VillagerRadius, VillagerRadius, VillagerHeight, Slices, PlayerColor);
                break;
            case EntityKind.Tree:
                DrawCylinder(ground, 0, TreeRadius, TreeHeight, Slices, TreeColor);
                break;
        }
    }

    private static void DrawBox(Vector3 center, Vector3 size, Color color)
    {
        DrawCubeV(center, size, color);
        DrawCubeWiresV(center, size, Outline);
    }

    private static Vector3 TileCenter(GridPosition tile, float y) => new(tile.X + Half, y, tile.Y + Half);
}

/// <summary>Footprint sizes the renderer needs; mirrors the map format (ADR-0011).</summary>
internal static class MapSizes
{
    public const float TownCenterTiles = 3f;
}
