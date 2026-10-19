using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Raylib_cs;
using Rts.Rendering.Contracts;

namespace Rts.Rendering.Raylib;

/// <summary>Maps the engine's <see cref="CameraView"/> to Raylib's camera type.</summary>
[ExcludeFromCodeCoverage(Justification = "Raylib adapter, verified manually (ADR-0013).")]
internal static class RaylibCamera
{
    public static Camera3D From(CameraView view)
    {
        return new Camera3D(view.Position, view.Target, Vector3.UnitY, view.FieldOfViewDegrees, CameraProjection.Perspective);
    }
}
