using System.Numerics;
using Rts.Client.Camera;

namespace Rts.Client.Tests.Camera;

public sealed class CameraRigTests
{
    private const float Tolerance = 1e-4f;
    private const float OneSecond = 1f;

    private const float MidDistance = 30f;

    private static readonly CameraBounds Bounds = new(Width: 64, Height: 64);

    [Fact]
    public void CenteredOn_StartsInMapCenter()
    {
        var rig = CameraRig.CenteredOn(new CameraBounds(Width: 64, Height: 32));

        Assert.Equal(new Vector2(32, 16), rig.Focus);
    }

    [Theory]
    [InlineData(8, 6, CameraRig.MinDistance)]
    [InlineData(40, 32, 40f)]
    [InlineData(256, 256, CameraRig.MaxDistance)]
    public void CenteredOn_FitsDistanceToLongerMapEdge(float width, float height, float expectedDistance)
    {
        var rig = CameraRig.CenteredOn(new CameraBounds(width, height));

        Assert.Equal(expectedDistance, rig.Distance);
    }

    [Fact]
    public void Update_PanRightAndUp_MovesFocusRightAndTowardTopRow()
    {
        var rig = CameraRig.CenteredOn(Bounds);

        var moved = rig.Update(new CameraInput(Pan: new Vector2(1, 1), Zoom: 0), OneSecond, Bounds);

        Assert.True(moved.Focus.X > rig.Focus.X, "pan right increases x");
        Assert.True(moved.Focus.Y < rig.Focus.Y, "pan up moves toward row 0 at the top of the screen");
    }

    [Fact]
    public void Update_ReturnsNewRigAndKeepsOriginal()
    {
        var rig = CameraRig.CenteredOn(Bounds);

        rig.Update(new CameraInput(new Vector2(1, 0), Zoom: 1), OneSecond, Bounds);

        Assert.Equal(CameraRig.CenteredOn(Bounds), rig);
    }

    [Fact]
    public void Update_PanSpeedScalesWithDistance()
    {
        var near = new CameraRig(new Vector2(32, 32), CameraRig.MinDistance);
        var far = new CameraRig(new Vector2(32, 32), CameraRig.MaxDistance);
        var input = new CameraInput(new Vector2(1, 0), Zoom: 0);
        const float shortFrame = 0.01f;

        var nearStep = near.Update(input, shortFrame, Bounds).Focus.X - near.Focus.X;
        var farStep = far.Update(input, shortFrame, Bounds).Focus.X - far.Focus.X;

        Assert.True(farStep > nearStep, "zoomed out pans faster");
    }

    [Fact]
    public void Update_PanBeyondMapEdge_StopsAtEdge()
    {
        var rig = new CameraRig(new Vector2(1, 1), MidDistance);
        const float longPan = 100f;

        var moved = rig.Update(new CameraInput(new Vector2(-1, 1), Zoom: 0), longPan, Bounds);

        Assert.Equal(Vector2.Zero, moved.Focus);
    }

    [Theory]
    [InlineData(1000f, CameraRig.MinDistance)]
    [InlineData(-1000f, CameraRig.MaxDistance)]
    public void Update_ZoomBeyondLimits_IsClamped(float zoom, float expectedDistance)
    {
        var rig = CameraRig.CenteredOn(Bounds);

        var zoomed = rig.Update(new CameraInput(Vector2.Zero, zoom), OneSecond, Bounds);

        Assert.Equal(expectedDistance, zoomed.Distance);
    }

    [Fact]
    public void Update_ZoomIn_ReducesDistance()
    {
        var rig = CameraRig.CenteredOn(Bounds);

        var zoomed = rig.Update(new CameraInput(Vector2.Zero, Zoom: 1), OneSecond, Bounds);

        Assert.True(zoomed.Distance < rig.Distance);
    }

    [Fact]
    public void Update_NegativeDelta_Throws()
    {
        var rig = CameraRig.CenteredOn(Bounds);

        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Update(new CameraInput(Vector2.Zero, 0), -1f, Bounds));
    }

    [Fact]
    public void ToView_LooksAtFocusOnGroundFromAboveAndSouth()
    {
        var rig = new CameraRig(new Vector2(10, 20), Distance: 30);

        var view = rig.ToView();

        Assert.Equal(new Vector3(10, 0, 20), view.Target);
        Assert.Equal(30f, Vector3.Distance(view.Position, view.Target), Tolerance);
        Assert.True(view.Position.Z > view.Target.Z, "camera sits south of its focus so row 0 is at the top");

        var pitch = MathF.Asin((view.Position.Y - view.Target.Y) / 30f) * 180f / MathF.PI;
        Assert.Equal(CameraRig.PitchDegrees, pitch, Tolerance * 100);
    }
}
