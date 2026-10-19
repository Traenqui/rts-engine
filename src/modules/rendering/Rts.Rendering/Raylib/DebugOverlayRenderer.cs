using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Raylib_cs;
using Rts.Rendering.Scene;
using Rts.Simulation.Contracts;
using static Raylib_cs.Raylib;

namespace Rts.Rendering.Raylib;

/// <summary>F3 debug overlay (PRD US-09): tile grid, current paths and the engine state as text.</summary>
[ExcludeFromCodeCoverage(Justification = "Raylib draw calls, verified manually (ADR-0013).")]
public static class DebugOverlayRenderer
{
    private const float LineHeightAboveGround = 0.02f;
    private const float Half = 0.5f;
    private const int TextLeft = 10;
    private const int TextTop = 10;
    private const int FontSize = 20;
    private const int LineSpacing = 24;

    private static readonly Color GridColor = new(255, 255, 255, 40);
    private static readonly Color PathColor = new(255, 214, 10, 255);
    private static readonly Color TextColor = new(255, 255, 255, 255);
    private static readonly Color TextShadow = new(0, 0, 0, 200);

    /// <summary>Grid lines and paths; call inside 3D mode.</summary>
    public static void Draw3D(TerrainInfo terrain, IReadOnlyList<RenderEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        ArgumentNullException.ThrowIfNull(entities);

        for (var x = 0; x <= terrain.Width; x++)
        {
            DrawLine3D(new Vector3(x, LineHeightAboveGround, 0), new Vector3(x, LineHeightAboveGround, terrain.Height), GridColor);
        }

        for (var y = 0; y <= terrain.Height; y++)
        {
            DrawLine3D(new Vector3(0, LineHeightAboveGround, y), new Vector3(terrain.Width, LineHeightAboveGround, y), GridColor);
        }

        foreach (var entity in entities)
        {
            DrawPath(entity);
        }
    }

    /// <summary>State text in the top left corner; call after 3D mode.</summary>
    public static void Draw2D(IReadOnlyList<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        for (var index = 0; index < lines.Count; index++)
        {
            var y = TextTop + (index * LineSpacing);
            DrawText(lines[index], TextLeft + 1, y + 1, FontSize, TextShadow);
            DrawText(lines[index], TextLeft, y, FontSize, TextColor);
        }
    }

    private static void DrawPath(RenderEntity entity)
    {
        var from = new Vector3(entity.Position.X, LineHeightAboveGround, entity.Position.Y);

        foreach (var waypoint in entity.RemainingPath)
        {
            var to = new Vector3(waypoint.X + Half, LineHeightAboveGround, waypoint.Y + Half);
            DrawLine3D(from, to, PathColor);
            from = to;
        }
    }
}
