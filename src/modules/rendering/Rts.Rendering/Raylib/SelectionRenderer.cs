using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Raylib_cs;
using Rts.Rendering.Contracts;
using Rts.Rendering.Scene;
using Rts.Simulation.Contracts;
using static Raylib_cs.Raylib;

namespace Rts.Rendering.Raylib;

/// <summary>Selection rings on the ground and the selection box (PRD US-03).</summary>
[ExcludeFromCodeCoverage(Justification = "Raylib draw calls, verified manually (ADR-0013).")]
public static class SelectionRenderer
{
    private const float RingHeightAboveGround = 0.03f;
    private const float VillagerRingRadius = 0.5f;
    private const float TownCenterRingRadius = 2.2f;
    private const float BoxOutlineThickness = 1f;

    // DrawCircle3D draws in the XY plane; a quarter turn around X lays the ring flat on the ground.
    private const float FlatOnGroundDegrees = 90f;

    private static readonly Color RingColor = new(255, 255, 255, 230);
    private static readonly Color BoxFill = new(255, 255, 255, 40);
    private static readonly Color BoxOutline = new(255, 255, 255, 200);

    /// <summary>A ring around every selected entity; call inside 3D mode.</summary>
    public static void DrawRings(SceneFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        foreach (var entity in frame.Entities.Where(entity => frame.Selected.Contains(entity.Id)))
        {
            var radius = entity.Kind == EntityKind.TownCenter ? TownCenterRingRadius : VillagerRingRadius;
            var center = new Vector3(entity.Position.X, RingHeightAboveGround, entity.Position.Y);
            DrawCircle3D(center, radius, Vector3.UnitX, FlatOnGroundDegrees, RingColor);
        }
    }

    /// <summary>The selection box while dragging; call after 3D mode.</summary>
    public static void DrawBox(ScreenRectangle box)
    {
        var bounds = new Rectangle(box.Min, box.Size);
        DrawRectangleRec(bounds, BoxFill);
        DrawRectangleLinesEx(bounds, BoxOutlineThickness, BoxOutline);
    }
}
