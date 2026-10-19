using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Raylib_cs;
using Rts.Client.Camera;
using Rts.Client.Selection;
using static Raylib_cs.Raylib;

namespace Rts.Client.Input;

/// <summary>Thin Raylib adapter that turns keyboard and mouse state into client intents (ADR-0012).</summary>
[ExcludeFromCodeCoverage(Justification = "Raylib input adapter, verified manually (ADR-0013).")]
public static class RaylibInput
{
    /// <summary>WASD or arrow keys pan, the mouse wheel zooms.</summary>
    public static CameraInput ReadCamera()
    {
        var pan = new Vector2(Axis(KeyboardKey.D, KeyboardKey.Right, KeyboardKey.A, KeyboardKey.Left),
            Axis(KeyboardKey.W, KeyboardKey.Up, KeyboardKey.S, KeyboardKey.Down));

        return new CameraInput(pan == Vector2.Zero ? pan : Vector2.Normalize(pan), GetMouseWheelMove());
    }

    /// <summary>Left button selects, right button commands.</summary>
    public static PointerInput ReadPointer() => new(
        GetMousePosition(),
        IsMouseButtonPressed(MouseButton.Left),
        IsMouseButtonReleased(MouseButton.Left),
        IsMouseButtonPressed(MouseButton.Right));

    /// <summary>True in the frame F3 is pressed.</summary>
    public static bool IsDebugOverlayToggled() => IsKeyPressed(KeyboardKey.F3);

    private static float Axis(KeyboardKey positive, KeyboardKey positiveAlternative, KeyboardKey negative, KeyboardKey negativeAlternative)
    {
        var isPositive = IsKeyDown(positive) || IsKeyDown(positiveAlternative);
        var isNegative = IsKeyDown(negative) || IsKeyDown(negativeAlternative);

        if (isPositive == isNegative)
        {
            return 0f;
        }

        return isPositive ? 1f : -1f;
    }
}
