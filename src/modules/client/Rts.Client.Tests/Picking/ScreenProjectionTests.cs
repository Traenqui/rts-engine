using System.Numerics;
using Rts.Client.Camera;
using Rts.Client.Picking;
using Rts.Rendering.Contracts;

namespace Rts.Client.Tests.Picking;

public sealed class ScreenProjectionTests
{
    private const float Tolerance = 1e-3f;
    private const float PixelTolerance = 0.05f;

    private static readonly Vector2 Screen = new(1280, 720);
    private static readonly CameraRig Rig = CameraRig.CenteredOn(new CameraBounds(Width: 16, Height: 16));
    private static readonly ScreenProjection Projection = new(Rig.ToView(), Screen);

    [Fact]
    public void GroundAt_ScreenCenter_HitsCameraFocus()
    {
        var ground = Projection.GroundAt(Screen / 2);

        Assert.NotNull(ground);
        Assert.Equal(Rig.Focus.X, ground.Value.X, Tolerance);
        Assert.Equal(Rig.Focus.Y, ground.Value.Y, Tolerance);
    }

    [Fact]
    public void ToScreen_CameraFocus_IsScreenCenter()
    {
        var screen = Projection.ToScreen(new Vector3(Rig.Focus.X, 0, Rig.Focus.Y));

        Assert.NotNull(screen);
        Assert.Equal(Screen.X / 2, screen.Value.X, PixelTolerance);
        Assert.Equal(Screen.Y / 2, screen.Value.Y, PixelTolerance);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1280, 720, 0)]
    [InlineData(100, 600, 0)]
    [InlineData(900, 200, 0.4f)]
    [InlineData(640, 360, 1.5f)]
    public void GroundAt_RoundTripsThroughToScreen(float x, float y, float height)
    {
        var ground = Projection.GroundAt(new Vector2(x, y), height);

        Assert.NotNull(ground);
        var screen = Projection.ToScreen(new Vector3(ground.Value.X, height, ground.Value.Y));
        Assert.NotNull(screen);
        Assert.Equal(x, screen.Value.X, PixelTolerance);
        Assert.Equal(y, screen.Value.Y, PixelTolerance);
    }

    [Fact]
    public void GroundAt_ScreenUpAndRight_PointsTowardTopRowAndLargerX()
    {
        var center = Projection.GroundAt(Screen / 2)!.Value;

        var top = Projection.GroundAt(Screen with { X = Screen.X / 2, Y = 0 })!.Value;
        var right = Projection.GroundAt(Screen with { Y = Screen.Y / 2 })!.Value;

        Assert.True(top.Y < center.Y, $"top {top} should have a smaller row than center {center}");
        Assert.True(right.X > center.X, $"right {right} should have a larger column than center {center}");
    }

    [Fact]
    public void GroundAt_RayNotPointingDown_ReturnsNull()
    {
        var horizontal = new ScreenProjection(new CameraView(new Vector3(0, 1, 0), new Vector3(0, 1, -10), 45f), Screen);

        Assert.Null(horizontal.GroundAt(new Vector2(640, 0)));
    }

    [Fact]
    public void ToScreen_PointBehindCamera_ReturnsNull()
    {
        var view = Rig.ToView();
        var behind = view.Position + (view.Position - view.Target);

        Assert.Null(Projection.ToScreen(behind));
    }

    [Theory]
    [InlineData(0, 720)]
    [InlineData(1280, 0)]
    [InlineData(-1, 720)]
    public void Constructor_NonPositiveScreenSize_Throws(float width, float height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScreenProjection(Rig.ToView(), new Vector2(width, height)));
    }
}
