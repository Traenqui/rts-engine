using System.Numerics;
using Rts.Rendering.Contracts;

namespace Rts.Client.Camera;

/// <summary>Map area the camera focus may move in, in tiles.</summary>
public readonly record struct CameraBounds(float Width, float Height);

/// <summary>Camera control intent of one frame.</summary>
/// <param name="Pan">Screen-space direction, X right and Y up, each component in [-1, 1].</param>
/// <param name="Zoom">Mouse wheel notches; positive zooms in.</param>
public readonly record struct CameraInput(Vector2 Pan, float Zoom);

/// <summary>
/// Fixed-pitch RTS camera (ADR-0003): it looks at <see cref="Focus"/> on the ground from the south at
/// <see cref="PitchDegrees"/>. Immutable; <see cref="Update"/> returns the moved rig.
/// </summary>
/// <param name="Focus">Ground point in tiles the camera looks at.</param>
/// <param name="Distance">Distance from camera to focus in tiles.</param>
public sealed record CameraRig(Vector2 Focus, float Distance)
{
    public const float PitchDegrees = 55f;
    public const float MinDistance = 10f;
    public const float MaxDistance = 80f;

    private const float FieldOfViewDegrees = 45f;

    // Start distance per tile of the longer map edge, so small test maps fill the screen as well as large ones.
    private const float FitDistancePerTile = 1f;

    // Pan speed grows with distance so the screen scrolls at a similar rate at every zoom level.
    private const float PanTilesPerSecondPerDistance = 0.5f;
    private const float ZoomDistancePerNotch = 3f;
    private const float DegreesToRadians = MathF.PI / 180f;

    /// <summary>Camera above the map center at a distance that fits the map size.</summary>
    public static CameraRig CenteredOn(CameraBounds bounds)
    {
        var distance = Math.Clamp(Math.Max(bounds.Width, bounds.Height) * FitDistancePerTile, MinDistance, MaxDistance);
        return new CameraRig(new Vector2(bounds.Width / 2, bounds.Height / 2), distance);
    }

    /// <summary>Applies one frame of input; the focus stays inside <paramref name="bounds"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deltaSeconds"/> is negative.</exception>
    public CameraRig Update(CameraInput input, float deltaSeconds, CameraBounds bounds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(deltaSeconds);

        var distance = Math.Clamp(Distance - (input.Zoom * ZoomDistancePerNotch), MinDistance, MaxDistance);

        // Screen up points to row 0, i.e. toward smaller y.
        var panDirection = new Vector2(input.Pan.X, -input.Pan.Y);
        var focus = Focus + (panDirection * PanTilesPerSecondPerDistance * distance * deltaSeconds);
        var clampedFocus = Vector2.Clamp(focus, Vector2.Zero, new Vector2(bounds.Width, bounds.Height));

        return new CameraRig(clampedFocus, distance);
    }

    /// <summary>The render-space view of this rig.</summary>
    public CameraView ToView()
    {
        var pitch = PitchDegrees * DegreesToRadians;
        var target = new Vector3(Focus.X, 0, Focus.Y);
        var offset = new Vector3(0, MathF.Sin(pitch) * Distance, MathF.Cos(pitch) * Distance);
        return new CameraView(target + offset, target, FieldOfViewDegrees);
    }
}
